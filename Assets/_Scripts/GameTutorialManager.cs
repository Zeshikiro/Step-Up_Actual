using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTutorialManager : MonoBehaviour
{
    [Header("Cross-Scene Chaining")]
    [Tooltip("If true, shows the Yes/No prompt. If false, auto-starts the tutorial if a tutorial tour is in progress.")]
    public bool isFirstSceneOfTutorial = true;
    [Tooltip("If true, this marks the absolute end of the cross-scene tour. It will wipe the progress flag and give the coin reward BEFORE teleporting.")]
    public bool isLastSceneOfTutorial = false;
    [Tooltip("If true, teleport to the next scene when this tutorial finishes.")]
    public bool loadSceneOnComplete = false;
    [Tooltip("The name of the next scene to load (e.g. SampleScene)")]
    public string nextSceneToLoad = "";

    [Header("UI References")]
    public GameObject tutorialPromptPanel; // The "Do you want a tutorial?" Yes/No box
    public GameObject[] tutorialSteps;     // Array of panels for each step of the tutorial

    private int currentStepIndex = 0;
    private string saveKey;

    void Start()
    {
        // Unique save key so we know if they've seen the tutorial globally
        saveKey = "HasSeenGlobalTutorial";

        HideAllSteps();
        if (tutorialPromptPanel != null) tutorialPromptPanel.SetActive(false);

        if (isFirstSceneOfTutorial)
        {
            // If they have never seen the tutorial prompt ever, show it
            if (PlayerPrefs.GetInt(saveKey, 0) == 0)
            {
                if (tutorialPromptPanel != null) tutorialPromptPanel.SetActive(true);
            }
            else
            {
                // CRITICAL FIX: If they already finished the tutorial, completely shut off the 
                // TutorialHolder folder so its invisible background stops blocking the Main Menu buttons!
                gameObject.SetActive(false);
            }
        }
        else
        {
            // This is a chained scene! Did they accept the tour in the first scene?
            if (PlayerPrefs.GetInt("Tutorial_InProgress", 0) == 1)
            {
                // Auto-start without asking!
                StartTutorialSequence();
            }
            else
            {
                // CRITICAL FIX: Not in a tutorial tour? Shut off the raycast shield!
                gameObject.SetActive(false);
            }
        }
    }

    public void AcceptTutorial()
    {
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.SetInt("Tutorial_InProgress", 1); // The tour has begun!
        PlayerPrefs.Save();

        if (tutorialPromptPanel != null) tutorialPromptPanel.SetActive(false);

        StartTutorialSequence();
    }

    public void DeclineTutorial()
    {
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.SetInt("Tutorial_InProgress", 0);
        PlayerPrefs.Save();

        if (tutorialPromptPanel != null) tutorialPromptPanel.SetActive(false);
        HideAllSteps();

        // CRITICAL FIX: Destroy the invisible shield immediately if they click No!
        gameObject.SetActive(false);
    }

    private void StartTutorialSequence()
    {
        currentStepIndex = 0;
        if (tutorialSteps.Length > 0 && tutorialSteps[0] != null)
        {
            tutorialSteps[0].SetActive(true);
        }
        else 
        {
            EndTutorial();
        }
    }

    public void NextStep()
    {
        if (currentStepIndex < tutorialSteps.Length && tutorialSteps[currentStepIndex] != null)
        {
            tutorialSteps[currentStepIndex].SetActive(false);
        }

        currentStepIndex++;

        if (currentStepIndex < tutorialSteps.Length && tutorialSteps[currentStepIndex] != null)
        {
            tutorialSteps[currentStepIndex].SetActive(true);
        }
        else
        {
            EndTutorial();
        }
    }

    // Call this from a "Skip Tutorial" button inside your tutorial panels
    public void SkipEntireTutorial()
    {
        PlayerPrefs.SetInt("Tutorial_InProgress", 0);
        PlayerPrefs.Save();
        HideAllSteps();

        // CRITICAL FIX: Destroy the invisible shield immediately if they skip!
        gameObject.SetActive(false);
    }

    public void EndTutorial()
    {
        HideAllSteps();

        // 🚨 CRITICAL FIX: Gracefully shut down Mapbox before leaving ANY scene via the tutorial to prevent Android background crashes!
        var tracker = UnityEngine.Object.FindFirstObjectByType<MapAvatarTracker>();
        if (tracker != null && tracker.mapManager != null)
        {
            tracker.mapManager.gameObject.SetActive(false);
            Debug.Log("[GameTutorialManager] Mapbox gracefully shutdown before teleporting.");
        }

        if (isLastSceneOfTutorial)
        {
            // The tour is officially over! Wipe the progress flag so it doesn't auto-start next time they enter a scene
            PlayerPrefs.SetInt("Tutorial_InProgress", 0);
            PlayerPrefs.Save();

            // Reward the player with 100 coins for finishing the whole tour!
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddCoins(100);
                Debug.Log("[Tutorial] Player finished the tour! Rewarded 100 Coins!");
            }
        }

        if (loadSceneOnComplete && !string.IsNullOrEmpty(nextSceneToLoad))
        {
            // Teleport to the next scene in the chain safely via the loading screen!
            if (SceneLoader.Instance == null) 
            {
                SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
                if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(nextSceneToLoad);
            }
            else
            {
                Debug.LogWarning("SCENELOADER IS NULL! Bypassing Cutscene...");
                SceneManager.LoadScene(nextSceneToLoad); // Use synchronous load if loader is totally missing to prevent async OOM crash
            }
        }
        else 
        {
            if (!isLastSceneOfTutorial)
            {
                // Just in case it wasn't checked, but we are stopping here
                PlayerPrefs.SetInt("Tutorial_InProgress", 0);
                PlayerPrefs.Save();
            }

            // CRITICAL FIX: If we are staying in the exact same scene, destroy the invisible shield!
            gameObject.SetActive(false);
        }
    }

    private void HideAllSteps()
    {
        foreach (GameObject step in tutorialSteps)
        {
            if (step != null) step.SetActive(false);
        }
    }
}
