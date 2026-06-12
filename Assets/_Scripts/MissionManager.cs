using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MissionCardUI
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Slider progressBar;
    public Button claimButton;
    public TextMeshProUGUI claimBtnText;
}

public class MissionManager : MonoBehaviour
{
    public StepManager stepManager;
    public ProfileManager profileManager;

    [Header("UI Scroll View Setup")]
    public Transform scrollContentParent;
    public GameObject missionCardPrefab;
    
    // We will track spawned UI elements to update them live
    private List<GameObject> spawnedCards = new List<GameObject>();
    private List<MissionCardUI> cardUIs = new List<MissionCardUI>();

    private int dailyGoal;

    struct MissionParams {
        public int M1_StepsToday;
        public int M2_SingleSessionSteps;
        public int M3_NumSessions;
        public int M3_SessionMinSteps;
        public int M4_ContinuousWalkMins;
        
        public int M5_ARSteps; 
        public int M5_PercentGoal; 
        
        public int M6_PercentGoal; 
        public int M6_BeatYesterdayBy;
        
        public int M7_BonusSteps;
        
        public int W1_WeeklySteps;
        public int W2_GoalDays;
        
        public int W3_ContinuousWalkMins; 
        public int W3_ARSteps;
    }

    void OnEnable()
    {
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();
        if (profileManager == null) profileManager = FindFirstObjectByType<ProfileManager>();
        
        dailyGoal = PlayerPrefs.GetInt("DailyStepGoal", 10000);
        
        GenerateMissionBoard();
    }

    void Update()
    {
        // Live update the UI every frame
        if (spawnedCards.Count > 0 && Time.frameCount % 10 == 0) // Limit to every 10 frames to save battery
        {
            UpdateAllProgress();
        }
    }

    public void GenerateMissionBoard()
    {
        // 1. Clear old cards
        foreach (var c in spawnedCards) Destroy(c);
        spawnedCards.Clear();
        cardUIs.Clear();

        // 2. Fetch User Data
        string bmiCat = PlayerPrefs.GetString("BMICategory", "Normal");
        string rank = "Starter";
        
        if (profileManager != null && profileManager.activityLevelText != null)
        {
            rank = profileManager.activityLevelText.text.ToUpper(); // STARTER, EXPLORER, TRAILBLAZER, MARATHONER
        }

        // 3. Get exact mission numbers based on WHO standards
        MissionParams p = GetMissionParams(bmiCat, rank);

        // 4. Instantiate the 10 missions (7 Daily, 3 Weekly)
        SpawnMission("Daily Mission 1", $"Complete {p.M1_StepsToday} steps today.", 
                     stepManager.currentDailySteps, p.M1_StepsToday, 0);

        SpawnMission("Daily Mission 2", $"Walk {p.M2_SingleSessionSteps} steps in a single walking session.", 
                     stepManager.maxSessionStepsToday, p.M2_SingleSessionSteps, 1);

        SpawnMission("Daily Mission 3", $"Complete {p.M3_NumSessions} walking sessions with at least {p.M3_SessionMinSteps} steps each.", 
                     GetSessionsOver(p.M3_SessionMinSteps), p.M3_NumSessions, 2);

        SpawnMission("Daily Mission 4", $"Complete a {p.M4_ContinuousWalkMins}-minute continuous walk.", 
                     (int)stepManager.maxContinuousWalkMinutes, p.M4_ContinuousWalkMins, 3);

        if (p.M5_ARSteps > 0)
            SpawnMission("Daily Mission 5", $"Complete {p.M5_ARSteps} steps while AR Walking Mode is active.", 
                         stepManager.arStepsToday, p.M5_ARSteps, 4);
        else
            SpawnMission("Daily Mission 5", $"Reach {p.M5_PercentGoal}% of your daily step goal.", 
                         stepManager.currentDailySteps, (dailyGoal * p.M5_PercentGoal) / 100, 4);

        if (p.M6_BeatYesterdayBy > 0)
            SpawnMission("Daily Mission 6", $"Beat yesterday's step count by at least {p.M6_BeatYesterdayBy} steps.", 
                         stepManager.currentDailySteps, stepManager.yesterdaysSteps + p.M6_BeatYesterdayBy, 5);
        else
            SpawnMission("Daily Mission 6", $"Reach {p.M6_PercentGoal}% of your daily step goal.", 
                         stepManager.currentDailySteps, (dailyGoal * p.M6_PercentGoal) / 100, 5);

        SpawnMission("Daily Mission 7", $"Complete your daily goal plus {p.M7_BonusSteps} bonus steps.", 
                     stepManager.currentDailySteps, dailyGoal + p.M7_BonusSteps, 6);

        // WEEKLY MISSIONS
        SpawnMission("Weekly Mission 1", $"Accumulate {p.W1_WeeklySteps} steps this week.", 
                     stepManager.currentWeeklySteps, p.W1_WeeklySteps, 7);

        SpawnMission("Weekly Mission 2", $"Complete your daily step goal at least {p.W2_GoalDays} days this week.", 
                     stepManager.daysGoalMetThisWeek, p.W2_GoalDays, 8);

        if (p.W3_ContinuousWalkMins > 0)
            SpawnMission("Weekly Mission 3", $"Complete one {p.W3_ContinuousWalkMins}-minute continuous walk this week.", 
                         (int)stepManager.maxContinuousWalkMinutes, p.W3_ContinuousWalkMins, 9); // For simplicity using daily max
        else
            SpawnMission("Weekly Mission 3", $"Complete {p.W3_ARSteps} total AR Walking Mode steps this week.", 
                         stepManager.totalWeeklyARSteps, p.W3_ARSteps, 9);
    }

