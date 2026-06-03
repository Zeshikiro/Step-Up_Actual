using UnityEngine;

public class MapCameraPanner : MonoBehaviour
{
    [Header("Panning Settings")]
    public float panSpeed = 2.0f;
    public float snapBackDelay = 3.0f;
    public float snapSpeed = 5.0f;

    private Vector3 _panOffset = Vector3.zero;
    private float _lastTouchTime;
    private bool _isPanning = false;

    void Update()
    {
        // Handle Mouse or Touch dragging
        if (Input.GetMouseButtonDown(0))
        {
            _isPanning = true;
            _lastTouchTime = Time.time;
        }
        else if (Input.GetMouseButton(0) && _isPanning)
        {
            // Unity's "Mouse X/Y" translates perfectly to physical touch swipes on Mobile
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            _panOffset.x -= deltaX * panSpeed;
            _panOffset.z -= deltaY * panSpeed; // Move along the Z axis (forward/back) instead of Y (up/down)

            _lastTouchTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isPanning = false;
        }

        // Snap back to avatar after no input
        if (!_isPanning && Time.time - _lastTouchTime > snapBackDelay)
        {
            _panOffset.x = Mathf.Lerp(_panOffset.x, 0, Time.deltaTime * snapSpeed);
            _panOffset.z = Mathf.Lerp(_panOffset.z, 0, Time.deltaTime * snapSpeed);
        }

        // Apply the panning offset but preserve the Y coordinate so the Cinematic Zoom doesn't break
        Vector3 localPos = transform.localPosition;
        localPos.x = _panOffset.x;
        localPos.z = _panOffset.z;
        transform.localPosition = localPos;
    }
}
