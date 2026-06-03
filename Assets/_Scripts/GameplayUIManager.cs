using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Mapbox.Unity.Location;

public class GameplayUIManager : MonoBehaviour
{
    [Header("UI Panels (Drag your panels here!)")]
    public GameObject tipPopupPanel;
    public GameObject missionPanel;      
    public GameObject settingsPanel;     
    public GameObject progressPanel;     
    public GameObject leaderboardPanel;  
    public GameObject profilePanel;     
    public GameObject summaryPanel;  

    [Header("Tip Pop-Up Text")]
    public TextMeshProUGUI tipTitleText;
    public TextMeshProUGUI tipBodyText;

    [Header("Routing Settings")]
    public string mainMenuSceneName = "LoginScene"; 
    public string alternateViewSceneName = "Your3DSceneName"; 

    [Header("Compass Settings")]
    public RectTransform compassUI; // Drag your Compass UI Image here

    [Header("Tip Settings")]
    [Tooltip("How many seconds before a new tip pops up?")]
    public float tipPopupInterval = 60f;

    private string[] funFacts = new string[]
    {
        "Walking 10,000 steps a day burns around 300 to 400 calories, depending on your pace and body weight!",
        "Walking backwards (retro walking) actually burns more calories and helps sharpen your balance.",
        "Brisk walking for just 30 minutes a day can significantly boost your mood and reduce stress.",
        "The human foot has 26 bones, 33 joints, and over 100 tendons, muscles, and ligaments. Treat them well!",
        "Listening to upbeat music while walking can naturally increase your pace and make the workout feel easier."
    };

    private ILocationProvider _locationProvider;

    void Start()
    {
        // Start the continuous looping routine
        StartCoroutine(TipRoutine());

        // Turn on the phone's internal compass sensor and location to allow True North tracking
        Input.compass.enabled = true;
        Input.location.Start();

        // Grab Mapbox's highly accurate location provider which handles device tilt (portrait mode) automatically!
        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }

        // Force the compass pivot to be exactly in the center to fix rotation swings
        if (compassUI != null)
        {
            compassUI.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
    }

    void Update()
    {
        // Rotate the compass UI to match real-world magnetic north
        if (compassUI != null)
        {
            float heading = Input.compass.trueHeading;
            
            // If Mapbox is successfully tracking orientation (which fixes the vertical flipping bug), use it!
            if (_locationProvider != null)
            {
                heading = _locationProvider.CurrentLocation.UserHeading;
            }

            // We use a negative value because Unity's UI Z-axis rotation is counter-clockwise.
            // We use Quaternion.Lerp to smooth out the raw sensor data and eliminate jitter.
            Quaternion targetRotation = Quaternion.Euler(0, 0, -heading);
            compassUI.localRotation = Quaternion.Lerp(compassUI.localRotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private IEnumerator TipRoutine()
    {
        // Wait a few seconds before the very first popup so the map can load
        yield return new WaitForSeconds(5f);
        ShowRandomTip();

        while (true)
        {
            yield return new WaitForSeconds(tipPopupInterval);
            
            // Only pop up if they aren't already looking at a tip
            if (tipPopupPanel != null && !tipPopupPanel.activeSelf) 
            {
                ShowRandomTip();
            }
        }
    }

    // This turns off every panel so they don't stack on top of each other
    public void HideAllPanels()
    {
        if (missionPanel) missionPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (progressPanel) progressPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (profilePanel) profilePanel.SetActive(false);
        if (summaryPanel) summaryPanel.SetActive(false);
    }

    // Call these from your specific HUD buttons
    public void OpenMissionPanel() { HideAllPanels(); missionPanel.SetActive(true); }
    public void OpenSettingsPanel() { HideAllPanels(); settingsPanel.SetActive(true); }
    public void OpenProgressPanel() { HideAllPanels(); progressPanel.SetActive(true); }
    public void OpenLeaderboardPanel() { HideAllPanels(); leaderboardPanel.SetActive(true); }
    public void OpenProfilePanel() { HideAllPanels(); profilePanel.SetActive(true); }
    public void OpenSummaryPanel() { HideAllPanels(); summaryPanel.SetActive(true); }

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

    public void GoToCustomizeScene()
    {
        SceneManager.LoadScene("CustomizeScene");
    }
}