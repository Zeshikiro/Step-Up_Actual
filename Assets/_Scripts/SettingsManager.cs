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

        // Route smoothly back to the login environment
        SceneManager.LoadScene("LoginScene");
    }

    // --- HELP & DOCUMENTATION ---

    public void OnLegalClicked()
    {
        if (eulaPanel != null)
        {
            eulaPanel.SetActive(true);
        }
    }

    public void OnChangeEmailClicked()
    {
        Debug.Log("Change Email Clicked! Needs a UI popup to accept new email.");
        // Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UpdateEmailAsync("newemail");
    }

    public void OnPrivacyAndSocialClicked()
    {
        Debug.Log("Privacy and Social Clicked! Add your privacy policy link here.");
        // Application.OpenURL("https://your-privacy-policy-link.com");
    }

    public void OnContactsAndSupportClicked()
    {
        Debug.Log("Contacts and Support Clicked! Opening email client...");
        Application.OpenURL("mailto:support@stepup.com"); // Automatically opens phone's email app!
    }

    public void OnAboutClicked()
    {
        Debug.Log("About Clicked! Step-Up V1.0");
    }
}