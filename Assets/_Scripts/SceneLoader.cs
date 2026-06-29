using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingScreenCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI healthTipText;
    [SerializeField] private GameObject tapToContinuePrompt;
    [SerializeField] private GameObject loadingTextPrompt; // Added for the "Loading..." text
    
    [Header("Settings")]
    [SerializeField] private float minimumLoadingTime = 2.0f; // Forces the screen to stay up a bit so they can read the tip

    [Header("Content")]
    [TextArea(2, 4)]
    [SerializeField] private string[] healthTips = new string[]
    {
        "Did you know? Walking 10,000 steps a day burns around 300 to 400 calories!",
        "Stay hydrated! Drink water before, during, and after your walks.",
        "Walking briskly for 30 minutes a day can improve your cardiovascular fitness.",
        "Take the stairs instead of the elevator to sneak in some extra steps!",
        "Post-walk stretching reduces muscle soreness and improves flexibility.",
        "Did you know? Walking improves your mood by releasing endorphins!",
        "A 15-minute walk after meals helps with digestion and blood sugar control."
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Hide on start
            if (loadingScreenCanvas != null)
                loadingScreenCanvas.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        // 1. Setup UI
        loadingScreenCanvas.SetActive(true);
        if (tapToContinuePrompt != null) tapToContinuePrompt.SetActive(false);
        if (loadingTextPrompt != null) loadingTextPrompt.SetActive(true);
        progressBar.value = 0f;

        // Pick random tip
        if (healthTips.Length > 0 && healthTipText != null)
        {
            healthTipText.text = healthTips[Random.Range(0, healthTips.Length)];
        }

        // 2. Start Async Loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevents Unity from auto-switching when reaching 90%

        float timeElapsed = 0f;

        // 3. Update Progress Bar
        while (!operation.isDone)
        {
            timeElapsed += Time.deltaTime;
            
            // Unity's load progress stops at 0.9. We map it to 0-1 for the slider.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.value = progress;

            // Check if loading is mathematically complete AND our minimum reading time has passed
            if (operation.progress >= 0.9f && timeElapsed >= minimumLoadingTime)
            {
                progressBar.value = 1f;
                if (tapToContinuePrompt != null) tapToContinuePrompt.SetActive(true); // Show blinking text
                if (loadingTextPrompt != null) loadingTextPrompt.SetActive(false); // Hide "Loading..."

                // 4. Wait for user input to finalize
                if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
                {
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        // 5. Cleanup
        loadingScreenCanvas.SetActive(false);
    }
}
