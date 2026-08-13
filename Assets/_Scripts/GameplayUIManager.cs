using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Mapbox.Unity.Location;

public class GameplayUIManager : MonoBehaviour
{
    void Awake()
    {
        // Fix for Android hardware buttons (Volume Down / Power) triggering UI clicks during screenshots!
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = false;
        }
    }

    [Header("UI Panels (Drag your panels here!)")]
    public GameObject tipPopupPanel;
    public GameObject missionPanel;      
    public GameObject settingsPanel;     
    public GameObject leaderboardPanel;  
    public GameObject profilePanel;     
    public GameObject summaryPanel;  
    public GameObject customizerPanel;

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
        // FPS is now dynamically managed by SetOptimalFPS() instead of hardcoded to 30.
        SetOptimalFPS();

        // Force all panels to turn off immediately so they don't block the map!
        HideAllPanels();
        if (tipPopupPanel != null) tipPopupPanel.SetActive(false);
        if (closePopupBackground != null) closePopupBackground.SetActive(false);

        // Temporarily disabled pending leader feedback so it doesn't interrupt the main game
        // StartCoroutine(TipRoutine());

        // Start checking internet connection periodically, not every frame!
        StartCoroutine(InternetCheckRoutine());

        // Ask for permissions using the Google Play Prominent Disclosure first!
        ProminentDisclosure.CheckAndShow();

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
        if (Camera.main != null && compassUI != null)
        {
            // If camera twists to the right (positive Y), map North goes left on screen (positive Z)
            float cameraYRotation = Camera.main.transform.eulerAngles.y;
            compassUI.localRotation = Quaternion.Euler(0, 0, cameraYRotation);
        }
    }

    private GameObject noInternetPanel;

    private IEnumerator InternetCheckRoutine()
    {
        while (true)
        {
            CheckInternetConnection();
            yield return new WaitForSeconds(3.0f); // Check every 3 seconds instead of 60 times a second
        }
    }

    private void CheckInternetConnection()
    {
        bool hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
        
        // --- OFFLINE SYNC FIX: Inform StepManager if internet was just restored ---
        if (hasInternet && noInternetPanel != null && noInternetPanel.activeSelf)
        {
            // We just got internet back! Trigger a cloud sync for our local steps.
            StepManager sm = FindFirstObjectByType<StepManager>();
            if (sm != null) sm.ForceCloudSync();
        }

        if (!hasInternet && noInternetPanel == null)
        {
            // Dynamically create a NON-BLOCKING No Internet banner
            noInternetPanel = new GameObject("NoInternetPanel");
            Canvas canvas = noInternetPanel.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9998; // High order to be visible, but we will disable raycasts so you can still click GUIs!
            noInternetPanel.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(noInternetPanel.transform, false);
            UnityEngine.UI.Image img = bg.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Semi-transparent dark grey
            img.raycastTarget = false; // CRITICAL: Allows clicks to pass through to the GUI!
            
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.85f); // Top 15% of the screen
            bgRect.anchorMax = new Vector2(1, 1);
            bgRect.sizeDelta = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(bg.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "OFFLINE MODE\nMap unavailable, but steps & missions still work!";
            text.fontSize = 35;
            text.color = new Color(1f, 0.4f, 0.4f); // Reddish warning color
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false; // Allows clicks to pass through
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
        }

        if (noInternetPanel != null)
        {
            noInternetPanel.SetActive(!hasInternet);
        }
    }

    private IEnumerator TipRoutine()
    {
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
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (profilePanel) profilePanel.SetActive(false);
        if (summaryPanel) summaryPanel.SetActive(false);
        if (customizerPanel) customizerPanel.SetActive(false);
    }

    // Call these from your specific HUD buttons
    public void OpenMissionPanel() { TogglePanel(missionPanel); }
    public void OpenSettingsPanel() { TogglePanel(settingsPanel); }
    public void ToggleLeaderboard()
    {
        TogglePanel(leaderboardPanel);
    }
    
    // Helper to manage FPS when opening/closing UI
    private void TogglePanel(GameObject panel)
    {
        if (panel == null) return;
        
        bool willBeActive = !panel.activeSelf;
        
        if (willBeActive)
        {
            HideAllPanels(); // Close others first
            panel.SetActive(true);
        }
        else
        {
            panel.SetActive(false);
        }
        
        SetOptimalFPS(); // Adjust FPS based on whether UI is open
    }
    
    /// <summary>
    /// Dynmically switches FPS to save battery when idle, but provides smooth 60fps for UI and AR
    /// </summary>
    public void SetOptimalFPS()
    {
        bool isARActive = false;
        ARManager ar = FindFirstObjectByType<ARManager>();
        if (ar != null && ar.IsARMode) isARActive = true;
        
        bool isUIOpen = (missionPanel != null && missionPanel.activeSelf) ||
                        (settingsPanel != null && settingsPanel.activeSelf) ||
                        (leaderboardPanel != null && leaderboardPanel.activeSelf) ||
                        (profilePanel != null && profilePanel.activeSelf) ||
                        (summaryPanel != null && summaryPanel.activeSelf) ||
                        (customizerPanel != null && customizerPanel.activeSelf);
                        
        if (isARActive || isUIOpen)
        {
            Application.targetFrameRate = 60; // Smooth for UI and Camera
        }
        else
        {
            Application.targetFrameRate = 30; // Battery saver for Mapbox
        }
    }

    public void OpenProfilePanel() { TogglePanel(profilePanel); }
    public void OpenSummaryPanel() { TogglePanel(summaryPanel); }
    public void OpenCustomizerPanel() { TogglePanel(customizerPanel); InventoryManager.Instance?.GenerateInventoryUI(); }

    // Call this from the "Back" arrows inside your new panels
    public void CloseCurrentPanel()
    {
        HideAllPanels();
    }

    public GameObject closePopupBackground; // Drag your ClosePupUp button here!

    public void ShowRandomTip()
    {
        int randomIndex = Random.Range(0, funFacts.Length);
        tipTitleText.text = "Did you know?";
        tipBodyText.text = funFacts[randomIndex];
        tipPopupPanel.SetActive(true); 
        if (closePopupBackground != null) closePopupBackground.SetActive(true);
    }

    public void CloseTipPopup()
    {
        tipPopupPanel.SetActive(false);
        if (closePopupBackground != null) closePopupBackground.SetActive(false);
    }

    private void ShutdownMapboxGracefully()
    {
        // 🚨 CRITICAL FIX: Disable Mapbox before leaving the scene to instantly abort 
        // background tile-download threads. This prevents Unity Editor from freezing!
        MapAvatarTracker tracker = FindFirstObjectByType<MapAvatarTracker>();
        if (tracker != null && tracker.mapManager != null)
        {
            tracker.mapManager.gameObject.SetActive(false);
            Debug.Log("[GameplayUIManager] Mapbox gracefully shutdown to prevent Editor crash.");
        }
    }

    public void ReturnToMainMenu()
    {
        ShutdownMapboxGracefully();
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) 
        {
            Debug.Log("USING SCENELOADER TO GO TO MAIN MENU");
            SceneLoader.Instance.LoadScene(mainMenuSceneName);
        }
        else 
        {
            Debug.LogWarning("SCENELOADER IS NULL! Bypassing Cutscene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void SwapViewMode()
    {
        ShutdownMapboxGracefully();
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(alternateViewSceneName);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(alternateViewSceneName);
    }

    public void GoToCustomizeScene()
    {
        ShutdownMapboxGracefully();
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) 
        {
            Debug.Log("USING SCENELOADER TO GO TO CUSTOMIZE");
            SceneLoader.Instance.LoadScene("CustomizeScene");
        }
        else 
        {
            Debug.LogWarning("SCENELOADER IS NULL! Bypassing Cutscene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("CustomizeScene");
        }
    }

    public void RecenterMap()
    {
        if (Camera.main != null)
        {
            MapCameraPanner panner = Camera.main.GetComponent<MapCameraPanner>();
            if (panner != null)
            {
                panner.RecenterCamera();
            }
        }
    }
}