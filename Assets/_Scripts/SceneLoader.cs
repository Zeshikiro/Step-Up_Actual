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
        if (Instance == null || Instance == this)
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
        Debug.Log("SCENELOADER TRACE: LoadScene called for " + sceneName);
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        Debug.Log("SCENELOADER TRACE: Starting LoadSceneRoutine. Activating Canvas.");
        
        // 1. Setup UI with Null Checks
        if (loadingScreenCanvas != null) loadingScreenCanvas.SetActive(true);
        else Debug.LogError("SCENELOADER CRITICAL: loadingScreenCanvas is missing from the inspector!");

        if (tapToContinuePrompt != null) tapToContinuePrompt.SetActive(false);
        if (loadingTextPrompt != null) loadingTextPrompt.SetActive(true);
        if (progressBar != null) progressBar.value = 0f;

        // Pick random tip
        if (healthTips.Length > 0 && healthTipText != null)
        {
            healthTipText.text = healthTips[Random.Range(0, healthTips.Length)];
        }

        // 2. Start Async Loading
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        if (operation == null)
        {
            Debug.LogError("SCENELOADER CRITICAL: Failed to start AsyncOperation!");
            yield break;
        }
        
        operation.allowSceneActivation = false; 

        float timeElapsed = 0f;

        // 3. Update Progress Bar
        while (!operation.isDone)
        {
            timeElapsed += Time.deltaTime;
            
            // Calculate real loading progress vs fake time-based progress
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float fakeProgress = Mathf.Clamp01(timeElapsed / minimumLoadingTime);
            
            // Show whichever is smaller so it fills smoothly over 2 seconds
            float displayProgress = Mathf.Min(realProgress, fakeProgress);
            if (progressBar != null) progressBar.value = displayProgress;

            if (operation.progress >= 0.9f && timeElapsed >= minimumLoadingTime)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (loadingTextPrompt != null) loadingTextPrompt.SetActive(false); 

                // Blinking effect for "Tap to continue"
                if (tapToContinuePrompt != null) 
                {
                    // Blinks exactly twice per second
                    bool isBlinking = Mathf.Sin(Time.time * 6f) > 0;
                    tapToContinuePrompt.SetActive(isBlinking);
                }

                // 4. Wait for user input (Upgraded to New Input System!)
                bool hasInput = false;

                if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    hasInput = true;
                }
                
                if (UnityEngine.InputSystem.Touchscreen.current != null && UnityEngine.InputSystem.Touchscreen.current.touches.Count > 0)
                {
                    foreach (var touch in UnityEngine.InputSystem.Touchscreen.current.touches)
                    {
                        if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                        {
                            hasInput = true;
                            break;
                        }
                    }
                }

                if (hasInput || timeElapsed > minimumLoadingTime + 8f) // Auto-skip after 8 seconds of waiting!
                {
                    if (tapToContinuePrompt != null) tapToContinuePrompt.SetActive(true); // Force on before leaving
                    operation.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        // 5. Cleanup
        if (loadingScreenCanvas != null) loadingScreenCanvas.SetActive(false);
    }
}
