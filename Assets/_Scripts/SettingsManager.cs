using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio; 
using Firebase.Auth; 

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
            SetVolume(volumeSlider.value); // Apply immediately on startup
        }

        // Load notifications, default to ON
        if (notificationsToggle != null)
        {
            notificationsToggle.isOn = PlayerPrefs.GetInt("NotificationsEnabled", 1) == 1;
            notificationsToggle.onValueChanged.AddListener(ToggleNotifications);
        }
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
            // Failsafe
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // --- HELP & DOCUMENTATION ---

    public void OnLegalClicked()
    {
        if (eulaPanel != null)
        {
            eulaPanel.SetActive(true);
        }
    }

    // RESTORED SIGNATURES: We added the 'string url' back to these methods so the Unity Inspector
    // doesn't lose its "UnityEvent" bindings!
    public void OnChangeEmailClicked(string url)
    {
        Debug.Log("Change Email Clicked! Opening support portal...");
        // Fallback in case Inspector string is blank
        if (string.IsNullOrEmpty(url)) url = "https://stepup.com/support/email-change";
        Application.OpenURL(url);
    }

    public void OnPrivacyAndSocialClicked(string url)
    {
        Debug.Log("Privacy and Social Clicked! Opening URL: " + url);
        if (string.IsNullOrEmpty(url)) url = "https://stepup.com/privacy";
        Application.OpenURL(url);
    }

    public void OnContactsAndSupportClicked()
    {
        Debug.Log("Contacts and Support Clicked! Opening support portal...");
        Application.OpenURL("https://stepup.com/support"); // mailto: often silently fails on Windows 10/11!
    }

    public void OnAboutClicked(string url)
    {
        Debug.Log("About Clicked! Opening URL: " + url);
        if (string.IsNullOrEmpty(url)) url = "https://stepup.com/about";
        Application.OpenURL(url);
    }
}