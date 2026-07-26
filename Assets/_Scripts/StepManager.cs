using UnityEngine;
using TMPro; 
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
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
    private System.DateTime lastWalkingTime = System.DateTime.Now;
    private int autoSessionSteps = 0;
    private System.DateTime autoSessionStartTime = System.DateTime.Now;
    private bool inAutoSession = false;

    [Header("Hardware Pedometer")]
    private int baselineHardwareSteps = -1;
    private int lastHardwareStepCount = -1;
    private bool hasPermission = false;

    // GPS Speed Tracking
    private ILocationProvider _locationProvider;
    private Vector2d _lastGPSPos;
    private float _lastGPSTime;
    private float _currentSpeedMPS = 0f;
    public float CurrentSpeedMPS { get { return _currentSpeedMPS; } }

    [Header("Mission Active Session (Anti-Cheat)")]
    public bool isSessionActive = false;
    public int sessionSteps = 0;
    public System.DateTime sessionStartTime = System.DateTime.Now;
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
        AndroidNotificationCenter.CancelScheduledNotification(888); // Cancel short-term reminder
        AndroidNotificationCenter.CancelScheduledNotification(999); // Cancel long-term reminder
#endif

        baselineHardwareSteps = PlayerPrefs.GetInt("BaselineHardwareSteps", -1);
        lastHardwareStepCount = PlayerPrefs.GetInt("LastHardwareStepCount", -1);

        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        arManager = FindFirstObjectByType<ARManager>();

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                if (FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                    dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                }
            }
        });

        PerformDateRolloverCheck();

        // Render the correct initial value on screen immediately
        UpdateStepUI();

        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }
    }

    public void InitializeSensors()
    {
        // Explicitly add the StepCounter device to fix missing sensors on some Android phones
        if (StepCounter.current == null)
        {
            InputSystem.AddDevice<StepCounter>();
        }

        if (StepCounter.current != null)
        {
            InputSystem.EnableDevice(StepCounter.current);
            // Force the pedometer to poll faster to reduce the 5-second delay!
            StepCounter.current.samplingFrequency = 30;
            Debug.Log("[StepManager] StepCounter explicitly added and enabled.");
        }
        else
        {
            Debug.LogWarning("[StepManager] StepCounter could not be added. Device might not have a hardware pedometer.");
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

            // RESET ALL DAILY MISSIONS (0 to 6)
            for (int i = 0; i <= 6; i++)
            {
                PlayerPrefs.DeleteKey("MissionClaimed_" + i);
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

                // RESET ALL WEEKLY MISSIONS (7 to 9)
                for (int i = 7; i <= 9; i++)
                {
                    PlayerPrefs.DeleteKey("MissionClaimed_" + i);
                }
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
        // Debug: Press Space to simulate a step (New Input System safe)
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            RegisterStep();
        }

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

#if UNITY_ANDROID
        if (!hasPermission && Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
        {
            hasPermission = true;
            if (StepCounter.current != null) InputSystem.EnableDevice(StepCounter.current);
        }
#else
        hasPermission = true; 
#endif

        if (StepCounter.current != null && StepCounter.current.enabled && hasPermission)
        {
            int currentHardwareSteps = StepCounter.current.stepCounter.ReadValue();
            
            // ANTI-CHEAT: If moving faster than 6 meters/second (13 mph), you are in a car! Ignore steps!
            if (_currentSpeedMPS > 6.0f && lastHardwareStepCount != -1)
            {
                // Constantly update the baseline so we don't accidentally award these "car steps" when they slow down
                lastHardwareStepCount = currentHardwareSteps;
                PlayerPrefs.SetInt("LastHardwareStepCount", lastHardwareStepCount);
            }
            else if (currentHardwareSteps > 0)
            {
                if (baselineHardwareSteps == -1 || (lastHardwareStepCount != -1 && currentHardwareSteps < lastHardwareStepCount))
                {
                    // Initialization or phone was rebooted (sensor reset to 0)
                    baselineHardwareSteps = currentHardwareSteps;
                    PlayerPrefs.SetInt("BaselineHardwareSteps", baselineHardwareSteps);
                    lastHardwareStepCount = currentHardwareSteps;
                    PlayerPrefs.SetInt("LastHardwareStepCount", lastHardwareStepCount);
                    PlayerPrefs.Save();
                }
                else if (lastHardwareStepCount == -1)
                {
                    // App just opened, but phone was NOT rebooted. Grant offline steps!
                    int savedLastCount = PlayerPrefs.GetInt("LastHardwareStepCount", currentHardwareSteps);
                    if (currentHardwareSteps >= savedLastCount)
                    {
                        int offlineSteps = currentHardwareSteps - savedLastCount;
                        if (offlineSteps > 0 && offlineSteps < 50000) 
                        {
                            for (int i = 0; i < offlineSteps; i++) RegisterStep();
                        }
                    }
                    lastHardwareStepCount = currentHardwareSteps;
                    PlayerPrefs.SetInt("LastHardwareStepCount", lastHardwareStepCount);
                }
                else
                {
                    // Normal active session
                    int stepsTaken = currentHardwareSteps - lastHardwareStepCount;
                    if (stepsTaken > 0)
                    {
                        for (int i = 0; i < stepsTaken; i++) RegisterStep();
                        lastHardwareStepCount = currentHardwareSteps;
                        PlayerPrefs.SetInt("LastHardwareStepCount", lastHardwareStepCount);
                    }
                }
            }
        }

        // Auto-Session Detection logic
        if (inAutoSession)
        {
            // If they haven't walked for 2 minutes, the session ends
            if ((System.DateTime.Now - lastWalkingTime).TotalSeconds > 120f)
            {
                EndAutoSession();
            }
        }
    }

    private void EndAutoSession()
    {
        if (!inAutoSession) return;
        
        float durationMins = (float)(lastWalkingTime - autoSessionStartTime).TotalMinutes;
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
            // We no longer call EndAutoSession here! This allows the session to continue while backgrounded!
#if UNITY_ANDROID
            SendStepNotification();
            ScheduleReminders();
#endif
            SaveAllProgress();
        }
        else
        {
#if UNITY_ANDROID
            AndroidNotificationCenter.CancelNotification(777); 
            AndroidNotificationCenter.CancelScheduledNotification(888); // Cancel short-term reminder
            AndroidNotificationCenter.CancelScheduledNotification(999); // Cancel long-term reminder
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
        ScheduleReminders(); // Ensure reminders are scheduled even if fully quit
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

    private void ScheduleReminders()
    {
        // 1. Short-term reminder (Every 4 hours while in background)
        var shortReminder = new AndroidNotification();
        shortReminder.Title = "Step-Up is waiting!";
        shortReminder.Text = "Don't forget to complete your walking missions today!";
        shortReminder.FireTime = System.DateTime.Now.AddHours(4);
        shortReminder.RepeatInterval = System.TimeSpan.FromHours(4);
        shortReminder.SmallIcon = "icon"; 
        AndroidNotificationCenter.SendNotificationWithExplicitID(shortReminder, "step_tracker_background", 888);

        // 2. Long-term reminder (Every 4 days to bring user back)
        var longReminder = new AndroidNotification();
        longReminder.Title = "We miss you on Step-Up!";
        longReminder.Text = "It's been a while! Come back and continue your fitness journey!";
        longReminder.FireTime = System.DateTime.Now.AddDays(4);
        longReminder.RepeatInterval = System.TimeSpan.FromDays(4);
        longReminder.SmallIcon = "icon"; 
        AndroidNotificationCenter.SendNotificationWithExplicitID(longReminder, "step_tracker_background", 999);
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
            autoSessionStartTime = System.DateTime.Now;
            autoSessionSteps = 0;
        }
        autoSessionSteps++;
        lastWalkingTime = System.DateTime.Now;

        UpdateStepUI();

        if (currentDailySteps % 50 == 0) 
        {
            SaveAllProgress(); // Periodically save local and cloud data
        }
        
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

        // ONLY sync to Firebase when saving to prevent massive network lag on every step!
        SyncStepsToFirebase();
    }

    private void UpdateStepUI()
    {
        if (stepTextDisplay != null)
        {
            stepTextDisplay.text = currentDailySteps.ToString("N0"); // HUD NOW SHOWS DAILY STEPS!
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
        sessionStartTime = System.DateTime.Now;
        Debug.Log($"[StepManager] Started a {durationMinutes}-minute mission!");
    }
    
    public void StopMissionSession()
    {
        isSessionActive = false;
        Debug.Log($"[StepManager] Mission Stopped. Total Steps: {sessionSteps}, Total Dist: {sessionDistanceMeters}m");
    }
}