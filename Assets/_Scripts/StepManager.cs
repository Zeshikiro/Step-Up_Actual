using UnityEngine;

public class StepManager : MonoBehaviour
{
    [Header("Step Data")]
    public int currentDailySteps = 0;
    public int totalLifetimeSteps = 0;

    [Header("Pedometer Sensitivity")]
    // 1.0 is normal gravity. 1.5 means a slight "jolt" from walking.
    public float stepThreshold = 1.5f; 
    // Phone must settle below this before counting the next step (prevents double-counting)
    public float resetThreshold = 1.0f; 
    
    private bool isStepReady = true;

    void Start()
    {
        // Load the saved steps when the app opens
        currentDailySteps = PlayerPrefs.GetInt("DailySteps", 0);
        totalLifetimeSteps = PlayerPrefs.GetInt("TotalLifetimeSteps", 0);
    }

    void Update()
    {
        // 1. Measure the total force/movement acting on the phone right now
        float acceleration = Input.acceleration.magnitude;

        // 2. Did the phone bounce hard enough to be considered a step?
        if (acceleration > stepThreshold && isStepReady)
        {
            RegisterStep();
            isStepReady = false; // Lock it so it doesn't count 5 steps in one movement
        }
        
        // 3. Has the phone settled down? If yes, unlock ready for the next step
        if (acceleration < resetThreshold)
        {
            isStepReady = true;
        }
    }

    void RegisterStep()
    {
        currentDailySteps++;
        totalLifetimeSteps++;

        // Save the real steps to the phone's hard drive instantly
        PlayerPrefs.SetInt("DailySteps", currentDailySteps);
        PlayerPrefs.SetInt("TotalLifetimeSteps", totalLifetimeSteps);
        PlayerPrefs.Save();
        
        // Optional: If you plug your phone into your PC to debug, you'll see this!
        Debug.Log("Real Step Taken! Total: " + currentDailySteps);
    }
}