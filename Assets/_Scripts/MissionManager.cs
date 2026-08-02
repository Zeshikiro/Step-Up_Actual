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
        
        // Delay generation by 1 frame to ensure StepManager.Start() has fully loaded the PlayerPrefs!
        StartCoroutine(DelayedGenerate());
    }

    private System.Collections.IEnumerator DelayedGenerate()
    {
        yield return new WaitForEndOfFrame();
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
        // 1. REUSE old cards instead of destroying them to save massive memory spikes!
        // We no longer call Destroy() or Clear() here.

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
                     stepManager.GetLiveMaxSessionSteps(), p.M2_SingleSessionSteps, 1);

        SpawnMission("Daily Mission 3", $"Complete {p.M3_NumSessions} walking sessions with at least {p.M3_SessionMinSteps} steps each.", 
                     stepManager.GetLiveSessionsOver(p.M3_SessionMinSteps), p.M3_NumSessions, 2);

        SpawnMission("Daily Mission 4", $"Complete a {p.M4_ContinuousWalkMins}-minute continuous walk.", 
                     (int)stepManager.GetLiveContinuousWalkMinutes(), p.M4_ContinuousWalkMins, 3);

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
                         (int)stepManager.GetLiveContinuousWalkMinutes(), p.W3_ContinuousWalkMins, 9);
        else
            SpawnMission("Weekly Mission 3", $"Complete {p.W3_ARSteps} total AR Walking Mode steps this week.", 
                         stepManager.totalWeeklyARSteps, p.W3_ARSteps, 9);
                         
        // Default to showing daily missions
        ShowDailyMissions();
    }
    
    // UI Tab Methods (Link these to your Daily and Weekly buttons in the Inspector)
    public void ShowDailyMissions()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
            {
                spawnedCards[i].SetActive(i < 7); // Daily missions are indexes 0 to 6
            }
        }
    }

    public void ShowWeeklyMissions()
    {
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            if (spawnedCards[i] != null)
            {
                spawnedCards[i].SetActive(i >= 7); // Weekly missions are indexes 7 to 9
            }
        }
    }

    private void SpawnMission(string title, string desc, int currentProgress, int target, int missionIndex)
    {
        if (missionCardPrefab == null || scrollContentParent == null) return;

        GameObject cardObj;
        MissionCardUI ui;
        
        // REUSE existing card if it exists, otherwise instantiate a new one
        if (missionIndex < spawnedCards.Count)
        {
            cardObj = spawnedCards[missionIndex];
            ui = cardUIs[missionIndex];
        }
        else
        {
            cardObj = Instantiate(missionCardPrefab, scrollContentParent);
            cardObj.SetActive(true); // THIS FIXES THE INVISIBLE PREFAB BUG!
            cardObj.transform.localScale = Vector3.one;
            cardObj.transform.localPosition = new Vector3(cardObj.transform.localPosition.x, cardObj.transform.localPosition.y, 0f);
            
            ui = new MissionCardUI();
            
            // Find children dynamically no matter how deep they are nested in the prefab!
            TextMeshProUGUI[] allTexts = cardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in allTexts)
            {
                if (t.gameObject.name == "TitleText") ui.titleText = t;
                else if (t.gameObject.name == "DescText") ui.descText = t;
                else if (t.transform.parent != null && t.transform.parent.name == "ClaimButton") ui.claimBtnText = t;
                else if (t.gameObject.name == "Text (TMP)") ui.claimBtnText = t; // Fallback
            }

            Slider[] allSliders = cardObj.GetComponentsInChildren<Slider>(true);
            if (allSliders.Length > 0) ui.progressBar = allSliders[0];

            Button[] allButtons = cardObj.GetComponentsInChildren<Button>(true);
            foreach (var b in allButtons)
            {
                if (b.gameObject.name == "ClaimButton") ui.claimButton = b;
            }
            
            // Cache for live updates
            spawnedCards.Add(cardObj);
            cardUIs.Add(ui);
        }

        if (ui.titleText != null) ui.titleText.text = title;
        if (ui.descText != null) ui.descText.text = desc;

        string rank = "STARTER";
        if (profileManager != null && profileManager.activityLevelText != null) rank = profileManager.activityLevelText.text.ToUpper();

        UpdateSingleCardUI(ui, currentProgress, target, missionIndex, rank);
    }

    private void UpdateAllProgress()
    {
        string bmiCat = PlayerPrefs.GetString("BMICategory", "Normal");
        string rank = "STARTER";
        if (profileManager != null && profileManager.activityLevelText != null) rank = profileManager.activityLevelText.text.ToUpper();
        
        MissionParams p = GetMissionParams(bmiCat, rank);

        // Map live progress data to the 10 slots — using LIVE getters for real-time tracking!
        int[] currentProgs = new int[10] {
            stepManager.currentDailySteps,
            stepManager.GetLiveMaxSessionSteps(),
            stepManager.GetLiveSessionsOver(p.M3_SessionMinSteps),
            (int)stepManager.GetLiveContinuousWalkMinutes(),
            p.M5_ARSteps > 0 ? stepManager.arStepsToday : stepManager.currentDailySteps,
            p.M6_BeatYesterdayBy > 0 ? stepManager.currentDailySteps : stepManager.currentDailySteps,
            stepManager.currentDailySteps,
            stepManager.currentWeeklySteps,
            stepManager.daysGoalMetThisWeek,
            p.W3_ContinuousWalkMins > 0 ? (int)stepManager.GetLiveContinuousWalkMinutes() : stepManager.totalWeeklyARSteps
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
            UpdateSingleCardUI(cardUIs[i], currentProgs[i], targets[i], i, rank);
        }
    }

    private void UpdateSingleCardUI(MissionCardUI ui, int currentProgress, int target, int missionIndex, string rank)
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
                // Dynamic Economy: Calculate rewards based on difficulty rank!
                int baseDailyReward = 20;
                if (rank == "EXPLORER") baseDailyReward = 30;
                else if (rank == "TRAILBLAZER") baseDailyReward = 40;
                else if (rank == "MARATHONER" || rank == "ELITE RUNNER") baseDailyReward = 50;
                
                // Weekly missions give 5x the daily reward
                int xpReward = missionIndex >= 7 ? baseDailyReward * 5 : baseDailyReward;

                if (ui.claimBtnText != null) ui.claimBtnText.text = "CLAIM";
                if (ui.claimButton != null) 
                {
                    ui.claimButton.interactable = true;
                    ui.claimButton.onClick.RemoveAllListeners();
                    ui.claimButton.onClick.AddListener(() => ClaimMission(missionIndex, xpReward));
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
        
        // ECONOMY: Give the player coins equal to the XP reward!
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddCoins(xpReward);
        }

        UpdateAllProgress(); // Refresh instantly
        Debug.Log($"Mission {index} Claimed! +{xpReward} XP & Coins");
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