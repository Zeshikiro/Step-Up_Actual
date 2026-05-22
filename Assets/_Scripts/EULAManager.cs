using UnityEngine;

public class EULAManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject eulaPanel;
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // The panel with Start, Customize, Settings, etc.

    // Wire this to your EULA "Accept & Continue" Button's OnClick() event
    public void OnAcceptEulaClicked()
    {
        // 1. Mark EULA as accepted so they don't see it again
        PlayerPrefs.SetInt("EulaAccepted", 1);
        if (eulaPanel != null) eulaPanel.SetActive(false);

        // 2. Check if this user already completed their BMI setup last time
        if (PlayerPrefs.GetInt("BMI_Setup_Complete", 0) == 1)
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