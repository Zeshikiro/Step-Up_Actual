using UnityEngine;
using TMPro; // Needed to update your screen text
using Firebase.Database;
using Firebase.Auth;

public class StepManager : MonoBehaviour
{
    [Header("Step Data")]
    public int currentDailySteps = 0;
    public int totalLifetimeSteps = 0;

    [Header("Pedometer Sensitivity")]
    [Tooltip("How hard the phone needs to shake to count a step. Default gravity is 1.0.")]
    public float stepThreshold = 1.5f; 
    [Tooltip("Phone must settle below this before counting the next step to prevent double-counting.")]
    public float resetThreshold = 1.0f; 
    
    private bool isStepReady = true;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text stepTextDisplay; // Assign your UI text slot here!

    private DatabaseReference dbReference;
    private string userId;

    void Start()
    {
        // 1. Initialize Firebase connection strings safely
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        }

        // 2. Load cached historical records from the phone's storage
        currentDailySteps = PlayerPrefs.GetInt("DailySteps", 0);
        totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 0);

        // 3. Render the correct initial value on screen immediately
        UpdateStepUI();
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

        if (acceleration > stepThreshold && isStepReady)
        {
            RegisterStep();
            isStepReady = false; // Lock tracking loop frame execution
        }
        
        if (acceleration < resetThreshold)
        {
            isStepReady = true; // Settle state reached, unlock loop
        }
    }

    void RegisterStep()
    {
        currentDailySteps++;
        totalLifetimeSteps++;

        // Save progress locally onto the hardware storage layer
        PlayerPrefs.SetInt("DailySteps", currentDailySteps);
        PlayerPrefs.SetInt("TotalLifetimeSteps", totalLifetimeSteps);
        PlayerPrefs.Save();
        
        // Update the user screen interface elements
        UpdateStepUI();

        // Sync live data up to your Firebase Database Tree matching LeaderboardManager's query
        SyncStepsToFirebase();

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
}