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

    private Coroutine arCoroutine;

    public void ToggleARMode()
    {
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
            customAvatar.transform.localScale = Vector3.one; 
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
    }

    private void ActivateFallbackBackground()
    {
        if (fallbackBackground != null)
        {
            // 1. Completely disable the UI Image so it doesn't block the 3D Avatar!
            fallbackBackground.SetActive(false); 

            // 2. Spawn a perfectly formatted 3D Quad behind the Avatar to hold the image
            Image img = fallbackBackground.GetComponent<Image>();
            if (img != null && img.sprite != null)
            {
                if (dynamicBgQuad == null)
                {
                    dynamicBgQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    Destroy(dynamicBgQuad.GetComponent<Collider>());
                    dynamicBgQuad.transform.SetParent(mainCamera.transform, false);
                    
                    // Put it 20 meters away so it's far behind the avatar (avatar is at Z=5)
                    dynamicBgQuad.transform.localPosition = new Vector3(0, 0, 20f); 
                    
                    // Stretch to perfectly fill the camera's FOV at 20 meters
                    float h = Mathf.Tan(mainCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * 20f * 2f;
                    float w = h * mainCamera.aspect;
                    dynamicBgQuad.transform.localScale = new Vector3(w, h, 1f);

                    Material mat = new Material(Shader.Find("Unlit/Texture"));
                    mat.mainTexture = img.sprite.texture;
                    dynamicBgQuad.GetComponent<MeshRenderer>().material = mat;
                }
                dynamicBgQuad.SetActive(true);
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
            Destroy(arSessionObj);
            arSessionObj = null;
        }
        if (arCameraManager != null)
        {
            Destroy(arCameraBackground);
            Destroy(arCameraManager);
            arCameraBackground = null;
            arCameraManager = null;
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
