using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

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

#if PLATFORM_ANDROID
        // 1. Request Hardware GPS Access immediately on startup
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }
#endif
        // 2. Turn on the hardware compass
        Input.compass.enabled = true;

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

        // --- DYNAMICALLY ADD STRATVA-STYLE TRAIL RENDERER ---
        TrailRenderer tr = gameObject.GetComponent<TrailRenderer>();
        if (tr == null)
        {
            tr = gameObject.AddComponent<TrailRenderer>();
        }
        
        tr.time = Mathf.Infinity; // Trail lasts forever while app is open
        tr.startWidth = 4.0f; // Thick like Strava!
        tr.endWidth = 4.0f;
        tr.numCapVertices = 5; // Perfectly rounded ends
        tr.numCornerVertices = 5; // Perfectly rounded corners when you turn
        
        // Strava Neon Glowing Orange
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = new Color(1.0f, 0.35f, 0.0f, 0.9f); // #fc5a03 (Strava Orange)
        tr.material = trailMat;
        
        tr.minVertexDistance = 1.0f; 
        tr.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

        // --- HIDE (BUT DO NOT DESTROY) THE 3D AVATAR SO AR CAN USE IT ---
        foreach (Transform child in transform)
        {
            if (child.name != "Main Camera" && child.name != "MapCameraPanner" && child.name != "Custom 3D Avatar")
            {
                Destroy(child.gameObject);
            }
        }

        // --- CREATE PREMIUM 2D STRAVA AVATAR ---
        // 1. Outer White Ring
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Avatar_Ring";
        ring.transform.SetParent(transform);
        ring.transform.localPosition = new Vector3(0, 0.1f, 0);
        ring.transform.localScale = new Vector3(3f, 0.01f, 3f); // Flat!
        Destroy(ring.GetComponent<CapsuleCollider>()); // Remove physics
        Material ringMat = new Material(Shader.Find("Unlit/Color"));
        ringMat.color = Color.white;
        ring.GetComponent<MeshRenderer>().material = ringMat;

        // 2. Inner Blue Dot
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dot.name = "Avatar_Dot";
        dot.transform.SetParent(transform);
        dot.transform.localPosition = new Vector3(0, 0.15f, 0); // Slightly higher than ring
        dot.transform.localScale = new Vector3(2.2f, 0.01f, 2.2f);
        Destroy(dot.GetComponent<CapsuleCollider>());
        Material dotMat = new Material(Shader.Find("Unlit/Color"));
        dotMat.color = new Color(0.0f, 0.5f, 1.0f); // Bright blue
        dot.GetComponent<MeshRenderer>().material = dotMat;

        // 3. View Cone (Semi-transparent triangle)
        GameObject cone = new GameObject("ViewCone");
        cone.transform.SetParent(transform);
        cone.transform.localPosition = new Vector3(0, 0.05f, 0); // Lowest layer
        
        MeshRenderer coneRenderer = cone.AddComponent<MeshRenderer>();
        MeshFilter coneFilter = cone.AddComponent<MeshFilter>();
        
        Material coneMat = new Material(Shader.Find("Sprites/Default"));
        coneMat.color = new Color(0.0f, 0.5f, 1.0f, 0.35f); // 35% opacity blue
        coneRenderer.material = coneMat;

        // Draw a flat triangle pointing forward (Z-axis)
        Mesh m = new Mesh();
        m.vertices = new Vector3[] {
            Vector3.zero,
            new Vector3(-4.5f, 0, 9f), // left forward
            new Vector3(4.5f, 0, 9f)   // right forward
        };
        m.triangles = new int[] { 0, 1, 2 }; // Clockwise face
        coneFilter.mesh = m;
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

        // --- DESTROY PESKY DEFAULT MAPBOX RED PINS ---
        GameObject mapboxPin1 = GameObject.Find("LocationProvider(Clone)");
        if (mapboxPin1 != null) Destroy(mapboxPin1);
        GameObject mapboxPin2 = GameObject.Find("LocationPrefab(Clone)");
        if (mapboxPin2 != null) Destroy(mapboxPin2);

        // 2. Convert real-world GPS into Unity 3D World space
        Vector3 targetPosition = mapManager.GeoToWorldPosition(currentLocation, true);
        
        // Keep the avatar at ground level (Y = 0) so it doesn't fly or sink
        targetPosition.y = 0f;

        // 3. Smoothly move the Avatar to the new location
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);

        // 4. Sync View Cone to Real-World Compass Heading!
        Transform viewCone = transform.Find("ViewCone");
        if (viewCone != null)
        {
            // trueHeading is 0 when facing North, 90 East, 180 South. 
            float targetAngle = Input.compass.trueHeading;
            float currentAngle = viewCone.eulerAngles.y;
            float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
            
            viewCone.eulerAngles = new Vector3(0, smoothAngle, 0);
            
            // Also rotate the blue dot so the avatar itself faces the right way
            Transform dot = transform.Find("Avatar_Dot");
            if (dot != null) dot.eulerAngles = new Vector3(0, smoothAngle, 0);
        }
    }
}

