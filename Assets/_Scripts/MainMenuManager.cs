using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;

    // 1. Starts the core game tracking map scene
    public void StartGame()
    {
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) 
        {
            Debug.Log("USING SCENELOADER TO GO TO SAMPLE SCENE");
            SceneLoader.Instance.LoadScene("SampleScene");
        }
        else 
        {
            Debug.LogWarning("SCENELOADER IS NULL! Bypassing Cutscene...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene"); 
        }
    }

    // 2. Transits cleanly over to your customizer studio scene
    public void OpenCustomize()
    {
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("CustomizeScene");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("CustomizeScene");
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