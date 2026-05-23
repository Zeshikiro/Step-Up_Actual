using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    // This allows you to drag the Settings Panel from the Hierarchy into this slot
    public GameObject settingsPanel;

    public void StartGame()
    {
        SceneManager.LoadScene("SampleScene"); 
    }

    // Logic to open the panel
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Settings Panel is not assigned in the Inspector!");
        }
    }

    // Logic to close the panel (useful for your Close button)
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenCustomize()
    {
        Debug.Log("Opening Customize Menu...");
    }

    public void OpenHealthTips()
    {
        Debug.Log("Opening Health Tips...");
    }
}