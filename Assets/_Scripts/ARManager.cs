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
    
    private GameObject dynamicBgQuad;
    
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
            originalAvatarParent = customAvatar.transform.parent;
            originalAvatarScale = customAvatar.transform.localScale;
            originalAvatarLocalPos = customAvatar.transform.localPosition;

            customAvatar.SetActive(true);
            customAvatar.transform.SetParent(mainCamera.transform, false);
            customAvatar.transform.localPosition = avatarARPosition;
            // Removed: customAvatar.transform.localScale = Vector3.one; so the prefab keeps its original size!
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
        if (fallbackBackground != null)
        {
            fallbackBackground.SetActive(true);
            
            // Put the fallback background in a special camera-space canvas so it renders behind the 3D avatar!
            Canvas bgCanvas = fallbackBackground.GetComponentInParent<Canvas>();
            if (bgCanvas != null)
            {
                if (bgCanvas.renderMode != RenderMode.ScreenSpaceCamera)
                {
                    // Create a new canvas just for the background so animations keep playing normally!
                    GameObject newCanvasObj = new GameObject("FallbackBackgroundCanvas");
                    Canvas newCanvas = newCanvasObj.AddComponent<Canvas>();
                    newCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                    newCanvas.worldCamera = mainCamera;
                    newCanvas.planeDistance = 20f; // Far behind the avatar (avatar is at 5f)
                    newCanvas.sortingOrder = -100; // Force it to the back
                    
                    UnityEngine.UI.CanvasScaler scaler = newCanvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    scaler.matchWidthOrHeight = 0.5f;

                    fallbackBackground.transform.SetParent(newCanvasObj.transform, false);
                    
                    // Reset its rect transform to stretch fully
                    RectTransform rt = fallbackBackground.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.offsetMin = Vector2.zero;
                        rt.offsetMax = Vector2.zero;
                    }
                }
                else
                {
                    bgCanvas.worldCamera = mainCamera;
                    bgCanvas.planeDistance = 20f;
                }
            }
        }
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
        
        if (fallbackBackground != null) fallbackBackground.SetActive(false);
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
        }

        if (mapGPSNode != null) mainCamera.transform.SetParent(mapGPSNode);
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = originalOrthoSize;
        mainCamera.transform.localPosition = originalCameraPos;
        mainCamera.transform.localRotation = originalCameraRot;

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
