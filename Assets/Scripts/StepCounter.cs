using UnityEngine;
using UnityEngine.UI;

public class StepCounter : MonoBehaviour
{
    // We will link this to a UI text box later
    public Text stepText; 
    private int totalSteps = 0;

    void Start()
    {
        // Check if the phone actually has a step sensor
        if (SystemInfo.supportsAccelerometer) 
        {
            Debug.Log("Pedometer/Accelerometer is supported on this device!");
        }
        else
        {
            Debug.LogWarning("No step sensor found!");
        }
    }

    void Update()
    {
        // A simple shake detection to mimic walking for our first test
        if (Input.acceleration.magnitude > 1.5f) 
        {
            totalSteps++;
            Debug.Log("Step taken! Total: " + totalSteps);
        }
    }
}