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
        // Check if the user is touching the screen with exactly 2 fingers
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find out how the touches moved since the last frame
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Calculate the distance between the fingers
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            // Difference in distance
            float difference = currentMagnitude - prevMagnitude;

            // Apply the zoom to the Orthographic camera
            mapCamera.orthographicSize -= difference * zoomSpeed;

            // Clamp the zoom so they can't zoom out into outer space or zoom in past the floor
            mapCamera.orthographicSize = Mathf.Clamp(mapCamera.orthographicSize, minZoom, maxZoom);
        }
    }
}