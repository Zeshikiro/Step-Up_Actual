using UnityEngine;
using TMPro; 
using Firebase.Database;
using Firebase.Auth;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System;
using System.Collections.Generic;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class StepManager : MonoBehaviour
{
    [Header("Step Data")]
    public int currentDailySteps = 0;
    public int currentWeeklySteps = 0;
    public int totalLifetimeSteps = 0;

    [Header("Advanced Mission Tracking")]
    public int yesterdaysSteps = 0;
    public int arStepsToday = 0;
    public int totalWeeklyARSteps = 0;
    public int daysGoalMetThisWeek = 0;
    public List<int> completedSessionsToday = new List<int>();
    public int maxSessionStepsToday = 0;
    public float maxContinuousWalkMinutes = 0f;

    // Auto-Session Detection variables
    private float lastWalkingTime = 0f;
    private int autoSessionSteps = 0;
    private float autoSessionStartTime = 0f;
    private bool inAutoSession = false;

    [Header("Pedometer Sensitivity")]
    public float stepThreshold = 1.5f; 
    public float resetThreshold = 1.0f; 

    [Header("Anti-Cheat System")]
    public float maxAllowedSpeed = 5.0f;
    public float maxShakeThreshold = 3.0f;
    public float minTimeBetweenSteps = 0.3f;
    
    private float _lastStepTime = 0f;
    private bool isStepReady = true;

    // GPS Speed Tracking
    private ILocationProvider _locationProvider;
    private Vector2d _lastGPSPos;
    private float _lastGPSTime;
    private float _currentSpeedMPS = 0f;
    public float CurrentSpeedMPS { get { return _currentSpeedMPS; } }

    [Header("Mission Active Session (Anti-Cheat)")]
    public bool isSessionActive = false;
    public int sessionSteps = 0;
    public float sessionStartTime = 0f;
    public float sessionDurationMinutes = 0f;
    public float sessionDistanceMeters = 0f; 

    [Header("UI Elements")]
    [SerializeField] private TMP_Text stepTextDisplay; 

    private DatabaseReference dbReference;
    private string userId;
    private ARManager arManager;

    void Start()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#endif
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        arManager = FindFirstObjectByType<ARManager>();

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        PerformDateRolloverCheck();

        // Render the correct initial value on screen immediately
        UpdateStepUI();

        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }
    }

    private void PerformDateRolloverCheck()
    {
        string lastDate = PlayerPrefs.GetString("LastLoginDate", "");
        string todayDate = System.DateTime.Now.ToString("yyyyMMdd");
        
        currentDailySteps = PlayerPrefs.GetInt("DailySteps", 0);
        currentWeeklySteps = PlayerPrefs.GetInt("WeeklySteps", 0);
        totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 0);

        if (lastDate != todayDate && !string.IsNullOrEmpty(lastDate))
        {
            // Rollover!
            yesterdaysSteps = currentDailySteps;
            PlayerPrefs.SetInt("YesterdaysSteps", yesterdaysSteps);
            
            // Check if they met yesterday's goal before resetting
            int dailyGoal = PlayerPrefs.GetInt("DailyStepGoal", 10000);
            if (currentDailySteps >= dailyGoal) 
            {
                daysGoalMetThisWeek = PlayerPrefs.GetInt("DaysGoalMetThisWeek", 0) + 1;
                PlayerPrefs.SetInt("DaysGoalMetThisWeek", daysGoalMetThisWeek);
            }

            // Reset Daily Stats
            currentDailySteps = 0;
            arStepsToday = 0;
            maxSessionStepsToday = 0;
            maxContinuousWalkMinutes = 0f;
            PlayerPrefs.SetString("CompletedSessions", ""); // Clear sessions
            
            // Weekly check (if it's Monday)
            if (System.DateTime.Today.DayOfWeek == DayOfWeek.Monday)
            {
                currentWeeklySteps = 0;
                totalWeeklyARSteps = 0;
                daysGoalMetThisWeek = 0;
                PlayerPrefs.SetInt("DaysGoalMetThisWeek", 0);
                PlayerPrefs.SetInt("WeeklyARSteps", 0);
            }

            SaveAllProgress();
            PlayerPrefs.SetString("LastLoginDate", todayDate);
        }
        else
        {
            if (string.IsNullOrEmpty(lastDate)) PlayerPrefs.SetString("LastLoginDate", todayDate);
            
            yesterdaysSteps = PlayerPrefs.GetInt("YesterdaysSteps", 0);
            daysGoalMetThisWeek = PlayerPrefs.GetInt("DaysGoalMetThisWeek", 0);
            arStepsToday = PlayerPrefs.GetInt("ARStepsToday", 0);
            totalWeeklyARSteps = PlayerPrefs.GetInt("WeeklyARSteps", 0);
            maxSessionStepsToday = PlayerPrefs.GetInt("MaxSessionStepsToday", 0);
            maxContinuousWalkMinutes = PlayerPrefs.GetFloat("MaxContinuousWalkMinutes", 0f);
            
            string sessions = PlayerPrefs.GetString("CompletedSessions", "");
            if (!string.IsNullOrEmpty(sessions))
            {
                string[] parts = sessions.Split(',');
                foreach(string p in parts) {
                    if (int.TryParse(p, out int val)) completedSessionsToday.Add(val);
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegisterStep();
        }

        float acceleration = Input.acceleration.magnitude;

        // GPS Speed Tracker
        if (_locationProvider != null && _locationProvider.CurrentLocation.LatitudeLongitude != Vector2d.zero)
        {
            Vector2d currentGPS = _locationProvider.CurrentLocation.LatitudeLongitude;
            if (_lastGPSPos != Vector2d.zero)
            {
                float timeDelta = Time.time - _lastGPSTime;
                if (timeDelta >= 1.0f)
                {
                    Vector2d posAMeters = Conversions.LatLonToMeters(_lastGPSPos.x, _lastGPSPos.y);
                    Vector2d posBMeters = Conversions.LatLonToMeters(currentGPS.x, currentGPS.y);
                    double distanceMeters = Vector2d.Distance(posAMeters, posBMeters);
                    
                    if (isSessionActive) sessionDistanceMeters += (float)distanceMeters;

                    _currentSpeedMPS = (float)(distanceMeters / timeDelta);
                    _lastGPSPos = currentGPS;
                    _lastGPSTime = Time.time;
                }
            }
            else
            {
                _lastGPSPos = currentGPS;
                _lastGPSTime = Time.time;
            }
        }

        if (acceleration > stepThreshold && acceleration < maxShakeThreshold && isStepReady)
        {
            if (Time.time - _lastStepTime >= minTimeBetweenSteps)
            {
                if (_currentSpeedMPS <= maxAllowedSpeed)
                {
                    RegisterStep();
                    _lastStepTime = Time.time;
                    isStepReady = false; 
                }
            }
        }
        
        if (acceleration < resetThreshold)
        {
            isStepReady = true; 
        }

        // Auto-Session Detection logic
        if (inAutoSession)
        {
            // If they haven't walked for 2 minutes, the session ends
            if (Time.time - lastWalkingTime > 120f)
            {
                EndAutoSession();
            }
        }
    }

    private void EndAutoSession()
    {
        if (!inAutoSession) return;
        
        float durationMins = (lastWalkingTime - autoSessionStartTime) / 60f;
        if (durationMins > maxContinuousWalkMinutes) maxContinuousWalkMinutes = durationMins;
        
        if (autoSessionSteps > maxSessionStepsToday) maxSessionStepsToday = autoSessionSteps;
        
        // Only save meaningful sessions (e.g., > 100 steps)
        if (autoSessionSteps >= 100)
        {
            completedSessionsToday.Add(autoSessionSteps);
            PlayerPrefs.SetString("CompletedSessions", string.Join(",", completedSessionsToday));
        }

        PlayerPrefs.SetInt("MaxSessionStepsToday", maxSessionStepsToday);
        PlayerPrefs.SetFloat("MaxContinuousWalkMinutes", maxContinuousWalkMinutes);
        
        inAutoSession = false;
        autoSessionSteps = 0;
    }

    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            EndAutoSession(); // End session safely if they background the app
#if UNITY_ANDROID
            SendStepNotification();
#endif
            SaveAllProgress();
        }
        else
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(777); 
#endif
            PerformDateRolloverCheck(); // Check if day changed while suspended
        }
    }

    void OnApplicationQuit()
    {
        EndAutoSession();
        SaveAllProgress();
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#endif
    }

