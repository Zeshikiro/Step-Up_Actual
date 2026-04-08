using UnityEngine;
using TMPro;

public class StepCounter1 : MonoBehaviour
{
    public TextMeshProUGUI stepText; 
    public FirebaseManager firebaseManager;
    private int totalSteps = 0;

    void Start()
    {
        if (SystemInfo.supportsAccelerometer) 
        {
            Debug.Log("Pedometer/Accelerometer is supported!");
        }
        else
        {
            Debug.LogWarning("No step sensor found (PC Testing Mode)");
        }
    }

    void Update()
    {
        // Mobile Shake
        if (SystemInfo.supportsAccelerometer && Input.acceleration.magnitude > 1.5f) 
        {
            RecordStep();
        }

        // PC Spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RecordStep();
        }
    }

    void RecordStep()
    {
        totalSteps++;
        stepText.text = "Steps: " + totalSteps;
        
        // The Handshake
        if (firebaseManager != null) 
        {
            firebaseManager.UpdateStepsInCloud(totalSteps);
        }
    }
}