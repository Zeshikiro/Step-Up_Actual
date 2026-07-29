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
    public bool forceFallbackMode = true; // Bypass ARCore entirely on phones without AR!
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
        
        // 1. Force the camera to clear to a Solid Color so it's never a black void
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0.15f, 0.15f, 0.2f, 1f); // Dark blue-grey
            Debug.Log("[ARManager] Camera clearFlags set to SolidColor.");
        }

        // 2. Try the inspector-assigned fallback first
        if (fallbackBackground != null)
        {
            fallbackBackground.SetActive(true);
            
            // Reparent to camera so it follows the view
            fallbackBackground.transform.SetParent(mainCamera.transform, false);
            fallbackBackground.transform.localPosition = new Vector3(0, 0, 10f);
            fallbackBackground.transform.localRotation = Quaternion.identity;
            fallbackBackground.transform.localScale = new Vector3(50f, 50f, 1f);
            
            // Force a renderer material so it's NEVER invisible
            Renderer rend = fallbackBackground.GetComponent<Renderer>();
            if (rend != null)
            {
                if (rend.sharedMaterial == null)
                {
                    Material mat = CreateFallbackMaterial();
                    if (mat != null) rend.material = mat;
                }
                Debug.Log("[ARManager] Fallback BG renderer: enabled=" + rend.enabled + 
                          ", material=" + (rend.sharedMaterial != null ? rend.sharedMaterial.name : "NULL") +
                          ", shader=" + (rend.sharedMaterial != null ? rend.sharedMaterial.shader.name : "NULL"));
            }
            else
            {
                Debug.LogWarning("[ARManager] fallbackBackground has NO Renderer component! Type: " + fallbackBackground.GetType().Name);
            }
            
            Debug.Log("[ARManager] Fallback background activated: " + fallbackBackground.name);
        }
        else
        {
            Debug.LogWarning("[ARManager] fallbackBackground is NULL in Inspector!");
        }
        
        // 3. ALWAYS create a guaranteed backup quad in case fallbackBackground is null or invisible
        if (dynamicBgQuad == null)
        {
            dynamicBgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dynamicBgQuad.name = "DynamicFallbackBG";
            
            // Remove the collider so it doesn't block raycasts
            Collider col = dynamicBgQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Attach to camera at a REASONABLE distance (not farClipPlane which could be 1000+)
            float bgDistance = 10f;
            dynamicBgQuad.transform.SetParent(mainCamera.transform, false);
            dynamicBgQuad.transform.localPosition = new Vector3(0, 0, bgDistance);
            dynamicBgQuad.transform.localRotation = Quaternion.identity;
            
            // Scale to fill the entire camera frustum at that distance
            float frustumHeight = 2.0f * bgDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * mainCamera.aspect;
            dynamicBgQuad.transform.localScale = new Vector3(frustumWidth * 1.5f, frustumHeight * 1.5f, 1f);
            
            // Create a visible material (URP-safe)
            Renderer quadRend = dynamicBgQuad.GetComponent<Renderer>();
            Material bgMat = CreateFallbackMaterial();
            if (bgMat != null)
            {
                quadRend.material = bgMat;
            }
            
            Debug.Log("[ARManager] Dynamic BG Quad created. Scale: " + dynamicBgQuad.transform.localScale + 
                       ", Distance: " + bgDistance + ", Material: " + (quadRend.sharedMaterial != null ? quadRend.sharedMaterial.shader.name : "NONE"));
        }
        dynamicBgQuad.SetActive(true);
        
        Debug.Log("[ARManager] Fallback mode fully active.");
    }
    
    /// <summary>
    /// Creates a fallback material that works on BOTH Built-in RP and URP.
    /// Shader.Find("Unlit/Color") returns NULL on URP builds!
    /// </summary>
    private Material CreateFallbackMaterial()
    {
        Color bgColor = new Color(0.12f, 0.14f, 0.22f, 1f); // Dark navy
        
        // Try URP unlit shader first (most likely on this project)
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("UI/Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = bgColor;
            // URP uses _BaseColor instead of _Color
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", bgColor);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", bgColor);
            Debug.Log("[ARManager] Created fallback material with shader: " + shader.name);
            return mat;
        }
        
        Debug.LogError("[ARManager] CRITICAL: Could not find ANY shader for fallback material!");
        return null;
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
