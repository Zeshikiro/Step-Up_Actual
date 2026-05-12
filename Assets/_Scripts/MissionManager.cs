using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour
{
    [Header("Daily Mission UI")]
    public TextMeshProUGUI missionTitleText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;
    public Button claimButton;

    private int dailyGoal;
    private int currentSteps;

    // This runs every time you open the Mission Panel!
    void OnEnable()
    {
        RefreshMissions();
    }

    public void RefreshMissions()
    {
        // 1. Fetch the personalized BMI Goal and Current Steps
        dailyGoal = PlayerPrefs.GetInt("DailyStepGoal", 10000);
        string bmiCategory = PlayerPrefs.GetString("BMICategory", "Normal");
        currentSteps = PlayerPrefs.GetInt("DailySteps", 0);

        // 2. Calculate the 5 Milestones dynamically based on their goal
        int m1 = Mathf.RoundToInt(dailyGoal * 0.2f);
        int m2 = Mathf.RoundToInt(dailyGoal * 0.4f);
        int m3 = Mathf.RoundToInt(dailyGoal * 0.6f);
        int m4 = Mathf.RoundToInt(dailyGoal * 0.8f);
        int m5 = dailyGoal;

        // 3. Update the Title to show their live progress
        missionTitleText.text = $"DAILY GOAL: {currentSteps} / {dailyGoal} STEPS";

        // 4. Update the text BELOW the bar to show the 5 milestones
        // We use the 'progressText' slot in the Inspector for this!
        progressText.text = $"{m1} > {m2} > {m3} > {m4} > {m5}";

        // 5. Update the Slider (Fills slowly based on exact steps)
        if (progressBar != null)
        {
            progressBar.value = (float)currentSteps / dailyGoal;
        }

        // 6. Unlock the Claim button ONLY if they hit the final milestone!
        if (currentSteps >= dailyGoal)
        {
            claimButton.interactable = true;
        }
        else
        {
            claimButton.interactable = false;
        }
    }

    // Call this from the "Claim" Button's OnClick() event!
    public void ClaimReward()
    {
        // Give them 1,000 XP for the Profile System we built earlier!
        int currentXP = PlayerPrefs.GetInt("MissionXPEarned", 0);
        PlayerPrefs.SetInt("MissionXPEarned", currentXP + 1000);
        PlayerPrefs.Save();

        // Change the UI to show they finished
        missionTitleText.text = "Goal Reached! +1000 XP";
        claimButton.interactable = false; // Turn button off so they can't spam it
    }
}