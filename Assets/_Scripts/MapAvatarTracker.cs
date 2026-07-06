using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;
using Mapbox.Utils;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.EventSystems;

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

    // Safety Cooldown
    private Vector2d _lastRecenteredGPS = Vector2d.zero;

    // Saved Trail System
    private System.Collections.Generic.List<Vector2d> _savedGPSPoints = new System.Collections.Generic.List<Vector2d>();
    private float _lastSaveTime;

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
        tr.startWidth = 1.5f; // Thinner and sleeker
        tr.endWidth = 1.5f;
        tr.numCapVertices = 5; // Perfectly rounded ends
        tr.numCornerVertices = 5; // Perfectly rounded corners when you turn
        
        // Strava Neon Glowing Orange
        Material trailMat = new Material(Shader.Find("Sprites/Default"));
        trailMat.color = new Color(1.0f, 0.35f, 0.0f, 0.9f); // #fc5a03 (Strava Orange)
        tr.material = trailMat;
        
        tr.minVertexDistance = 0.5f; // Filters out small GPS jitters for a clean line
        tr.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

        // --- DYNAMICALLY ADD LINE RENDERER FOR SAVED TRAIL HISTORY ---
        LineRenderer lr = gameObject.GetComponent<LineRenderer>();
        if (lr == null) lr = gameObject.AddComponent<LineRenderer>();
        lr.startWidth = 1.5f;
        lr.endWidth = 1.5f;
        lr.numCapVertices = 5;
        lr.numCornerVertices = 5;
        lr.material = trailMat; // Reuse Strava Orange
        lr.useWorldSpace = true;

        // Load Saved GPS Points for the Trail
        string savedTrail = PlayerPrefs.GetString("SavedGPSTrail", "");
        if (!string.IsNullOrEmpty(savedTrail))
        {
            string[] points = savedTrail.Split('|');
            foreach (string p in points)
            {
                if (string.IsNullOrEmpty(p)) continue;
                string[] coords = p.Split(',');
                if (coords.Length == 2 && double.TryParse(coords[0], out double lat) && double.TryParse(coords[1], out double lon))
                {
                    _savedGPSPoints.Add(new Vector2d(lat, lon));
                }
            }
        }
        
        // INSTANTLY SPAWN AT LAST KNOWN GPS INSTEAD OF WAITING FOR WI-FI FALLBACK!
        if (_savedGPSPoints.Count > 0)
        {
            _fallbackLatLon = _savedGPSPoints[_savedGPSPoints.Count - 1];
            try { mapManager.Initialize(_fallbackLatLon, mapManager.AbsoluteZoom); }
            catch { mapManager.UpdateMap(_fallbackLatLon, mapManager.AbsoluteZoom); }
        }

        // (Removed the child destruction loop so your 2D map pin doesn't get deleted!)

        // We no longer generate a 2D Blue Dot Avatar!
        // The Custom 3D Avatar child object will be used instead.
        
        CreateVisionCone();
    }

    private void CreateVisionCone()
    {
        // Dynamically build a "Google Maps" style blue vision cone
        GameObject coneObj = new GameObject("VisionCone");
        Transform custom3DAvatar = transform.Find("Custom 3D Avatar");
        coneObj.transform.SetParent(custom3DAvatar != null ? custom3DAvatar : transform);
        coneObj.transform.localPosition = new Vector3(0, 0.2f, 0); // Slightly above ground
        coneObj.transform.localEulerAngles = Vector3.zero;

        MeshFilter mf = coneObj.AddComponent<MeshFilter>();
        MeshRenderer mr = coneObj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[3];
        vertices[0] = Vector3.zero; // Base at player center
        vertices[1] = new Vector3(-2f, 0, 5f); // Left edge of cone
        vertices[2] = new Vector3(2f, 0, 5f);  // Right edge of cone

        int[] triangles = new int[] { 0, 1, 2, 0, 2, 1 }; // Double sided just in case
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        // Create a transparent blue material
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = new Color(0.0f, 0.5f, 1.0f, 0.4f);
        mr.material = mat;
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
                    try 
                    {
                        mapManager.Initialize(_fallbackLatLon, mapManager.AbsoluteZoom);
                    }
                    catch 
                    {
                        // If it was already initialized by the Unity Inspector, just update it instead!
                        mapManager.UpdateMap(_fallbackLatLon, mapManager.AbsoluteZoom);
                    }
                    Debug.Log($"[MapAvatarTracker] Loaded IP Fallback Location: {data.lat}, {data.lon}");

                    // Fetch Yelp places instantly so the map isn't empty while waiting for GPS!
                    if (YelpPlacesManager.Instance != null)
                    {
                        YelpPlacesManager.Instance.FetchNearbyPlaces(_fallbackLatLon);
                    }
                }
            }
        }
    }

    private bool _isFirstLocationSet = false;

    private Vector2d _lastValidGPS = Vector2d.zero;

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

        Vector2d currentLocation = _lastValidGPS;
        if (currentLocation == Vector2d.zero) currentLocation = _fallbackLatLon;

        // If the hardware GPS finally locks onto a satellite, it overrides the Wi-Fi fallback
        if (_locationProvider != null && _locationProvider.CurrentLocation.LatitudeLongitude != Vector2d.zero)
        {
            currentLocation = _locationProvider.CurrentLocation.LatitudeLongitude;
            _lastValidGPS = currentLocation;
            
            // Re-center the map if we just transitioned from the fallback to real GPS
            if (_useFallbackLocation)
            {
                try { mapManager.Initialize(currentLocation, mapManager.AbsoluteZoom); }
                catch { mapManager.UpdateMap(currentLocation, mapManager.AbsoluteZoom); }
                _useFallbackLocation = false;

                // Also refresh Yelp places for the accurate GPS location
                if (YelpPlacesManager.Instance != null)
                {
                    YelpPlacesManager.Instance.FetchNearbyPlaces(currentLocation);
                }
            }
        }

        // If neither GPS nor Fallback is ready, do nothing (wait)
        if (currentLocation == Vector2d.zero) return;

        // --- SAVE GPS POINTS PERIODICALLY ---
        if (Time.time - _lastSaveTime > 5f && currentLocation != _fallbackLatLon)
        {
            _lastSaveTime = Time.time;
            
            // ANTI-TELEPORT: If the GPS jumps by more than ~2 kilometers instantly (like Editor spawning at 0,0), wipe the trail!
            if (_savedGPSPoints.Count > 0 && Vector2d.Distance(currentLocation, _savedGPSPoints[_savedGPSPoints.Count - 1]) > 0.02)
            {
                _savedGPSPoints.Clear();
                PlayerPrefs.DeleteKey("SavedGPSTrail");
            }

            if (_savedGPSPoints.Count == 0 || Vector2d.Distance(currentLocation, _savedGPSPoints[_savedGPSPoints.Count - 1]) > 0.00005) // ~5 meters
            {
                _savedGPSPoints.Add(currentLocation);
                if (_savedGPSPoints.Count > 500) _savedGPSPoints.RemoveAt(0); // Keep last 500 to prevent RAM crashes
                
                string saveString = "";
                foreach(var pt in _savedGPSPoints) saveString += pt.x + "," + pt.y + "|";
                PlayerPrefs.SetString("SavedGPSTrail", saveString);
            }
        }

        // --- REDRAW SAVED TRAIL HISTORY EVERY FRAME TO HANDLE MAP SHIFTS ---
        if (_savedGPSPoints.Count > 0)
        {
            LineRenderer lr = GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = _savedGPSPoints.Count;
                for (int i = 0; i < _savedGPSPoints.Count; i++)
                {
                    Vector3 worldPt = mapManager.GeoToWorldPosition(_savedGPSPoints[i], true);
                    worldPt.y = 0.5f; // Keep it on the ground
                    lr.SetPosition(i, worldPt);
                }
            }
        }

        // --- DESTROY PESKY DEFAULT MAPBOX RED PINS ---
        // (Commented out because the user wants to keep the red pin and blue cone!)
        // GameObject mapboxPin1 = GameObject.Find("LocationProvider(Clone)");
        // if (mapboxPin1 != null) Destroy(mapboxPin1);
        // GameObject mapboxPin2 = GameObject.Find("LocationPrefab(Clone)");
        // if (mapboxPin2 != null) Destroy(mapboxPin2);

        // 2. Convert real-world GPS into Unity 3D World space
        Vector3 targetPosition = mapManager.GeoToWorldPosition(currentLocation, true);
        
        // --- CRITICAL FIX: PREVENT FLOATING POINT CRASH & MASSIVE TILE SPAWN LAG ---
        // If the GPS jumped across the world, targetPosition will be 1,000,000+ units away.
        // This causes Mapbox to try to load 10,000 tiles and crashes the game!
        if (targetPosition.magnitude > 500f)
        {
            // Only recenter if we haven't already recentered to this exact GPS spot!
            // Mapbox takes a few seconds to download, so we can't spam UpdateMap!
            // 0.001 degrees of lat/lon is roughly 100 meters in the real world.
            double distanceInDegrees = Vector2d.Distance(currentLocation, _lastRecenteredGPS);
            
            if (_lastRecenteredGPS == Vector2d.zero || distanceInDegrees > 0.001)
            {
                _lastRecenteredGPS = currentLocation;
                Debug.Log("[MapAvatarTracker] GPS jumped too far! Re-centering map to prevent lag!");
                try { mapManager.Initialize(currentLocation, mapManager.AbsoluteZoom); }
                catch { mapManager.UpdateMap(currentLocation, mapManager.AbsoluteZoom); }
                
                // Recalculate target position relative to the NEW perfectly centered map!
                targetPosition = mapManager.GeoToWorldPosition(currentLocation, true);
                
                // Force an instant snap so it doesn't drag a massive trail
                StartCoroutine(SnapAndClear(targetPosition));
            }
        }

        // Keep the avatar at ground level (Y = 0) so it doesn't fly or sink
        targetPosition.y = 0f;

        // 3. Smoothly move the Avatar to the new location
        if (!_isFirstLocationSet)
        {
            _isFirstLocationSet = true;
            StartCoroutine(SnapAndClear(targetPosition));
        }
        else if (Vector3.Distance(transform.position, targetPosition) > 50f)
        {
            // If GPS suddenly jumps (e.g. from fallback to real GPS), snap instantly to prevent giant trails!
            StartCoroutine(SnapAndClear(targetPosition));
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
        }

        // 4. Sync Avatar Rotation to Real-World Compass Heading!
        float targetAngle = Input.compass.trueHeading;
        float currentAngle = transform.eulerAngles.y;
        float smoothAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 5f);
        
        // Rotate the actual 3D Avatar child object to face the compass direction!
        Transform custom3DAvatar = transform.Find("Custom 3D Avatar");
        if (custom3DAvatar != null)
        {
            custom3DAvatar.eulerAngles = new Vector3(0, smoothAngle, 0);
        }
    }

    private IEnumerator SnapAndClear(Vector3 targetPosition)
    {
        TrailRenderer tr = GetComponent<TrailRenderer>();
        if (tr != null) tr.emitting = false;
        
        transform.position = targetPosition;
        
        yield return null; // Wait one physical frame
        yield return new WaitForEndOfFrame(); // Wait for rendering cycle
        
        if (tr != null)
        {
            tr.Clear();
            tr.emitting = true;
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
        // Prevent panning when touching UI (like the Shop menu!)
        if (EventSystem.current != null)
        {
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
            if (Input.touchCount == 0 && EventSystem.current.IsPointerOverGameObject()) return;
        }

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
