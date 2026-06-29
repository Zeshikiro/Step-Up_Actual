using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTutorialManager : MonoBehaviour
{
    [Header("Cross-Scene Chaining")]
    [Tooltip("If true, shows the Yes/No prompt. If false, auto-starts the tutorial if a tutorial tour is in progress.")]
    public bool isFirstSceneOfTutorial = true;
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
        }
        else
        {
            // This is a chained scene! Did they accept the tour in the first scene?
            if (PlayerPrefs.GetInt("Tutorial_InProgress", 0) == 1)
            {
                // Auto-start without asking!
                StartTutorialSequence();
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
    }

    public void EndTutorial()
    {
        HideAllSteps();

        if (loadSceneOnComplete && !string.IsNullOrEmpty(nextSceneToLoad))
        {
            // Teleport to the next scene in the chain!
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(nextSceneToLoad);
            }
            else
            {
                SceneManager.LoadSceneAsync(nextSceneToLoad);
            }
        }
        else
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
    }

    private void HideAllSteps()
    {
        foreach (GameObject step in tutorialSteps)
        {
            if (step != null) step.SetActive(false);
        }
    }
}
