using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActivitySummaryManager : MonoBehaviour
{
    [Header("--- Top Section Components ---")]
    [SerializeField] private TextMeshProUGUI streakTxt;
    [SerializeField] private TextMeshProUGUI calorieTxt;
    [SerializeField] private TextMeshProUGUI speedTxt;
    [SerializeField] private Image graphFillRing;

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

        if (streakTxt != null) streakTxt.text = $": {actualStreak} DAYS";
        if (calorieTxt != null) calorieTxt.text = $"{calculatedCalories:F0} kcal";
        if (speedTxt != null) speedTxt.text = $"{estimatedSpeed:F1} KM/H";

        if (stepCountTxt != null) 
            stepCountTxt.text = $"\"{currentSteps}/{dailyStepGoal}\"";

        if (distanceTxt != null) 
            distanceTxt.text = $"\"{calculatedDistanceKm:F2} KM\"";

        if (graphFillRing != null)
        {
            float fillRatio = (float)currentSteps / dailyStepGoal;
            graphFillRing.fillAmount = Mathf.Clamp01(fillRatio);
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
    }
}