    private void SpawnMission(string title, string desc, int currentProgress, int target, int missionIndex)
    {
        if (missionCardPrefab == null || scrollContentParent == null) return;

        GameObject cardObj = Instantiate(missionCardPrefab, scrollContentParent);
        MissionCardUI ui = new MissionCardUI();
        
        // Find children by naming convention (You will need to set up the prefab this way)
        Transform titleT = cardObj.transform.Find("TitleText");
        Transform descT = cardObj.transform.Find("DescText");
        Transform sliderT = cardObj.transform.Find("ProgressBar");
        Transform btnT = cardObj.transform.Find("ClaimButton");

        if (titleT != null) ui.titleText = titleT.GetComponent<TextMeshProUGUI>();
        if (descT != null) ui.descText = descT.GetComponent<TextMeshProUGUI>();
        if (sliderT != null) ui.progressBar = sliderT.GetComponent<Slider>();
        if (btnT != null)
        {
            ui.claimButton = btnT.GetComponent<Button>();
            Transform btnTextT = btnT.Find("Text (TMP)");
            if (btnTextT != null) ui.claimBtnText = btnTextT.GetComponent<TextMeshProUGUI>();
        }

        ui.titleText.text = title;
        ui.descText.text = desc;
        
        // Cache for live updates
        spawnedCards.Add(cardObj);
        cardUIs.Add(ui);

        UpdateSingleCardUI(ui, currentProgress, target, missionIndex);
    }

    private void UpdateAllProgress()
    {
        string bmiCat = PlayerPrefs.GetString("BMICategory", "Normal");
        string rank = "STARTER";
        if (profileManager != null && profileManager.activityLevelText != null) rank = profileManager.activityLevelText.text.ToUpper();
        
        MissionParams p = GetMissionParams(bmiCat, rank);

        // Map live progress data to the 10 slots
        int[] currentProgs = new int[10] {
            stepManager.currentDailySteps,
            stepManager.maxSessionStepsToday,
            GetSessionsOver(p.M3_SessionMinSteps),
            (int)stepManager.maxContinuousWalkMinutes,
            p.M5_ARSteps > 0 ? stepManager.arStepsToday : stepManager.currentDailySteps,
            p.M6_BeatYesterdayBy > 0 ? stepManager.currentDailySteps : stepManager.currentDailySteps,
            stepManager.currentDailySteps,
            stepManager.currentWeeklySteps,
            stepManager.daysGoalMetThisWeek,
            p.W3_ContinuousWalkMins > 0 ? (int)stepManager.maxContinuousWalkMinutes : stepManager.totalWeeklyARSteps
        };

        int[] targets = new int[10] {
            p.M1_StepsToday,
            p.M2_SingleSessionSteps,
            p.M3_NumSessions,
            p.M4_ContinuousWalkMins,
            p.M5_ARSteps > 0 ? p.M5_ARSteps : (dailyGoal * p.M5_PercentGoal)/100,
            p.M6_BeatYesterdayBy > 0 ? stepManager.yesterdaysSteps + p.M6_BeatYesterdayBy : (dailyGoal * p.M6_PercentGoal)/100,
            dailyGoal + p.M7_BonusSteps,
            p.W1_WeeklySteps,
            p.W2_GoalDays,
            p.W3_ContinuousWalkMins > 0 ? p.W3_ContinuousWalkMins : p.W3_ARSteps
        };

        for (int i = 0; i < cardUIs.Count; i++)
        {
            UpdateSingleCardUI(cardUIs[i], currentProgs[i], targets[i], i);
        }
    }

