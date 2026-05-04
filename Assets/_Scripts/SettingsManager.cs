using UnityEngine;
using Firebase.Auth;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;

    // Call this from a "Gear/Settings" button on your Main Menu
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Call this from the "X" button on the Settings Panel
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // Call this from your Log Out button!
    public void LogOutUser()
    {
        // 1. Tell Firebase to terminate the session
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("User successfully logged out of Firebase.");
        }

        // 2. Load the Login Scene so they have to sign in again
        // (Change "LoginScene" to the exact name of your login scene!)
        SceneManager.LoadScene("LoginScene"); 
    }
}