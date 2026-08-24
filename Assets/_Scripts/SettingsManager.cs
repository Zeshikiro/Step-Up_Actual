using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; 
using Firebase.Auth; 
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI & System References")]
    public Slider volumeSlider;
    public Toggle notificationsToggle;
    public GameObject eulaPanel;     // Slot for your EULA Panel
    public AudioMixer masterMixer;   // Slot for your Audio Mixer
    public GameObject settingsPanel; // Drag your overall Settings Panel here
    public GameObject bmiPanel;      // Slot for your BMI Panel!

        private void Start()
    {
        // Load volume, default to 75%
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Auto-find panels if they are missing (crucial for prefabs used across multiple scenes)
        if (bmiPanel == null) bmiPanel = FindInactivePanel("BMI Panel");
        if (eulaPanel == null) eulaPanel = FindInactivePanel("EULA Panel");
        if (settingsPanel == null) settingsPanel = gameObject; // Fallback to itself

        // --- FOOLPROOF BUTTON BINDING ---
        // We bind ALL buttons dynamically in code based on their text! 
        // This ensures the prefab NEVER breaks when spawned in new scenes!
        if (settingsPanel != null)
        {
            Button[] allButtons = settingsPanel.GetComponentsInChildren<Button>(true);
            foreach (Button b in allButtons)
            {
                TMP_Text t = b.GetComponentInChildren<TMP_Text>();
                if (t != null)
                {
                    string txt = t.text.ToLower();
                    
                    // Nuke all Inspector bindings so we don't double-fire
                    b.onClick.RemoveAllListeners();

                    if (txt.Contains("email")) b.onClick.AddListener(() => OnChangeEmailClicked(""));
                    else if (txt.Contains("privacy") || txt.Contains("social")) b.onClick.AddListener(() => OnPrivacyAndSocialClicked(""));
                    else if (txt.Contains("bmi")) b.onClick.AddListener(OnUpdateBMIClicked);
                    else if (txt.Contains("log out")) b.onClick.AddListener(OnLogOutClicked);
                    else if (txt.Contains("contacts")) b.onClick.AddListener(OnContactsAndSupportClicked);
                    else if (txt.Contains("legal") || txt.Contains("eula")) b.onClick.AddListener(OnLegalClicked);
                    else if (txt.Contains("about")) b.onClick.AddListener(() => OnAboutClicked(""));
                }
            }
        }
        
        if (volumeSlider != null) SetVolume(volumeSlider.value); // Apply immediately on startup

        // Load notifications, default to ON
        if (notificationsToggle != null)
        {
            notificationsToggle.isOn = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
            notificationsToggle.onValueChanged.AddListener(ToggleNotifications);
        }
    }

    private GameObject FindInactivePanel(string panelName)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == panelName && t.gameObject.scene.isLoaded) return t.gameObject;
        }
        return null;
    }

    // --- FITNESS & PREFERENCES ---

    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat("MasterVolume", volume);
        
        if (masterMixer != null)
        {
            // Convert the 0-1 slider value to a logarithmic dB scale (-80 to 0)
            float dB = volume > 0.001f ? Mathf.Log10(volume) * 20f : -80f;
            masterMixer.SetFloat("MasterVolume", dB);
        }
    }

    public void OnUpdateBMIClicked()
    {
        if (bmiPanel != null)
        {
            bmiPanel.SetActive(true); // Opens the BMI Panel!
        }
    }
    
    public void OnCloseSettingsClicked()
    {
    if (settingsPanel != null)
    {
        settingsPanel.SetActive(false); // Shuts down the panel view
    }
    }

    public void ToggleNotifications(bool isOn)
    {
    PlayerPrefs.SetInt("NotificationsEnabled", isOn ? 1 : 0);
    PlayerPrefs.Save();

    // Look for our Notification Manager in the scene and tell it to recalculate
    NotificationManager notificationMgr = FindFirstObjectByType<NotificationManager>();
    if (notificationMgr != null)
    {
        notificationMgr.RefreshNotificationSchedule();
    }
    }

    // --- ACCOUNT SECURITY ---

        public void OnLogOutClicked()
    {
        Debug.Log("Signing out of Firebase...");
        
        // Disconnect from Firebase
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }

        // Instead of reloading the volatile scene, let's gracefully shut down the UI and pop the Login Panel!
        GameplayUIManager ui = FindFirstObjectByType<GameplayUIManager>();
        if (ui != null) ui.HideAllPanels();

        AuthManager authManager = FindFirstObjectByType<AuthManager>();
        if (authManager != null && authManager.loginPanel != null)
        {
            authManager.loginPanel.SetActive(true);
        }
        else
        {
            // If we are in SampleScene or CustomizeScene, force a hard load to LoginScene!
            UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
        }
    }

    // --- HELP & DOCUMENTATION ---

    public void OnLegalClicked()
    {
        if (eulaPanel != null)
        {
            eulaPanel.SetActive(true);
            EULAManager eulaManager = eulaPanel.GetComponent<EULAManager>();
            if (eulaManager != null) eulaManager.openedFromSettings = true;
        }
    }

    // RESTORED SIGNATURES: We added the 'string url' back to these methods so the Unity Inspector
    // doesn't lose its "UnityEvent" bindings!
    public void OnChangeEmailClicked(string url)
    {
        Debug.Log("Change Email Clicked! Opening support portal...");
        // Fallback in case Inspector string is blank
        if (string.IsNullOrEmpty(url)) url = "https://step-up-actual.vercel.app/?tab=change-email";
        Application.OpenURL(url);
    }

    public void OnPrivacyAndSocialClicked(string url)
    {
        Debug.Log("Privacy and Social Clicked! Opening URL: " + url);
        if (string.IsNullOrEmpty(url)) url = "https://step-up-actual.vercel.app/?tab=privacy";
        Application.OpenURL(url);
    }

    public void OnContactsAndSupportClicked()
    {
        Debug.Log("Contacts and Support Clicked! Opening Email Client...");
        Application.OpenURL("mailto:stepup.app.project@gmail.com?subject=Step-Up%20App%20Support");
    }

    public void OnAboutClicked(string url)
    {
        Debug.Log("About Clicked! Opening URL: " + url);
        if (string.IsNullOrEmpty(url)) url = "https://stepup.com/about";
        Application.OpenURL(url);
    }
}

