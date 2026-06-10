using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    public StepManager stepManager;

    [Header("Mission 1: Daily Step Goal")]
    public TextMeshProUGUI m1TitleText;
    public Slider m1ProgressBar;
    public Button m1ClaimButton;

    [Header("Mission 2: Timed Walk (Anti-Cheat)")]
    public TextMeshProUGUI m2TitleText;
    public Slider m2ProgressBar;
    public Button m2ActionBtn;
    public TextMeshProUGUI m2ActionBtnText;

    [Header("Mission 3: Weekly Goal")]
    public TextMeshProUGUI m3TitleText;
    public Slider m3ProgressBar;
    public Button m3ClaimButton;

    private int dailyGoal;
    private string bmiCategory;
    private int currentDailySteps;
    
    // Mission 2 Settings
    private int m2RequiredSteps = 2000;
    private float m2DurationMinutes = 20f;
    private bool m2Claimed = false;

    // Mission 3 Settings
    private int weeklyGoal = 40000;

    void OnEnable()
    {
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();
        RefreshMissions();
    }

    void Update()
    {
        // Live update the Timed Mission progress
        if (stepManager != null && stepManager.isSessionActive)
        {
            float elapsedMins = (Time.time - stepManager.sessionStartTime) / 60f;
            int stepsLeft = m2RequiredSteps - stepManager.sessionSteps;
            
            m2TitleText.text = $"BRISK WALK: {stepManager.sessionSteps}/{m2RequiredSteps} steps\nTime Left: {(m2DurationMinutes - elapsedMins):F1} mins";
            if (m2ProgressBar != null) m2ProgressBar.value = (float)stepManager.sessionSteps / m2RequiredSteps;

            if (elapsedMins >= m2DurationMinutes)
            {
                // Timer finished! Did they pass?
                stepManager.StopMissionSession();
                
                // Anti-Cheat: They must walk enough steps AND move more than 50 meters physically
                if (stepManager.sessionSteps >= m2RequiredSteps && stepManager.sessionDistanceMeters > 50f)
                {
                    m2TitleText.text = "MISSION PASSED! +1000 XP";
                    m2ActionBtnText.text = "CLAIM";
                    m2ActionBtn.onClick.RemoveAllListeners();
                    m2ActionBtn.onClick.AddListener(ClaimMission2);
                    m2ActionBtn.interactable = true;
                }
                else
                {
                    m2TitleText.text = "FAILED! (Too slow or fake shaking)";
                    m2ActionBtnText.text = "RETRY";
                    m2ActionBtn.onClick.RemoveAllListeners();
                    m2ActionBtn.onClick.AddListener(StartMission2);
                    m2ActionBtn.interactable = true;
                }
            }
        }
    }

    public void RefreshMissions()
    {
        bmiCategory = PlayerPrefs.GetString("BMICategory", "Normal");
        dailyGoal = PlayerPrefs.GetInt("DailyStepGoal", 10000);
        currentDailySteps = PlayerPrefs.GetInt("DailySteps", 0);
        int currentWeeklySteps = PlayerPrefs.GetInt("WeeklySteps", 0);

        // Setup BMI specific difficulty
        if (bmiCategory == "Underweight") { m2RequiredSteps = 1000; m2DurationMinutes = 15f; weeklyGoal = 30000; }
        if (bmiCategory == "Normal")      { m2RequiredSteps = 2000; m2DurationMinutes = 20f; weeklyGoal = 50000; }
        if (bmiCategory == "Overweight")  { m2RequiredSteps = 1500; m2DurationMinutes = 20f; weeklyGoal = 40000; }
        if (bmiCategory == "Obese")       { m2RequiredSteps = 1000; m2DurationMinutes = 25f; weeklyGoal = 25000; }

        // --- MISSION 1: Daily Goal ---
        if (m1TitleText != null) m1TitleText.text = $"DAILY GOAL: {currentDailySteps} / {dailyGoal} STEPS";
        if (m1ProgressBar != null) m1ProgressBar.value = (float)currentDailySteps / dailyGoal;
        
        bool m1Claimed = PlayerPrefs.GetInt("M1_Claimed", 0) == 1;
        if (m1TitleText != null && m1Claimed) { m1TitleText.text = "DAILY DONE! +2500 XP"; }
        if (m1ClaimButton != null)
        {
            m1ClaimButton.interactable = !m1Claimed && (currentDailySteps >= dailyGoal);
            m1ClaimButton.onClick.RemoveAllListeners();
            m1ClaimButton.onClick.AddListener(ClaimMission1);
        }

        // --- MISSION 2: Timed Session ---
        if (!m2Claimed && (stepManager == null || !stepManager.isSessionActive))
        {
            if (m2TitleText != null) m2TitleText.text = $"TIMED: {m2RequiredSteps} steps in {m2DurationMinutes} mins";
            if (m2ProgressBar != null) m2ProgressBar.value = 0;
            if (m2ActionBtnText != null) m2ActionBtnText.text = "START";
            if (m2ActionBtn != null)
            {
                m2ActionBtn.onClick.RemoveAllListeners();
                m2ActionBtn.onClick.AddListener(StartMission2);
                m2ActionBtn.interactable = true;
            }
        }

        // --- MISSION 3: Weekly Goal ---
        if (m3TitleText != null) m3TitleText.text = $"WEEKLY: {currentWeeklySteps} / {weeklyGoal} STEPS";
        if (m3ProgressBar != null) m3ProgressBar.value = (float)currentWeeklySteps / weeklyGoal;
        
        bool m3Claimed = PlayerPrefs.GetInt("M3_Claimed", 0) == 1;
        if (m3TitleText != null && m3Claimed) { m3TitleText.text = "WEEKLY DONE! +10000 XP"; }
        if (m3ClaimButton != null)
        {
            m3ClaimButton.interactable = !m3Claimed && (currentWeeklySteps >= weeklyGoal);
            m3ClaimButton.onClick.RemoveAllListeners();
            m3ClaimButton.onClick.AddListener(ClaimMission3);
        }
    }

    public void StartMission2()
    {
        if (stepManager != null)
        {
            stepManager.StartMissionSession(m2DurationMinutes);
            if (m2ActionBtn != null) m2ActionBtn.interactable = false;
            if (m2ActionBtnText != null) m2ActionBtnText.text = "WALKING...";
        }
    }

    public void ClaimMission1()
    {
        PlayerPrefs.SetInt("M1_Claimed", 1);
        AddXP(2500);
        RefreshMissions();
    }

    public void ClaimMission2()
    {
        m2Claimed = true;
        AddXP(1000);
        if (m2TitleText != null) m2TitleText.text = "TIMED DONE!";
        if (m2ActionBtn != null) m2ActionBtn.interactable = false;
    }

    public void ClaimMission3()
    {
        PlayerPrefs.SetInt("M3_Claimed", 1);
        AddXP(10000);
        RefreshMissions();
    }

    private void AddXP(int amount)
    {
        int currentXP = PlayerPrefs.GetInt("MissionXPEarned", 0);
        PlayerPrefs.SetInt("MissionXPEarned", currentXP + amount);
        PlayerPrefs.Save();
        Debug.Log($"Awarded {amount} XP!");
    }
}