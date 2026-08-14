using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ProminentDisclosure : MonoBehaviour
{
    private static ProminentDisclosure instance;
    private GameObject disclosureUI;

    public static void CheckAndShow()
    {
        if (instance == null)
        {
            GameObject obj = new GameObject("ProminentDisclosureManager");
            instance = obj.AddComponent<ProminentDisclosure>();
            DontDestroyOnLoad(obj);
        }

        instance.ExecuteCheck();
    }

    private void ExecuteCheck()
    {
#if UNITY_EDITOR
        // ULTIMATE PC BYPASS: Never show this UI in the Editor, just instantly start the game!
        PlayerPrefs.SetInt("HasSeenDisclosure", 1);
        PlayerPrefs.Save();
#endif

        if (PlayerPrefs.GetInt("HasSeenDisclosure", 0) == 1)
        {
            // Already accepted. Just request Android permissions if missing
            RequestPermissionsSilently();
            NotifySystemsToStart();
            return;
        }

        // Needs to show disclosure UI
        GenerateUI();
    }

    private void GenerateUI()
    {
        if (disclosureUI != null) return;

        // 1. Create Canvas
        disclosureUI = new GameObject("DisclosureCanvas");
        Canvas canvas = disclosureUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Always on top
        disclosureUI.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        disclosureUI.AddComponent<GraphicRaycaster>();

        // 2. Background Panel
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(disclosureUI.transform, false);
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f); // Dark Grey
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 3. Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Background Location & Activity";
        titleText.fontSize = 72;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontStyle = FontStyles.Bold;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.7f);
        titleRect.anchorMax = new Vector2(0.9f, 0.9f);
        titleRect.sizeDelta = Vector2.zero;

        // 4. Body Text (The actual Google Play disclosure)
        GameObject bodyObj = new GameObject("BodyText");
        bodyObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI bodyText = bodyObj.AddComponent<TextMeshProUGUI>();
        bodyText.text = "Step-Up collects location data in the background to track your walks, calculate your distance, and track your mission progress even when the app is closed or not in use.\n\nIt also requires access to your Physical Activity sensor to act as a pedometer and count your daily steps.";
        bodyText.fontSize = 48;
        bodyText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        bodyText.alignment = TextAlignmentOptions.Center;
        bodyText.enableWordWrapping = true;
        RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.1f, 0.3f);
        bodyRect.anchorMax = new Vector2(0.9f, 0.65f);
        bodyRect.sizeDelta = Vector2.zero;

        // 5. I Understand Button
        GameObject buttonObj = new GameObject("AcceptButton");
        buttonObj.transform.SetParent(panelObj.transform, false);
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue
        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(OnAcceptClicked);
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.2f, 0.1f);
        btnRect.anchorMax = new Vector2(0.8f, 0.2f);
        btnRect.sizeDelta = Vector2.zero;

        // Button Text
        GameObject btnTextObj = new GameObject("BtnText");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "I Understand";
        btnText.fontSize = 54;
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontStyle = FontStyles.Bold;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;
    }

#if UNITY_EDITOR
    private void Update()
    {
        // PC TESTING BYPASS: Allow forcefully dismissing via New Input System
        bool keyPressed = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame || 
                UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame)
            {
                keyPressed = true;
            }
        }
        
        bool mouseClicked = false;
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                mouseClicked = true;
            }
        }

        if (disclosureUI != null && (keyPressed || mouseClicked))
        {
            Debug.Log("[ProminentDisclosure] Bypassed via PC Keyboard/Mouse input!");
            OnAcceptClicked();
        }
    }
#endif

    private void OnAcceptClicked()
    {
        PlayerPrefs.SetInt("HasSeenDisclosure", 1);
        PlayerPrefs.Save();
        
        if (disclosureUI != null) Destroy(disclosureUI);

        RequestPermissionsSilently();
        NotifySystemsToStart();
    }

    private void RequestPermissionsSilently()
    {
#if UNITY_ANDROID
        // Request Location
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
        
        // Request Background Location (Required for background tracking)
        // Note: Android 11+ requires Background Location to be requested SEPARATELY after Fine Location is granted.
        // We will just request it here, Unity handles the sequencing on newer API levels if possible.
        // For absolute safety, many developers ask for "android.permission.ACCESS_BACKGROUND_LOCATION".
        
        // Request Activity Recognition
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACTIVITY_RECOGNITION"))
        {
            Permission.RequestUserPermission("android.permission.ACTIVITY_RECOGNITION");
        }
#endif
    }

    private void NotifySystemsToStart()
    {
        // 1. Turn on Compass and Location Tracking
        // Wrapped in try-catch because the legacy Input class may throw
        // if Active Input Handling is set to "Input System Package (New)" exclusively.
        try
        {
            Input.location.Start(0.1f, 0.1f);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[ProminentDisclosure] Legacy Input location unavailable: " + e.Message);
        }

        // 2. Tell StepManager to initialize its sensor
        StepManager stepManager = FindFirstObjectByType<StepManager>();
        if (stepManager != null)
        {
            stepManager.InitializeSensors();
            Debug.Log("[ProminentDisclosure] StepManager.InitializeSensors() called successfully!");
        }
        else
        {
            Debug.LogError("[ProminentDisclosure] CRITICAL: Could not find StepManager in scene!");
        }
    }
}
