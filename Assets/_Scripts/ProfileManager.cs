using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

[System.Serializable]
public class RankTier
{
    public string rankName;
    public Sprite rankImage;
    public int requiredLevel;
    public Color rankColor = Color.white;
}

public class ProfileManager : MonoBehaviour
{
    [Header("UI Text References")]
    public TMP_InputField userNameInput; 
    public TextMeshProUGUI levelText;         
    public TextMeshProUGUI activityLevelText; 

    [Header("Stats UI")]
    public GameObject summaryPanel;
    [Header("XP Progress Bar")]
    public Slider xpProgressBar; 
    public TextMeshProUGUI xpProgressText; // Optional: To print "2500 / 5000 XP"

    [Header("Action Buttons & Icons")]
    public TextMeshProUGUI editButtonText; 
    public GameObject[] penIcons; // Drag both of your Pen icons into this array

    [Header("Image Displays")]
    public Image profileAvatarImage; // Top-Left user selected avatar photo
    public Image rankBadgeImage;     // Bottom activity milestone status badge
    
    [Header("Rank Configuration")]
    public RankTier[] rankTiers; 
    
    private int currentLevel = 1;
    private int xpPerLevel = 5000; 
    private bool isEditing = false; 
    private string savedImagePathKey = "CustomAvatarPath";
    private DateTime lastUsernameChange = DateTime.MinValue;

    void OnEnable()
    {
        isEditing = false;
        UpdateEditModeUI();
        RefreshProfileUI();
        LoadCustomAvatar();
    }

    void Update()
    {
        // Live XP Updates while the profile panel is open!
        if (StepManager.Instance != null)
        {
            int missionXPEarned = PlayerPrefs.GetInt("MissionXPEarned", 0);
            
            // BUGFIX: EXP should ONLY come from claimed missions, NOT from walking steps!
            int totalXP = missionXPEarned;
            currentLevel = (totalXP / xpPerLevel) + 1; 
            int currentXPInLevel = totalXP % xpPerLevel;
            float progressPercentage = (float)currentXPInLevel / xpPerLevel;
            
            if (xpProgressBar != null) 
                xpProgressBar.value = progressPercentage;
                
            if (xpProgressText != null)
                xpProgressText.text = $"{currentXPInLevel} / {xpPerLevel}";
        }
    }

