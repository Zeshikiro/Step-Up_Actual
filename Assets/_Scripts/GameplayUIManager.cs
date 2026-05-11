using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameplayUIManager : MonoBehaviour
{
    [Header("UI Panels (Drag your panels here!)")]
    public GameObject tipPopupPanel;
    public GameObject missionPanel;      
    public GameObject settingsPanel;     
    public GameObject progressPanel;     
    public GameObject leaderboardPanel;  
    public GameObject profilePanel;      

    [Header("Tip Pop-Up Text")]
    public TextMeshProUGUI tipTitleText;
    public TextMeshProUGUI tipBodyText;

    [Header("Routing Settings")]
    public string mainMenuSceneName = "LoginScene"; 
    public string alternateViewSceneName = "Your3DSceneName"; 

    private string[] funFacts = new string[]
    {
        "Walking 10,000 steps a day burns around 300 to 400 calories, depending on your pace and body weight!",
        "Walking backwards (retro walking) actually burns more calories and helps sharpen your balance.",
        "Brisk walking for just 30 minutes a day can significantly boost your mood and reduce stress.",
        "The human foot has 26 bones, 33 joints, and over 100 tendons, muscles, and ligaments. Treat them well!",
        "Listening to upbeat music while walking can naturally increase your pace and make the workout feel easier."
    };

    void Start()
    {
        // WE REMOVED HIDE ALL PANELS HERE!
        // The game now trusts that you unchecked them in the Unity Editor.
        
        ShowRandomTip();
    }

    // This turns off every panel so they don't stack on top of each other
    public void HideAllPanels()
    {
        if (missionPanel) missionPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (progressPanel) progressPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (profilePanel) profilePanel.SetActive(false);
    }

    // Call these from your specific HUD buttons
    public void OpenMissionPanel() { HideAllPanels(); missionPanel.SetActive(true); }
    public void OpenSettingsPanel() { HideAllPanels(); settingsPanel.SetActive(true); }
    public void OpenProgressPanel() { HideAllPanels(); progressPanel.SetActive(true); }
    public void OpenLeaderboardPanel() { HideAllPanels(); leaderboardPanel.SetActive(true); }
    public void OpenProfilePanel() { HideAllPanels(); profilePanel.SetActive(true); }

    // Call this from the "Back" arrows inside your new panels
    public void CloseCurrentPanel()
    {
        HideAllPanels();
    }

    public void ShowRandomTip()
    {
        int randomIndex = Random.Range(0, funFacts.Length);
        tipTitleText.text = "Did you know?";
        tipBodyText.text = funFacts[randomIndex];
        tipPopupPanel.SetActive(true); 
    }

    public void CloseTipPopup()
    {
        tipPopupPanel.SetActive(false);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void SwapViewMode()
    {
        SceneManager.LoadScene(alternateViewSceneName);
    }
}