using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;

    // 1. Starts the core game tracking map scene
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("SampleScene"); 
    }

    // 2. Transits cleanly over to your customizer studio scene
    public void OpenCustomize()
    {
        SceneManager.LoadSceneAsync("CustomizeScene");
    }

    // 3. Settings Canvas Toggles
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

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenHealthTips()
    {
        Debug.Log("Opening Health Tips...");
        // Add scene load or panel toggle here when ready!
    }

    // 4. Opens a web browser to the provided URL (e.g., About Us website)
    public void OpenWebsite(string url)
    {
        Application.OpenURL(url);
    }
}