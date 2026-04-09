using UnityEngine;

public class StepCounter1 : MonoBehaviour
{
    [Header("Hardware Settings")]
    [Tooltip("How hard the phone needs to shake to count a step. Default gravity is 1.0.")]
    public float stepThreshold = 2.5f; 
    private bool isStepping = false;

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
        // Grab the physical forces acting on the phone right now (X, Y, and Z axes)
        Vector3 phoneAcceleration = Input.acceleration; 
        
        // Calculate the total force combined (sqrMagnitude is highly efficient for mobile batteries)
        float totalForce = phoneAcceleration.sqrMagnitude;

        // If the force spikes higher than our threshold, it's a step!
        if (totalForce > stepThreshold && !isStepping)
        {
            isStepping = true;
            RegisterPhysicalStep();
        }
        // Reset the trigger once the phone stabilizes so we don't double-count one long shake
        else if (totalForce < stepThreshold)
        {
            isStepping = false;
        }
    }

    // The function that actually adds the step and tells the cloud
    private void RegisterPhysicalStep()
    {
        currentSteps++;
        Debug.Log("Real Step Detected! Session Total: " + currentSteps);

        // Tell Firebase to update the cloud
        if (firebaseManager != null)
        {
            firebaseManager.UpdateStepsInCloud(currentSteps);
        }
    }
}