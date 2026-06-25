using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivitySummaryManager : MonoBehaviour
{
    [Header("--- Top Section Components ---")]
    [SerializeField] private TextMeshProUGUI streakTxt;
    [SerializeField] private TextMeshProUGUI calorieTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;

    [Header("--- Middle Section Cards ---")]
    [SerializeField] private TextMeshProUGUI stepCountTxt;
    [SerializeField] private TextMeshProUGUI distanceTxt;

    [Header("--- Navigation & Panel References ---")]
    [SerializeField] private Button viewLeaderboardBtn;
    [SerializeField] private Button shareProgressBtn;
    [SerializeField] private GameObject leaderboardPanel;

    [Header("--- Core Tracking System References ---")]
    [SerializeField] private StepManager stepManager; 
    [SerializeField] private UserData userData; // Safe singular declaration

    [Header("--- Data Configurations & Targets ---")]
    [SerializeField] private int dailyStepGoal = 10000;
    [SerializeField] private float caloriesPerStep = 0.04f; 
    [SerializeField] private float stepStrideLengthMeters = 0.75f; 

    private void OnEnable()
    {
        RefreshSummaryDashboard();
    }

    private void Start()
    {
        if (viewLeaderboardBtn != null)
            viewLeaderboardBtn.onClick.AddListener(OnViewLeaderboardClicked);

        if (shareProgressBtn != null)
            shareProgressBtn.onClick.AddListener(OnShareProgressClicked);
    }

    public void RefreshSummaryDashboard()
    {
        int currentSteps = 0;
        int actualStreak = 0;

        if (stepManager != null)
        {
            currentSteps = Mathf.RoundToInt(stepManager.currentDailySteps);
        }

        if (userData != null)
        {
            actualStreak = userData.currentStreak; 
        }

        float calculatedCalories = currentSteps * caloriesPerStep;
        float calculatedDistanceKm = (currentSteps * stepStrideLengthMeters) / 1000f;
        float estimatedSpeed = currentSteps > 0 ? 4.5f : 0.0f;

        // --- Dynamic Counter Assignments ---
        
        // Streak
        if (streakTxt != null) {
            if (streakTxt.TryGetComponent(out UINumberCounter streakCounter)) streakCounter.CountTo(actualStreak);
            else streakTxt.text = actualStreak.ToString();
        }

        // Calories
        if (calorieTxt != null) {
            if (calorieTxt.TryGetComponent(out UINumberCounter calCounter)) calCounter.CountTo(Mathf.RoundToInt(calculatedCalories));
            else calorieTxt.text = calculatedCalories.ToString("F0");
        }

        // Speed (Float)
        if (speedTxt != null) {
            if (speedTxt.TryGetComponent(out UINumberCounter speedCounter)) speedCounter.CountToFloat(estimatedSpeed, 1.0f, "F1");
            else speedTxt.text = estimatedSpeed.ToString("F1");
        }

        // Steps
        if (stepCountTxt != null) {
            if (stepCountTxt.TryGetComponent(out UINumberCounter stepCounter)) stepCounter.CountTo(currentSteps);
            else stepCountTxt.text = currentSteps.ToString("N0");
        }

        // Distance (Float)
        if (distanceTxt != null) {
            if (distanceTxt.TryGetComponent(out UINumberCounter distCounter)) distCounter.CountToFloat(calculatedDistanceKm, 1.0f, "F2");
            else distanceTxt.text = calculatedDistanceKm.ToString("F2");
        }
    }

    private void OnViewLeaderboardClicked()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

    private void OnShareProgressClicked()
    {
        Debug.Log("Native Share Integration Hook Triggered.");
        
        // Share via Android Native Share
        new NativeShare()
            .SetSubject("My Step-Up Activity!")
            .SetText($"I just burned {calorieTxt.text} and walked {stepCountTxt.text} steps today on the Step-Up app!")
            .SetUrl("https://stepup-app.com")
            .SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
            .Share();
    }
}