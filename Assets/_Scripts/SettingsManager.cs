using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Toggle notificationToggle;

    void Start()
    {
        // Load the user's saved settings when the app opens
        if (volumeSlider != null) 
        {
            volumeSlider.value = PlayerPrefs.GetFloat("AppVolume", 1f); // Default to max volume (1)
            AudioListener.volume = volumeSlider.value; // Actually sets the game's volume!
        }
        
        if (notificationToggle != null) 
        {
            // PlayerPrefs can't save bools, so we use 1 for true and 0 for false
            notificationToggle.isOn = PlayerPrefs.GetInt("AppNotifs", 1) == 1; 
        }
    }

    // Call this function whenever the Slider or Toggle changes!
    public void SaveSettings()
    {
        if (volumeSlider != null) 
        {
            PlayerPrefs.SetFloat("AppVolume", volumeSlider.value);
            AudioListener.volume = volumeSlider.value; 
        }
        
        if (notificationToggle != null) 
        {
            PlayerPrefs.SetInt("AppNotifs", notificationToggle.isOn ? 1 : 0);
        }
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SaveSettings(); // Save right before closing!
        settingsPanel.SetActive(false);
    }

    public void LogOutUser()
    {
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
        }
        SceneManager.LoadScene("LoginScene"); 
    }
}