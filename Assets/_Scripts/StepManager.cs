using UnityEngine;
using TMPro; // Needed to update your screen text
using Firebase.Database;
using Firebase.Auth;
using Mapbox.Unity.Location;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class StepManager : MonoBehaviour
{
    [Header("Step Data")]
    public int currentDailySteps = 0;
    public int currentWeeklySteps = 0;
    public int totalLifetimeSteps = 0;

    [Header("Pedometer Sensitivity")]
    [Tooltip("How hard the phone needs to shake to count a step. Default gravity is 1.0.")]
    public float stepThreshold = 1.5f; 
    [Tooltip("Phone must settle below this before counting the next step to prevent double-counting.")]
    public float resetThreshold = 1.0f; 

    [Header("Anti-Cheat System")]
    [Tooltip("Max physical speed allowed (m/s). 5m/s = 18km/h. Faster than this ignores steps (Vehicle detection).")]
    public float maxAllowedSpeed = 5.0f;
    [Tooltip("Violent shaking detection. Spikes above this value are ignored.")]
    public float maxShakeThreshold = 3.0f;
    [Tooltip("Minimum time (seconds) between steps. Stops rapid shaking exploits.")]
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
    public float sessionDistanceMeters = 0f; // Track GPS distance to prove they actually walked

    [Header("UI Elements")]
    [SerializeField] private TMP_Text stepTextDisplay; // Assign your UI text slot here!

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllNotifications();
#endif

        // Force the app to stay alive when they minimize it!
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 1. Initialize Firebase connection strings safely
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        // 2. Load cached historical records from the phone's storage
        currentDailySteps = PlayerPrefs.GetInt("DailySteps", 0);
        currentWeeklySteps = PlayerPrefs.GetInt("WeeklySteps", 0);
        totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 0);

        // 3. Render the correct initial value on screen immediately
        UpdateStepUI();

        // 4. Hook into Mapbox for GPS Anti-Cheat
        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }
    }

    void Update()
    {
        // --- PC TESTING MODE (The Spacebar) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegisterStep();
        }

        // --- MOBILE HARDWARE MODE (The Accelerometer) ---
        float acceleration = Input.acceleration.magnitude;

        // --- ANTI-CHEAT: Calculate GPS Speed ---
        if (_locationProvider != null && _locationProvider.CurrentLocation.LatitudeLongitude != Vector2d.zero)
        {
            Vector2d currentGPS = _locationProvider.CurrentLocation.LatitudeLongitude;
            if (_lastGPSPos != Vector2d.zero)
            {
                // Only check speed every 1 second to get a stable reading
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

        // --- ANTI-CHEAT: Analyze Acceleration ---
        // 1. Must be above Step Threshold, but below Shake Threshold (violent shaking)
        if (acceleration > stepThreshold && acceleration < maxShakeThreshold && isStepReady)
        {
            // 2. Cooldown timer to prevent rapid shaking
            if (Time.time - _lastStepTime >= minTimeBetweenSteps)
            {
                // 3. GPS Vehicle check (Are they in a car?)
                if (_currentSpeedMPS <= maxAllowedSpeed)
                {
                    RegisterStep();
                    _lastStepTime = Time.time;
                    isStepReady = false; // Lock tracking loop frame execution
                }
            }
        }
        
        if (acceleration < resetThreshold)
        {
            isStepReady = true; // Settle state reached, unlock loop
        }
    }

    // This runs automatically when the user minimizes the app (presses the home button)
    void OnApplicationPause(bool isPaused)
    {
#if UNITY_ANDROID
        if (isPaused)
        {
            // They went to the home screen! Send a notification with their steps.
            SendStepNotification();
        }
        else
        {
            // They came back! Clear the notification.
            AndroidNotificationCenter.CancelNotification(777); 
        }
#endif
    }

    // This runs when the app is completely closed (swiped away from recents)
    void OnApplicationQuit()
    {
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
            Importance = Importance.Low, // Low importance so it doesn't constantly beep, just sits in the tray
            Description = "Tracks your steps while the app is minimized."
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        var notification = new AndroidNotification();
        notification.Title = "Step-Up is tracking!";
        notification.Text = $"You have taken {currentDailySteps} steps today. Keep going!";
        notification.FireTime = System.DateTime.Now;
        notification.SmallIcon = "icon"; // Uses default app icon

        // Sending with a specific ID (777) so we can update it or cancel it later
        AndroidNotificationCenter.SendNotificationWithExplicitID(notification, "step_tracker_background", 777);
    }
#endif

    void RegisterStep()
    {
        currentDailySteps++;
        currentWeeklySteps++;
        totalLifetimeSteps++;

        if (isSessionActive)
        {
            sessionSteps++;
        }

        // Save progress locally onto the hardware storage layer
        PlayerPrefs.SetInt("DailySteps", currentDailySteps);
        PlayerPrefs.SetInt("WeeklySteps", currentWeeklySteps);
        PlayerPrefs.SetInt("TotalLifetimeSteps", totalLifetimeSteps);
        PlayerPrefs.Save();
        
        // Update the user screen interface elements
        UpdateStepUI();

        // Sync live data up to your Firebase Database Tree matching LeaderboardManager's query
        SyncStepsToFirebase();

#if UNITY_ANDROID
        // If they are walking while the app is minimized, update the notification silently!
        // (We don't know for sure if it's minimized here, but sending it again updates the existing one)
        // We only do this every 10 steps so we don't spam the Android OS battery
        if (currentDailySteps % 10 == 0)
        {
            SendStepNotification();
        }
#endif

        Debug.Log($"Step Tracked! Daily: {currentDailySteps} | Lifetime: {totalLifetimeSteps}");
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
            // Updates 'TotalLifetimeSteps' under 'users/uid/' node
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