    private void UpdateSingleCardUI(MissionCardUI ui, int currentProgress, int target, int missionIndex)
    {
        if (ui.progressBar != null) ui.progressBar.value = Mathf.Clamp01((float)currentProgress / target);
        
        bool isClaimed = PlayerPrefs.GetInt("MissionClaimed_" + missionIndex, 0) == 1;

        if (isClaimed)
        {
            if (ui.claimBtnText != null) ui.claimBtnText.text = "CLAIMED";
            if (ui.claimButton != null) ui.claimButton.interactable = false;
        }
        else
        {
            if (currentProgress >= target)
            {
                if (ui.claimBtnText != null) ui.claimBtnText.text = "CLAIM";
                if (ui.claimButton != null) 
                {
                    ui.claimButton.interactable = true;
                    ui.claimButton.onClick.RemoveAllListeners();
                    ui.claimButton.onClick.AddListener(() => ClaimMission(missionIndex, missionIndex >= 7 ? 5000 : 1000));
                }
            }
            else
            {
                if (ui.claimBtnText != null) ui.claimBtnText.text = $"{currentProgress}/{target}";
                if (ui.claimButton != null) ui.claimButton.interactable = false;
            }
        }
    }

    private int GetSessionsOver(int targetSteps)
    {
        int count = 0;
        foreach (int s in stepManager.completedSessionsToday) if (s >= targetSteps) count++;
        return count;
    }

    private void ClaimMission(int index, int xpReward)
    {
        PlayerPrefs.SetInt("MissionClaimed_" + index, 1);
        int currentXP = PlayerPrefs.GetInt("MissionXPEarned", 0);
        PlayerPrefs.SetInt("MissionXPEarned", currentXP + xpReward);
        PlayerPrefs.Save();
        UpdateAllProgress(); // Refresh instantly
        Debug.Log($"Mission {index} Claimed! +{xpReward} XP");
    }

