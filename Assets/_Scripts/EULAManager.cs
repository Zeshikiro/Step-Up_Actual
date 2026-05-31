using UnityEngine;

public class EULAManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject eulaPanel;
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // The panel with Start, Customize, Settings, etc.

    [Header("Validation")]
    public UnityEngine.UI.Toggle acceptToggle;

    // Wire this to your EULA "Accept & Continue" Button's OnClick() event
    public void OnAcceptEulaClicked()
    {
        if (acceptToggle != null && !acceptToggle.isOn)
        {
            Debug.LogWarning("You must check the toggle to accept the EULA!");
            return;
        }

        string userId = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // 1. Mark EULA as accepted so they don't see it again
        PlayerPrefs.SetInt("EulaAccepted_" + userId, 1);
        if (eulaPanel != null) eulaPanel.SetActive(false);

        // 2. Check if this user already completed their BMI setup last time
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
}