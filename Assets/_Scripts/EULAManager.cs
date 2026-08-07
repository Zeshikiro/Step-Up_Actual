using UnityEngine;

public class EULAManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject eulaPanel;
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // The panel with Start, Customize, Settings, etc.

    [Header("Validation")]
    public GameObject warningText; // The red warning text

    void Start()
    {
        // Make sure the warning text is off by default when the scene loads
        if (warningText != null) 
        {
            warningText.SetActive(false);
        }
    }

    public void OnAgreeClicked()
    {
        if (warningText != null) warningText.SetActive(false); // Hide the warning

        // Guarantee the panel closes even if eulaPanel field is unassigned in Inspector!
        if (eulaPanel != null) eulaPanel.SetActive(false);
        gameObject.SetActive(false); 

        // 1. Mark EULA as accepted so they don't see it again
        string userId = "guest";
        if (Firebase.Auth.FirebaseAuth.DefaultInstance != null && Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }

        PlayerPrefs.SetInt("EulaAccepted_" + userId, 1);

        // 2. If they are just reading this from the Settings menu, stop here!
        if (PlayerPrefs.GetInt("OnboardingComplete_" + userId, 0) == 1)
        {
            PlayerPrefs.Save();
            return;
        }

        // 3. Check if this user already completed their BMI setup last time
        if (PlayerPrefs.GetInt("BMI_Setup_Complete_" + userId, 0) == 1)
        {
            Debug.Log("Returning User Detected! Bypassing BMI Panel straight to Main Menu.");
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        }
        else
        {
            Debug.Log("New User Detected! Opening BMI Panel.");
            if (bmiPanel != null) bmiPanel.SetActive(true);
        }

        PlayerPrefs.Save();
    }

    // Wire this to your EULA "DECLINE" Button's OnClick() event
    public void OnDeclineClicked()
    {
        Debug.LogWarning("User declined the EULA.");
        // Show the warning text reminding them they must accept to play
        if (warningText != null) 
        {
            warningText.SetActive(true);
        }
    }
}