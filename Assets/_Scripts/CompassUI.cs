using UnityEngine;

public class CompassUI : MonoBehaviour
{
    void Update()
    {
        if (Input.compass.enabled)
        {
            // Rotate the 2D UI Image based on real-world compass heading
            transform.localRotation = Quaternion.Euler(0, 0, Input.compass.trueHeading);
        }
    }
}
