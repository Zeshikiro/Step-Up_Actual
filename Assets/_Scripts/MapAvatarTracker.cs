using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;

[System.Serializable]
public class IPLocationData
{
    public string status;
    public double lat;
    public double lon;
}

public class MapAvatarTracker : MonoBehaviour
{
    [Header("Mapbox References")]
    public AbstractMap mapManager; // Drag your CitySimulatorMap here

    private ILocationProvider _locationProvider;
    private bool _useFallbackLocation = true;
    private Vector2d _fallbackLatLon;

    // Cinematic Camera Zoom
    private Transform _mainCameraTransform;
    private bool _isZoomingIn = true;
    private float _targetCameraY;

    void Start()
    {
        // Auto-find the map if you forget to drag it in
        if (mapManager == null) 
        {
            mapManager = FindFirstObjectByType<AbstractMap>();
        }

        // Setup the cinematic "Strava" zoom-in animation
        if (Camera.main != null)
        {
            _mainCameraTransform = Camera.main.transform;
            _targetCameraY = _mainCameraTransform.localPosition.y;
            
            // Start the camera 80 units higher in the sky
            Vector3 startPos = _mainCameraTransform.localPosition;
            startPos.y += 80f;
            _mainCameraTransform.localPosition = startPos;

            // Dynamically attach the touch panning script so the user can look around!
            if (_mainCameraTransform.gameObject.GetComponent<MapCameraPanner>() == null)
            {
                _mainCameraTransform.gameObject.AddComponent<MapCameraPanner>();
            }
        }

        // Get the Mapbox GPS Location Provider
        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }

        // Instantly ping the IP Location API to get an indoor coordinate fallback!
        StartCoroutine(FetchIPLocationFallback());

        // --- DYNAMICALLY ADD TRAIL RENDERER ---
        TrailRenderer tr = gameObject.GetComponent<TrailRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TrailRenderer>();
            tr.time = Mathf.Infinity; // Trail lasts forever while app is open
            tr.startWidth = 1.5f;
            tr.endWidth = 1.5f;
            
            // Try to use a basic unlit material so it shows up bright
            Material trailMat = new Material(Shader.Find("Sprites/Default"));
            trailMat.color = new Color(0.2f, 0.8f, 1.0f, 0.8f); // Neon Blue
            tr.material = trailMat;
            
            tr.minVertexDistance = 0.5f; // Drop a trail point every 0.5 meters
            // Make sure the trail renders slightly above ground to avoid Z-fighting with the map
            tr.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
        }

        // --- FIX PIN SIZE ---
        Transform pinVisual = transform.Find("PinVisual");
        if (pinVisual != null)
        {
            // The orange pin is way too small in the screenshot, let's make it 5x bigger
            pinVisual.localScale = new Vector3(5f, 5f, 5f);
        }
    }

    private IEnumerator FetchIPLocationFallback()
    {
        // ip-api is a free endpoint that guesses your location based on your Wi-Fi/Cellular IP Address
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://ip-api.com/json/"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                IPLocationData data = JsonUtility.FromJson<IPLocationData>(webRequest.downloadHandler.text);
                if (data.status == "success")
                {
                    _fallbackLatLon = new Vector2d(data.lat, data.lon);
                    
                    // Force the map to draw immediately at the fallback location!
                    mapManager.Initialize(_fallbackLatLon, mapManager.AbsoluteZoom);
                    Debug.Log($"[MapAvatarTracker] Loaded IP Fallback Location: {data.lat}, {data.lon}");
                }
            }
        }
    }

    void Update()
    {
        if (mapManager == null) return;

        // 1. Handle Cinematic Camera Zoom In Animation IMMEDIATELY
        if (_isZoomingIn && _mainCameraTransform != null)
        {
            Vector3 camPos = _mainCameraTransform.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, _targetCameraY, Time.deltaTime * 2f);
            _mainCameraTransform.localPosition = camPos;

            if (Mathf.Abs(camPos.y - _targetCameraY) < 0.5f)
            {
                camPos.y = _targetCameraY;
                _mainCameraTransform.localPosition = camPos;
                _isZoomingIn = false;
            }
        }

        Vector2d currentLocation = _fallbackLatLon;

        // If the hardware GPS finally locks onto a satellite, it overrides the Wi-Fi fallback
        if (_locationProvider != null && _locationProvider.CurrentLocation.LatitudeLongitude != Vector2d.zero)
        {
            currentLocation = _locationProvider.CurrentLocation.LatitudeLongitude;
            
            // Re-center the map if we just transitioned from the fallback to real GPS
            if (_useFallbackLocation)
            {
                mapManager.UpdateMap(currentLocation, mapManager.AbsoluteZoom);
                _useFallbackLocation = false;
            }
        }

        // If neither GPS nor Fallback is ready, do nothing (wait)
        if (currentLocation == Vector2d.zero) return;

        // 2. Convert real-world GPS into Unity 3D World space
        Vector3 targetPosition = mapManager.GeoToWorldPosition(currentLocation, true);
        
        // Keep the avatar at ground level (Y = 0) so it doesn't fly or sink
        targetPosition.y = 0f;

        // 3. Smoothly move the Avatar to the new location
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }
}

// Bundled into the same file to guarantee compilation
public class MapCameraPanner : MonoBehaviour
{
    public float panSpeed = 2.0f;
    public float snapBackDelay = 3.0f;
    public float snapSpeed = 5.0f;

    private Vector3 _panOffset = Vector3.zero;
    private float _lastTouchTime;
    private bool _isPanning = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isPanning = true;
            _lastTouchTime = Time.time;
        }
        else if (Input.GetMouseButton(0) && _isPanning)
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");

            _panOffset.x -= deltaX * panSpeed;
            _panOffset.z -= deltaY * panSpeed;

            _lastTouchTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isPanning = false;
        }

        if (!_isPanning && Time.time - _lastTouchTime > snapBackDelay)
        {
            _panOffset.x = Mathf.Lerp(_panOffset.x, 0, Time.deltaTime * snapSpeed);
            _panOffset.z = Mathf.Lerp(_panOffset.z, 0, Time.deltaTime * snapSpeed);
        }

        Vector3 localPos = transform.localPosition;
        localPos.x = _panOffset.x;
        localPos.z = _panOffset.z;
        transform.localPosition = localPos;
    }
}
