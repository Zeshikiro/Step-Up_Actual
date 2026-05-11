using UnityEngine;
using TMPro;
using UnityEngine.UI;

// This handles everything: The Name, the Image, and the Unlock Level!
[System.Serializable]
public class RankTier
{
    public string rankName;
    public Sprite rankImage;
    public int requiredLevel;
}

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField userNameInput; 
    public TextMeshProUGUI levelText;        
    public TextMeshProUGUI activityLevelText; 
    public Slider xpProgressBar; 
    
    [Header("Edit Mode UI")]
    public TextMeshProUGUI editButtonText; 
    public GameObject[] penIcons; // Drop your new Pen UI objects in here!

    [Header("Images")]
    public Image profileAvatarImage; // The top-left profile picture (Clickable)
    public Image rankBadgeImage;     // The bottom-left activity badge (Automatic)
    
    [Header("Rank System")]
    public RankTier[] rankTiers; // Build your ranks here!
    
    private int currentAvatarIndex = 0;
    private int currentLevel = 1;
    private int xpPerLevel = 5000; 
    private bool isEditing = false; 

    void OnEnable()
    {
        isEditing = false;
        UpdateEditModeUI();
        RefreshProfileUI();
    }

    public void RefreshProfileUI()
    {
        // 1. Fetch User Data
        string currentName = PlayerPrefs.GetString("UserName", "Player1"); 
        int totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 8500);
        int missionXPEarned = PlayerPrefs.GetInt("MissionXPEarned", 1000);

        // 2. Math for Leveling
        int totalXP = totalLifetimeSteps + missionXPEarned;
        currentLevel = (totalXP / xpPerLevel) + 1; 
        float currentXPProgress = (float)(totalXP % xpPerLevel) / xpPerLevel;

        // 3. Update Text and XP Bar
        userNameInput.text = currentName; 
        levelText.text = currentLevel.ToString();  
        if (xpProgressBar != null) xpProgressBar.value = currentXPProgress;

        // 4. AUTOMATIC RANK BADGE
        // Find the highest rank they have unlocked
        RankTier currentRank = rankTiers[0]; 
        for (int i = 0; i < rankTiers.Length; i++)
        {
            if (currentLevel >= rankTiers[i].requiredLevel)
            {
                currentRank = rankTiers[i];
            }
        }
        activityLevelText.text = currentRank.rankName.ToUpper();
        if (rankBadgeImage != null) rankBadgeImage.sprite = currentRank.rankImage;

        // 5. Load Saved Profile Avatar
        currentAvatarIndex = PlayerPrefs.GetInt("UserAvatar", 0);
        if (rankTiers.Length > 0 && currentLevel < rankTiers[currentAvatarIndex].requiredLevel)
        {
            currentAvatarIndex = 0; // Reset if they somehow saved a locked avatar
        }
        UpdateAvatarVisual();
    }

    // --- EDIT MODE SYSTEM ---
    public void ToggleEditMode()
    {
        isEditing = !isEditing;

        if (isEditing)
        {
            editButtonText.text = "SAVE PROFILE";
        }
        else
        {
            editButtonText.text = "EDIT MY PROFILE";
            PlayerPrefs.SetString("UserName", userNameInput.text);
            PlayerPrefs.SetInt("UserAvatar", currentAvatarIndex);
            PlayerPrefs.Save();
        }

        UpdateEditModeUI();
    }

    private void UpdateEditModeUI()
    {
        userNameInput.interactable = isEditing;
        
        // Turn the Pen icons on or off depending on edit mode!
        foreach (GameObject pen in penIcons)
        {
            if (pen != null) pen.SetActive(isEditing);
        }
    }

    // --- AVATAR CYCLING SYSTEM ---
    public void CycleNextAvatar()
    {
        if (!isEditing || rankTiers.Length == 0) return;

        int startIndex = currentAvatarIndex;

        // Loop to find the next unlocked avatar
        do
        {
            currentAvatarIndex++;
            if (currentAvatarIndex >= rankTiers.Length) currentAvatarIndex = 0; 
        } 
        while (currentLevel < rankTiers[currentAvatarIndex].requiredLevel && currentAvatarIndex != startIndex);

        UpdateAvatarVisual();
    }

    private void UpdateAvatarVisual()
    {
        if (rankTiers.Length > 0 && profileAvatarImage != null)
        {
            profileAvatarImage.sprite = rankTiers[currentAvatarIndex].rankImage;
        }
    }
}