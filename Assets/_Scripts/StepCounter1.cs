using UnityEngine;
using TMPro; // <--- 1. ADDED THIS: Tells Unity we want to talk to the UI text

public class StepCounter1 : MonoBehaviour
{
    [Header("Hardware Settings")]
    [Tooltip("How hard the phone needs to shake to count a step. Default gravity is 1.0.")]
    public float stepThreshold = 2.5f; 
    private bool isStepping = false;

    [Header("UI Elements")]
    public TextMeshProUGUI stepTextDisplay; // <--- 2. ADDED THIS: The blank slot for your text object

    // We need to talk to the FirebaseManager to save the steps
    private FirebaseManager firebaseManager;
    private int currentSteps = 0;

    void Start()
    {
        // Automatically find the FirebaseManager in the scene
        firebaseManager = FindObjectOfType<FirebaseManager>();
    }

    void Update()
    {
        // --- 1. PC TESTING MODE (The Spacebar) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegisterPhysicalStep();
        }

        // --- 2. MOBILE HARDWARE MODE (The Accelerometer) ---
        Vector3 phoneAcceleration = Input.acceleration; 
        float totalForce = phoneAcceleration.sqrMagnitude;

        if (totalForce > stepThreshold && !isStepping)
        {
            isStepping = true;
            RegisterPhysicalStep();
        }
        else if (totalForce < stepThreshold)
        {
            isStepping = false;
        }
    }

    private void RegisterPhysicalStep()
    {
        currentSteps++;
        Debug.Log("Real Step Detected! Session Total: " + currentSteps);

        // <--- 3. ADDED THIS: Update the physical screen!
        if (stepTextDisplay != null)
        {
            stepTextDisplay.text = "Steps: " + currentSteps;
        }

        // Tell Firebase to update the cloud
        if (firebaseManager != null)
        {
            firebaseManager.UpdateStepsInCloud(currentSteps);
        }
    }
}