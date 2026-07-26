using UnityEngine;

public class MapZoom : MonoBehaviour
{
    public Camera mapCamera;
    public float zoomSpeed = 0.5f;
    public float minZoom = 20f;  // The closest they can zoom in
    public float maxZoom = 200f; // The furthest they can zoom out

    void Start()
    {
        // Auto-find the camera if you forget to drag it in
        if (mapCamera == null) 
        {
            mapCamera = Camera.main;
        }
    }

    void Update()
    {
        // Upgraded to New Input System touchscreen
        if (UnityEngine.InputSystem.Touchscreen.current == null) return;

        var touches = UnityEngine.InputSystem.Touchscreen.current.touches;
        if (touches.Count < 2) return;

        var touchZero = touches[0];
        var touchOne = touches[1];

        // Only process if both fingers are actively touching
        var phase0 = touchZero.phase.ReadValue();
        var phase1 = touchOne.phase.ReadValue();
        if (phase0 == UnityEngine.InputSystem.TouchPhase.Ended || phase0 == UnityEngine.InputSystem.TouchPhase.None) return;
        if (phase1 == UnityEngine.InputSystem.TouchPhase.Ended || phase1 == UnityEngine.InputSystem.TouchPhase.None) return;

        Vector2 touchZeroPos = touchZero.position.ReadValue();
        Vector2 touchOnePos = touchOne.position.ReadValue();
        Vector2 touchZeroDelta = touchZero.delta.ReadValue();
        Vector2 touchOneDelta = touchOne.delta.ReadValue();

        // Find out how the touches moved since the last frame
        Vector2 touchZeroPrevPos = touchZeroPos - touchZeroDelta;
        Vector2 touchOnePrevPos = touchOnePos - touchOneDelta;

        // Calculate the distance between the fingers
        float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        float currentMagnitude = (touchZeroPos - touchOnePos).magnitude;

        // Difference in distance
        float difference = currentMagnitude - prevMagnitude;

        // Apply the zoom to the Orthographic camera
        mapCamera.orthographicSize -= difference * zoomSpeed;

        // Clamp the zoom so they can't zoom out into outer space or zoom in past the floor
        mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minZoom, maxZoom);
    }
}