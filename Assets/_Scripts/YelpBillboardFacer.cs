using UnityEngine;

public class YelpBillboardFacer : MonoBehaviour
{
    private Transform _mainCamera;

    void Start()
    {
        if (Camera.main != null)
        {
            _mainCamera = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (_mainCamera != null)
        {
            // Makes the 3D Canvas always face the player's screen!
            transform.LookAt(transform.position + _mainCamera.rotation * Vector3.forward, _mainCamera.rotation * Vector3.up);
        }
    }
}
