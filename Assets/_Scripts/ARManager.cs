using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.XR.ARFoundation;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class ARManager : MonoBehaviour
{
    [Header("--- Core References ---")]
    public GameObject mapRoot;
    public Camera mainCamera;
    
    [Header("--- 2D Map vs 3D AR Objects ---")]
    public Transform mapGPSNode; 
    public GameObject mapPin; 
    public GameObject customAvatar; 
    
    [Header("--- UI Elements to Toggle ---")]
    public GameObject rotateAvatarButton;
    public GameObject compassUI;
    public GameObject recenterButton;
    
    [Header("--- Icon Swapping ---")]
    public Image toggleButtonImage;
    public Sprite icon3D;
    public Sprite icon2D;
    
    [Header("--- AR Settings ---")]
    public bool forceFallbackMode = false; // Bypass ARCore entirely on phones without AR!
    public Vector3 avatarARPosition = new Vector3(0, -1.5f, 5f); 
    
    [Header("--- Fallback UI ---")]
    public GameObject fallbackBackground; 

    private GameObject arSessionObj;
    private ARCameraManager arCameraManager;
    private ARCameraBackground arCameraBackground;

    private bool isARMode = false;
    public bool IsARMode { get { return isARMode; } }
    private bool isFacingUser = true;

    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private float originalOrthoSize;

    private Transform originalAvatarParent;
    private Vector3 originalAvatarScale;
    private Vector3 originalAvatarLocalPos;
    private bool _hasCachedOriginalState = false;
    
    private GameObject dynamicBgQuad;
    
    private CameraClearFlags originalClearFlags;
    private Color originalBgColor;
    
    private bool isPermissionRequested = false;

    void Start()
    {
        if (fallbackBackground != null) fallbackBackground.SetActive(false);
        if (rotateAvatarButton != null) rotateAvatarButton.SetActive(false);

        if (mainCamera != null && mapGPSNode != null)
        {
            originalCameraPos = mainCamera.transform.localPosition;
            originalCameraRot = mainCamera.transform.localRotation;
            originalOrthoSize = mainCamera.orthographicSize;
            originalClearFlags = mainCamera.clearFlags;
            originalBgColor = mainCamera.backgroundColor;
        }

        if (customAvatar != null) customAvatar.SetActive(false);
        if (mapPin != null) mapPin.SetActive(true);
    }

    private bool isTransitioning = false;
    private Coroutine arCoroutine;

    public void ToggleARMode()
    {
        if (isTransitioning) return; // Ignore spam clicks!
        isTransitioning = true;
        
        isARMode = !isARMode;

        if (isARMode)
        {
            if (arCoroutine != null) StopCoroutine(arCoroutine);
            arCoroutine = StartCoroutine(StartARCoroutine());
        }
        else
        {
            StopAR();
        }
    }

    private GameObject arCameraParent;

    private IEnumerator StartARCoroutine()
    {
        if (fallbackBackground != null) fallbackBackground.SetActive(true);

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            if (!isPermissionRequested)
            {
                Permission.RequestUserPermission(Permission.Camera);
                isPermissionRequested = true;
            }
            float timeout = 10f;
            while (!Permission.HasUserAuthorizedPermission(Permission.Camera) && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
        }
#endif

        if (mapRoot != null) mapRoot.SetActive(false);
        if (mapPin != null) mapPin.SetActive(false);
        if (compassUI != null) compassUI.SetActive(false);
        if (recenterButton != null) recenterButton.SetActive(false);

        if (arCameraParent == null)
        {
            arCameraParent = new GameObject("ARCameraParent");
            arCameraParent.transform.position = new Vector3(0, -10000f, 0); // Hide completely underground from map
        }

        mainCamera.transform.SetParent(arCameraParent.transform, false);
        mainCamera.orthographic = false;
        mainCamera.fieldOfView = 60f;
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;

        if (customAvatar != null)
        {
            if (!_hasCachedOriginalState)
            {
                originalAvatarParent = customAvatar.transform.parent;
                originalAvatarScale = customAvatar.transform.localScale;
                originalAvatarLocalPos = customAvatar.transform.localPosition;
                _hasCachedOriginalState = true;
            }

            customAvatar.SetActive(true);
            
            // Turn off the trail renderer so it doesn't streak across the screen in AR/UI!
            TrailRenderer tr = customAvatar.GetComponent<TrailRenderer>();
            if (tr != null) tr.emitting = false;

            customAvatar.transform.SetParent(mainCamera.transform, false);
            customAvatar.transform.localPosition = avatarARPosition;
            // SAFETY FALLBACK: Guarantee it maintains its exact original scale in case it shrunk
            customAvatar.transform.localScale = originalAvatarScale;
            customAvatar.transform.localRotation = Quaternion.identity;
            
            // ENSURE it is completely active and on the Default layer so the camera sees it
            customAvatar.SetActive(true);
            customAvatar.layer = 0; 
        }
        UpdateAvatarRotation();

        bool cameraReady = false;
#if PLATFORM_ANDROID
        if (Permission.HasUserAuthorizedPermission(Permission.Camera)) cameraReady = true;
#else
        cameraReady = true; 
#endif

        if (cameraReady)
        {
            if (forceFallbackMode)
            {
                Debug.Log("[ARManager] Force Fallback Mode is enabled. Bypassing ARCore entirely!");
                ActivateFallbackBackground();
            }
            else
            {
                if (ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                // ADDED TIMEOUT: Don't hang forever if ARCore is broken on this phone
                float arTimeout = 5f;
                IEnumerator checkRoutine = ARSession.CheckAvailability();
                while (checkRoutine.MoveNext())
                {
                    arTimeout -= Time.deltaTime;
                    if (arTimeout <= 0f) break;
                    yield return checkRoutine.Current;
                }
            }

            if (!isARMode) yield break; // CRITICAL: Stop if user clicked 2D while we were checking availability!

            if (ARSession.state == ARSessionState.Unsupported || ARSession.state == ARSessionState.None || ARSession.state == ARSessionState.CheckingAvailability)
            {
                Debug.LogWarning("[ARManager] XRGOOGLE Unsupported or Timed out. Using fallback.");
                ActivateFallbackBackground();
            }
            else
            {
                if (fallbackBackground != null) fallbackBackground.SetActive(false);
                if (dynamicBgQuad != null) dynamicBgQuad.SetActive(false);

                if (arSessionObj == null)
                {
                    arSessionObj = new GameObject("AR Session");
                    arSessionObj.AddComponent<ARSession>();
                }
                arSessionObj.SetActive(true);
                
                if (arCameraManager == null)
                {
                    arCameraManager = mainCamera.gameObject.AddComponent<ARCameraManager>();
                    arCameraBackground = mainCamera.gameObject.AddComponent<ARCameraBackground>();
                }
                    arCameraManager.enabled = true;
                    arCameraBackground.enabled = true;
                }
            }
        }
        else
        {
            ActivateFallbackBackground();
        }

        if (rotateAvatarButton != null) rotateAvatarButton.SetActive(true);
        if (toggleButtonImage != null && icon2D != null) toggleButtonImage.sprite = icon2D;
        
        isTransitioning = false;
    }

    private void ActivateFallbackBackground()
    {
        Debug.Log("[ARManager] ActivateFallbackBackground() called.");
        
        // 1. Force the camera to clear to a Solid Color — this is the GUARANTEED fallback on ALL devices.
        //    No shader lookup, no quad, no material. Just a camera clear color. Works everywhere.
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.12f, 0.14f, 0.22f, 1f); // Dark navy
            Debug.Log("[ARManager] Camera clearFlags set to SolidColor with dark navy background.");
        }

        // 2. DISABLE the user's inspector-assigned fallback!
        // The user assigned a 3D Plane/Quad which is physically slicing through the avatar 
        // or engulfing the camera depending on rotation/scale.
        // By disabling it, we rely purely on the SolidColor background above, which is flawless.
        if (fallbackBackground != null)
        {
            fallbackBackground.SetActive(false);
            Debug.Log("[ARManager] User's 3D fallbackBackground disabled to prevent avatar slicing.");
        }
        
        Debug.Log("[ARManager] Fallback mode fully active (SolidColor only).");
    }

    private void StopAR()
    {
        if (arCoroutine != null)
        {
            StopCoroutine(arCoroutine);
            arCoroutine = null;
        }

        if (arSessionObj != null)
        {
            arSessionObj.SetActive(false);
        }
        if (arCameraManager != null)
        {
            arCameraBackground.enabled = false;
            arCameraManager.enabled = false;
        }
        
        if (fallbackBackground != null)
        {
            fallbackBackground.SetActive(false);
            // Also disable the parent Canvas so it doesn't overlay the 2D map
            Canvas parentCanvas = fallbackBackground.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null) parentCanvas.gameObject.SetActive(false);
        }
        if (dynamicBgQuad != null) dynamicBgQuad.SetActive(false);

        if (mapRoot != null) mapRoot.SetActive(true);
        if (mapPin != null) mapPin.SetActive(true);
        if (compassUI != null) compassUI.SetActive(true);
        if (recenterButton != null) recenterButton.SetActive(true);

        if (customAvatar != null)
        {
            customAvatar.transform.SetParent(originalAvatarParent, false);
            customAvatar.transform.localScale = originalAvatarScale;
            customAvatar.transform.localPosition = originalAvatarLocalPos;
            customAvatar.SetActive(false);
            customAvatar.transform.rotation = Quaternion.identity;
            
            // Turn trail renderer back on for map mode!
            TrailRenderer tr = customAvatar.GetComponent<TrailRenderer>();
            if (tr != null) tr.emitting = true;
        }

        if (mapGPSNode != null) mainCamera.transform.SetParent(mapGPSNode);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = originalOrthoSize;
        mainCamera.transform.localPosition = originalCameraPos;
        mainCamera.transform.localRotation = originalCameraRot;
        
        mainCamera.clearFlags = originalClearFlags;
        mainCamera.backgroundColor = originalBgColor;

        if (rotateAvatarButton != null) rotateAvatarButton.SetActive(false);
        if (toggleButtonImage != null && icon3D != null) toggleButtonImage.sprite = icon3D;
        
        isTransitioning = false;
    }

    public void ToggleAvatarFacing()
    {
        isFacingUser = !isFacingUser;
        if (isARMode) UpdateAvatarRotation();
    }

    private void UpdateAvatarRotation()
    {
        if (customAvatar == null) return;
        customAvatar.transform.localRotation = isFacingUser ? Quaternion.Euler(0, 180f, 0) : Quaternion.Euler(0, 0, 0);
    }
}