#if UNITY_ANDROID
    private void SendStepNotification()
    {
        var channel = new AndroidNotificationChannel()
        {
            Id = "step_tracker_background",
            Name = "Background Step Tracker",
            Importance = Importance.Low, 
            Description = "Tracks your steps while the app is minimized."
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification();
        notification.Title = "Step-Up is tracking!";
        notification.Text = $"You have taken {currentDailySteps} steps today. Keep going!";
        notification.FireTime = System.DateTime.Now;
        notification.SmallIcon = "icon"; 

        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "step_tracker_background", 777);
    }
#endif

    void RegisterStep()
    {
        currentDailySteps++;
        currentWeeklySteps++;
        totalLifetimeSteps++;

        if (arManager != null && arManager.IsARMode)
        {
            arStepsToday++;
            totalWeeklyARSteps++;
        }

        if (isSessionActive)
        {
            sessionSteps++;
        }

        // Auto Session tracking
        if (!inAutoSession)
        {
            inAutoSession = true;
            autoSessionStartTime = Time.time;
            autoSessionSteps = 0;
        }
        autoSessionSteps++;
        lastWalkingTime = Time.time;

        if (currentDailySteps % 50 == 0) SaveAllProgress(); // Periodically save
        
        UpdateStepUI();
        SyncStepsToFirebase();

#if UNITY_ANDROID
        if (currentDailySteps % 10 == 0)
        {
            SendStepNotification();
        }
#endif
    }

    private void SaveAllProgress()
    {
        PlayerPrefs.SetInt("DailySteps", currentDailySteps);
        PlayerPrefs.SetInt("WeeklySteps", currentWeeklySteps);
        PlayerPrefs.SetInt("TotalLifetimeSteps", totalLifetimeSteps);
        PlayerPrefs.SetInt("ARStepsToday", arStepsToday);
        PlayerPrefs.SetInt("WeeklyARSteps", totalWeeklyARSteps);
        PlayerPrefs.Save();
    }

    private void UpdateStepUI()
    {
        if (stepTextDisplay != null)
        {
            stepTextDisplay.text = "Steps: " + totalLifetimeSteps.ToString("N0");
        }
    }

    private void SyncStepsToFirebase()
    {
        if (dbReference != null && !string.IsNullOrEmpty(userId))
        {
            dbReference.Child("users").Child(userId).Child("TotalLifetimeSteps").SetValueAsync(totalLifetimeSteps);
        }
    }

    public void StartMissionSession(float durationMinutes)
    {
        isSessionActive = true;
        sessionSteps = 0;
        sessionDistanceMeters = 0f;
        sessionDurationMinutes = durationMinutes;
        sessionStartTime = Time.time;
        Debug.Log($"[StepManager] Started a {durationMinutes}-minute mission!");
    }
    
    public void StopMissionSession()
    {
        isSessionActive = false;
        Debug.Log($"[StepManager] Mission Stopped. Total Steps: {sessionSteps}, Total Dist: {sessionDistanceMeters}m");
    }
}