    public void RefreshProfileUI()
    {
        // 1. Load Step Tracker and Reward Core Values
        string currentName = PlayerPrefs.GetString("UserName", "Player 1"); 
        int totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 0);
        int missionXPEarned = PlayerPrefs.GetInt("MissionXPEarned", 0);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth != null && auth.CurrentUser != null)
        {
            FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(auth.CurrentUser.UserId)
                .GetValueAsync().ContinueWithOnMainThread(task => {
                    if (task.IsCompleted && task.Result.Exists)
                    {
                        var snapshot = task.Result;
                        if (snapshot.Child("username").Value != null)
                        {
                            currentName = snapshot.Child("username").Value.ToString();
                            
                            // Retroactive fix for old accounts stuck as "Player"
                            if (string.IsNullOrEmpty(currentName) || currentName.Trim().ToLower() == "player" || currentName.Trim().ToLower() == "player 1")
                            {
                                if (auth.CurrentUser != null && !string.IsNullOrEmpty(auth.CurrentUser.Email))
                                {
                                    string[] emailParts = auth.CurrentUser.Email.Split('@');
                                    if (emailParts.Length > 0)
                                    {
                                        currentName = emailParts[0];
                                        if (currentName.Length > 10) currentName = currentName.Substring(0, 10);
                                        // Force overwrite the bad name in DB
                                        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(auth.CurrentUser.UserId).Child("username").SetValueAsync(currentName);
                                    }
                                }
                            }
                            
                            userNameInput.text = currentName;
                            PlayerPrefs.SetString("UserName", currentName);
                        }
                        if (snapshot.Child("lastUsernameChange").Value != null)
                        {
                            long ticks = Convert.ToInt64(snapshot.Child("lastUsernameChange").Value);
                            lastUsernameChange = new DateTime(ticks);
                        }
                    }
                });
        }

        // 2. XP Calculations 
        // BUGFIX: EXP should ONLY come from claimed missions!
        int totalXP = missionXPEarned;
        currentLevel = (totalXP / xpPerLevel) + 1; 
        int currentXPInLevel = totalXP % xpPerLevel;
        float progressPercentage = (float)currentXPInLevel / xpPerLevel;

        // 3. Populate Fields
        userNameInput.text = currentName; 
        levelText.text = currentLevel.ToString();  
        
        if (xpProgressBar != null) 
            xpProgressBar.value = progressPercentage;
            
        if (xpProgressText != null)
            xpProgressText.text = $"{currentXPInLevel} / {xpPerLevel}";

        // 4. Evaluate and Assign Automated Milestones
        if (rankTiers != null && rankTiers.Length > 0)
        {
            RankTier currentRank = rankTiers[0]; 
            for (int i = 0; i < rankTiers.Length; i++)
            {
                if (currentLevel >= rankTiers[i].requiredLevel)
                {
                    currentRank = rankTiers[i];
                }
            }
            activityLevelText.text = currentRank.rankName.ToUpper();
            activityLevelText.color = currentRank.rankColor;
            if (rankBadgeImage != null) rankBadgeImage.sprite = currentRank.rankImage;
        }
    }

   public void OpenStats()
    {
    if (summaryPanel != null)
    {
        summaryPanel.SetActive(true);
        this.gameObject.SetActive(false); // Hides the profile panel so they don't overlap
    }
    else
    {
        Debug.LogError("Summary Panel is not assigned in the ProfileManager inspector!");
    }
    }

   public void CloseStats()
    {
    if (summaryPanel != null)
    {
        summaryPanel.SetActive(false);
        this.gameObject.SetActive(true); // Shows the profile panel again
    }
    }

    // --- INTERACTION HANDLING ---
    public void ToggleEditMode()
    {
        isEditing = !isEditing;

        if (isEditing)
        {
            editButtonText.text = "SAVE PROFILE";
        }
        else
        {
            // Validate and enforce character limits on save (Max 10 chars)
            string cleanName = userNameInput.text;
            if (cleanName.Length > 10) cleanName = cleanName.Substring(0, 10);

            // Cooldown check (7 days) REMOVED so users can edit anytime
            // (Previously blocked if changed within 7 days)            editButtonText.text = "EDIT MY PROFILE";
            
            PlayerPrefs.SetString("UserName", cleanName);
            PlayerPrefs.Save();
            userNameInput.text = cleanName;

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            if (auth != null && auth.CurrentUser != null)
            {
                lastUsernameChange = DateTime.UtcNow;
                DatabaseReference userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(auth.CurrentUser.UserId);
                userRef.Child("username").SetValueAsync(cleanName);
                userRef.Child("lastUsernameChange").SetValueAsync(lastUsernameChange.Ticks);
            }
        }

        UpdateEditModeUI();
    }

    private void UpdateEditModeUI()
    {
        userNameInput.interactable = isEditing;
        
        // Toggle the interactable edit items
        foreach (GameObject pen in penIcons)
        {
            if (pen != null) pen.SetActive(isEditing);
        }
    }

    // --- NATIVE MOBILE GALLERY HUB ---
    
    public void OpenDeviceGallery()
    {
        // Block gallery click if the user hasn't clicked "EDIT MY PROFILE" first!
        if (!isEditing) return;

        // FIX: Removed the "NativeGallery.Permission permission =" assignment
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                // Process and compile image bytes from disk
                byte[] fileData = File.ReadAllBytes(path);
                Texture2D texture = new Texture2D(2, 2);
                
                if (texture.LoadImage(fileData))
                {
                    // Format runtime file to UI layout specs
                    Sprite customAvatar = Sprite.Create(
                        texture, 
                        new Rect(0, 0, texture.width, texture.height), 
                        new Vector2(0.5f, 0.5f)
                    );
                    
                    profileAvatarImage.sprite = customAvatar;

                    // Permanently save the path string to storage
                    PlayerPrefs.SetString(savedImagePathKey, path);
                    PlayerPrefs.Save();
                }
            }
        }, "Select Profile Picture", "image/*");
    }

    private void LoadCustomAvatar()
    {
        if (PlayerPrefs.HasKey(savedImagePathKey))
        {
            string savedPath = PlayerPrefs.GetString(savedImagePathKey);
            if (File.Exists(savedPath))
            {
                byte[] fileData = File.ReadAllBytes(savedPath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    Sprite savedAvatar = Sprite.Create(
                        texture, 
                        new Rect(0, 0, texture.width, texture.height), 
                        new Vector2(0.5f, 0.5f)
                    );
                    profileAvatarImage.sprite = savedAvatar;
                }
            }
        }
    }
}