    // --- MASSIVE WHO LOOKUP TABLE ---
    private MissionParams GetMissionParams(string bmiCat, string rank)
    {
        // Safe default fallback
        MissionParams p = new MissionParams { 
            M1_StepsToday = 4000, M2_SingleSessionSteps = 1000, M3_NumSessions = 3, M3_SessionMinSteps = 500, M4_ContinuousWalkMins = 10,
            M5_PercentGoal = 50, M6_PercentGoal = 75, M7_BonusSteps = 300, W1_WeeklySteps = 24000, W2_GoalDays = 3, W3_ContinuousWalkMins = 20 
        };

        bool isStarter = rank == "STARTER";
        bool isExplorer = rank == "EXPLORER";
        bool isTrailblazer = rank == "TRAILBLAZER";
        bool isMarathoner = rank == "MARATHONER" || rank == "ELITE RUNNER";

        if (bmiCat == "Underweight")
        {
            if (isStarter) { p.M1_StepsToday=3000; p.M2_SingleSessionSteps=1000; p.M3_NumSessions=3; p.M3_SessionMinSteps=500; p.M4_ContinuousWalkMins=5; p.M5_PercentGoal=50; p.M6_PercentGoal=75; p.M7_BonusSteps=300; p.W1_WeeklySteps=18000; p.W2_GoalDays=3; p.W3_ContinuousWalkMins=20; }
            if (isExplorer) { p.M1_StepsToday=4000; p.M2_SingleSessionSteps=1400; p.M3_NumSessions=4; p.M3_SessionMinSteps=500; p.M4_ContinuousWalkMins=8; p.M5_ARSteps=1000; p.M6_PercentGoal=80; p.M7_BonusSteps=400; p.W1_WeeklySteps=24000; p.W2_GoalDays=4; p.W3_ARSteps=5000; p.W3_ContinuousWalkMins=0; }
            if (isTrailblazer) { p.M1_StepsToday=5000; p.M2_SingleSessionSteps=1800; p.M3_NumSessions=3; p.M3_SessionMinSteps=1000; p.M4_ContinuousWalkMins=12; p.M5_ARSteps=1500; p.M6_BeatYesterdayBy=300; p.M7_BonusSteps=500; p.W1_WeeklySteps=30000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=30; }
            if (isMarathoner) { p.M1_StepsToday=6000; p.M2_SingleSessionSteps=2200; p.M3_NumSessions=3; p.M3_SessionMinSteps=1200; p.M4_ContinuousWalkMins=15; p.M5_ARSteps=2000; p.M6_BeatYesterdayBy=500; p.M7_BonusSteps=600; p.W1_WeeklySteps=36000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=35; }
        }
        else if (bmiCat == "Normal")
        {
            if (isStarter) { p.M1_StepsToday=4000; p.M2_SingleSessionSteps=1200; p.M3_NumSessions=3; p.M3_SessionMinSteps=800; p.M4_ContinuousWalkMins=10; p.M5_ARSteps=1000; p.M6_PercentGoal=75; p.M7_BonusSteps=400; p.W1_WeeklySteps=24000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=25; }
            if (isExplorer) { p.M1_StepsToday=5500; p.M2_SingleSessionSteps=1800; p.M3_NumSessions=3; p.M3_SessionMinSteps=1000; p.M4_ContinuousWalkMins=12; p.M5_ARSteps=1500; p.M6_BeatYesterdayBy=300; p.M7_BonusSteps=500; p.W1_WeeklySteps=33000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=35; }
            if (isTrailblazer) { p.M1_StepsToday=7000; p.M2_SingleSessionSteps=2500; p.M3_NumSessions=3; p.M3_SessionMinSteps=1500; p.M4_ContinuousWalkMins=15; p.M5_ARSteps=2000; p.M6_BeatYesterdayBy=500; p.M7_BonusSteps=700; p.W1_WeeklySteps=42000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=45; }
            if (isMarathoner) { p.M1_StepsToday=8500; p.M2_SingleSessionSteps=3200; p.M3_NumSessions=4; p.M3_SessionMinSteps=1500; p.M4_ContinuousWalkMins=20; p.M5_ARSteps=3000; p.M6_BeatYesterdayBy=700; p.M7_BonusSteps=1000; p.W1_WeeklySteps=51000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=55; }
        }
        else if (bmiCat == "Overweight")
        {
            if (isStarter) { p.M1_StepsToday=5000; p.M2_SingleSessionSteps=1500; p.M3_NumSessions=3; p.M3_SessionMinSteps=1000; p.M4_ContinuousWalkMins=12; p.M5_ARSteps=1200; p.M6_PercentGoal=75; p.M7_BonusSteps=500; p.W1_WeeklySteps=30000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=30; }
            if (isExplorer) { p.M1_StepsToday=6500; p.M2_SingleSessionSteps=2200; p.M3_NumSessions=3; p.M3_SessionMinSteps=1200; p.M4_ContinuousWalkMins=15; p.M5_ARSteps=1800; p.M6_BeatYesterdayBy=400; p.M7_BonusSteps=650; p.W1_WeeklySteps=39000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=40; }
            if (isTrailblazer) { p.M1_StepsToday=8000; p.M2_SingleSessionSteps=3000; p.M3_NumSessions=4; p.M3_SessionMinSteps=1500; p.M4_ContinuousWalkMins=18; p.M5_ARSteps=2500; p.M6_BeatYesterdayBy=600; p.M7_BonusSteps=800; p.W1_WeeklySteps=48000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=50; }
            if (isMarathoner) { p.M1_StepsToday=9500; p.M2_SingleSessionSteps=3800; p.M3_NumSessions=4; p.M3_SessionMinSteps=1800; p.M4_ContinuousWalkMins=22; p.M5_ARSteps=3200; p.M6_BeatYesterdayBy=800; p.M7_BonusSteps=1000; p.W1_WeeklySteps=57000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=60; }
        }
        else if (bmiCat == "Obese Class I")
        {
            if (isStarter) { p.M1_StepsToday=4000; p.M2_SingleSessionSteps=1200; p.M3_NumSessions=3; p.M3_SessionMinSteps=700; p.M4_ContinuousWalkMins=8; p.M5_ARSteps=1000; p.M6_PercentGoal=75; p.M7_BonusSteps=300; p.W1_WeeklySteps=24000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=25; }
            if (isExplorer) { p.M1_StepsToday=5500; p.M2_SingleSessionSteps=1800; p.M3_NumSessions=3; p.M3_SessionMinSteps=1000; p.M4_ContinuousWalkMins=10; p.M5_ARSteps=1500; p.M6_BeatYesterdayBy=300; p.M7_BonusSteps=500; p.W1_WeeklySteps=33000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=35; }
            if (isTrailblazer) { p.M1_StepsToday=7000; p.M2_SingleSessionSteps=2500; p.M3_NumSessions=3; p.M3_SessionMinSteps=1300; p.M4_ContinuousWalkMins=15; p.M5_ARSteps=2000; p.M6_BeatYesterdayBy=500; p.M7_BonusSteps=700; p.W1_WeeklySteps=42000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=45; }
            if (isMarathoner) { p.M1_StepsToday=8500; p.M2_SingleSessionSteps=3200; p.M3_NumSessions=4; p.M3_SessionMinSteps=1500; p.M4_ContinuousWalkMins=18; p.M5_ARSteps=2800; p.M6_BeatYesterdayBy=600; p.M7_BonusSteps=1000; p.W1_WeeklySteps=51000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=55; }
        }
        else // Obese Class II & III
        {
            if (isStarter) { p.M1_StepsToday=3000; p.M2_SingleSessionSteps=800; p.M3_NumSessions=3; p.M3_SessionMinSteps=500; p.M4_ContinuousWalkMins=5; p.M5_ARSteps=600; p.M6_PercentGoal=75; p.M7_BonusSteps=200; p.W1_WeeklySteps=18000; p.W2_GoalDays=3; p.W3_ContinuousWalkMins=20; }
            if (isExplorer) { p.M1_StepsToday=4500; p.M2_SingleSessionSteps=1300; p.M3_NumSessions=3; p.M3_SessionMinSteps=700; p.M4_ContinuousWalkMins=8; p.M5_ARSteps=1000; p.M6_PercentGoal=80; p.M7_BonusSteps=300; p.W1_WeeklySteps=27000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=30; }
            if (isTrailblazer) { p.M1_StepsToday=6000; p.M2_SingleSessionSteps=2000; p.M3_NumSessions=3; p.M3_SessionMinSteps=1000; p.M4_ContinuousWalkMins=12; p.M5_ARSteps=1500; p.M6_BeatYesterdayBy=300; p.M7_BonusSteps=500; p.W1_WeeklySteps=36000; p.W2_GoalDays=4; p.W3_ContinuousWalkMins=40; }
            if (isMarathoner) { p.M1_StepsToday=7500; p.M2_SingleSessionSteps=2800; p.M3_NumSessions=3; p.M3_SessionMinSteps=1500; p.M4_ContinuousWalkMins=15; p.M5_ARSteps=2200; p.M6_BeatYesterdayBy=500; p.M7_BonusSteps=750; p.W1_WeeklySteps=45000; p.W2_GoalDays=5; p.W3_ContinuousWalkMins=50; }
        }
        
        return p;
    }
}