// Bundled into the same file to guarantee compilation
public class MapCameraPanner : MonoBehaviour
{
    public float panSpeed = 18.0f; // x3 speed as requested!
    public float rotationSpeed = 0.5f;
    public float snapBackDelay = 3.0f;
    public float snapSpeed = 5.0f;

    private Vector3 _panOffset = Vector3.zero;
    private float _rotationAngle = 0f;
    private float _lastTouchTime;
    private bool _isPanning = false;

    void Update()
    {
        // --- MOBILE TOUCH CONTROLS ---
        if (Input.touchCount == 1)
        {
            // 1 Finger: Pan the Map
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                // Calculate movement delta
                Vector3 panDelta = new Vector3(-touch.deltaPosition.x, 0, -touch.deltaPosition.y) * panSpeed * 0.05f;
                
                // Rotate the movement direction so it matches the camera's current twisted angle
                panDelta = Quaternion.Euler(0, _rotationAngle, 0) * panDelta;
                
                _panOffset += panDelta;
                _lastTouchTime = Time.time;
                _isPanning = true;
            }
        }
        else if (Input.touchCount == 2)
        {
            // 2 Fingers: Twist to Rotate
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                // Calculate angle change between the two fingers
                Vector2 prevDir = (t1.position - t1.deltaPosition) - (t2.position - t2.deltaPosition);
                Vector2 currDir = t1.position - t2.position;

                float angle = Vector2.SignedAngle(prevDir, currDir);
                _rotationAngle += angle * rotationSpeed;
                _lastTouchTime = Time.time;
                _isPanning = true;
            }
        }
        // --- PC MOUSE FALLBACK FOR TESTING ---
        else if (Input.GetMouseButton(0) && Input.touchCount == 0)
        {
            float deltaX = Input.GetAxis("Mouse X");
            float deltaY = Input.GetAxis("Mouse Y");
            if (Mathf.Abs(deltaX) > 0.01f || Mathf.Abs(deltaY) > 0.01f)
            {
                Vector3 panDelta = new Vector3(-deltaX, 0, -deltaY) * panSpeed * 2f;
                panDelta = Quaternion.Euler(0, _rotationAngle, 0) * panDelta;
                
                _panOffset += panDelta;
                _lastTouchTime = Time.time;
                _isPanning = true;
            }
        }

        // Release touches
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            _isPanning = false;
        }

        // Apply Panning Position (Removed auto-snap so you can explore infinitely!)
        Vector3 localPos = transform.localPosition;
        
        // Smoothly lerp towards the pan offset for a buttery smooth feel
        localPos.x = Mathf.Lerp(localPos.x, _panOffset.x, Time.deltaTime * snapSpeed);
        localPos.z = Mathf.Lerp(localPos.z, _panOffset.z, Time.deltaTime * snapSpeed);
        transform.localPosition = localPos;

        // Apply Camera Rotation
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, _rotationAngle, transform.localEulerAngles.z);
    }

    // Call this from a UI Button to instantly snap back to the player!
    public void RecenterCamera()
    {
        _panOffset = Vector3.zero;
        _rotationAngle = 0f;
    }
}
