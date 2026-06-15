using UnityEngine;
using UnityEngine.UI;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class ARManager : MonoBehaviour
{
    [Header("--- Core References ---")]
    public GameObject mapRoot; // Drag CitySimulatorMap here
    public Camera mainCamera; // Drag Main Camera here
    
    [Header("--- 2D Map vs 3D AR Objects ---")]
    [Tooltip("The parent object that moves on the Map (PlayerAvatar)")]
    public Transform mapGPSNode; 
    [Tooltip("The Cylinder or Map Pin icon")]
    public GameObject mapPin; 
    [Tooltip("The actual 3D Customized Avatar (We will add this soon)")]
    public GameObject customAvatar; 
    
    [Header("--- UI Elements to Toggle ---")]
    [Tooltip("The button used to rotate the avatar. It will be hidden in 2D Map Mode.")]
    public GameObject rotateAvatarButton;
    [Tooltip("The Compass UI to hide in 3D AR Mode")]
    public GameObject compassUI;
    [Tooltip("The Recenter Map button to hide in 3D AR Mode")]
    public GameObject recenterButton;
    
    [Header("--- Icon Swapping ---")]
    public Image toggleButtonImage; // The Image component on your 3D/2D toggle button
    public Sprite icon3D; // The default 3D cube icon
    public Sprite icon2D; // The new 2D icon we just generated
    
    [Header("--- AR Settings ---")]
    [Tooltip("Where the avatar stands relative to the camera in AR")]
    public Vector3 avatarARPosition = new Vector3(0, -1.5f, 5f); 
    
    [Header("--- UI Elements ---")]
    [Tooltip("The RawImage component at the back of your Canvas")]
    public RawImage cameraBackground; 
    [Tooltip("Attach an AspectRatioFitter to the RawImage and drag it here")]
    public AspectRatioFitter backgroundFitter; 
    [Tooltip("A static Image or UI Panel to show if the Camera fails or permissions are denied")]
    public GameObject fallbackBackground; 

    private WebCamTexture backCamera;
    private bool isARMode = false;
    public bool IsARMode { get { return isARMode; } }
    private bool isFacingUser = true;
    private bool isPermissionRequested = false;

    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private float originalOrthoSize;

    // Cache avatar settings
    private Transform originalAvatarParent;
    private Vector3 originalAvatarScale;
    private Vector3 originalAvatarLocalPos;

    void Start()
    {
        if (cameraBackground != null)
        {
            cameraBackground.gameObject.SetActive(false);
        }
        
        if (fallbackBackground != null)
        {
            fallbackBackground.SetActive(false);
        }
        
        if (rotateAvatarButton != null)
        {
            rotateAvatarButton.SetActive(false); // Hide the rotate button at start (2D Mode)
        }

        // Save original camera offset relative to the GPS node
        if (mainCamera != null && mapGPSNode != null)
        {
            originalCameraPos = mainCamera.transform.localPosition;
            originalCameraRot = mainCamera.transform.localRotation;
            originalOrthoSize = mainCamera.orthographicSize;
        }

        if (customAvatar != null) customAvatar.SetActive(false); // Hide 3D avatar on start
        if (mapPin != null) mapPin.SetActive(true); // Show Map Pin on start
    }

    public void ToggleARMode()
    {
        isARMode = !isARMode;

        if (isARMode)
        {
            StartAR();
        }
        else
        {
            StopAR();
        }
    }

    private void StartAR()
    {
        // 1. Request Camera Permission on Android
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera) && !isPermissionRequested)
        {
            Permission.RequestUserPermission(Permission.Camera);
            isPermissionRequested = true;
        }
#endif

        // 2. Hide the 3D Mapbox Map, Map Pin, Compass, & Recenter Button
        if (mapRoot != null) mapRoot.SetActive(false);
        if (mapPin != null) mapPin.SetActive(false);
        if (compassUI != null) compassUI.SetActive(false);
        if (recenterButton != null) recenterButton.SetActive(false);

        // 3. Unparent the Camera from the Map GPS Node
        mainCamera.transform.SetParent(null);
        mainCamera.orthographic = false;
        mainCamera.fieldOfView = 60f;
        mainCamera.transform.position = Vector3.zero;
        mainCamera.transform.rotation = Quaternion.identity;

        // 4. Show the Custom 3D Avatar and put it in front of the Camera
        if (customAvatar != null)
        {
            // Cache its original state before moving it
            originalAvatarParent = customAvatar.transform.parent;
            originalAvatarScale = customAvatar.transform.localScale;
            originalAvatarLocalPos = customAvatar.transform.localPosition;

            customAvatar.SetActive(true);
            customAvatar.transform.SetParent(mainCamera.transform, false);
            customAvatar.transform.localPosition = avatarARPosition;
            // Force it to a normal scale in front of the camera (assuming 1,1,1 is normal)
            customAvatar.transform.localScale = Vector3.one; 
        }
        
        UpdateAvatarRotation();

        // 5. Setup the Live Camera Feed or Fallback Background
        bool cameraReady = false;

#if PLATFORM_ANDROID
        if (Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            cameraReady = true;
        }
#else
        cameraReady = true; // Non-Android platforms don't use this specific permission API
#endif

        if (cameraReady)
        {
            if (fallbackBackground != null) fallbackBackground.SetActive(false);

            if (backCamera == null)
            {
                WebCamDevice[] devices = WebCamTexture.devices;
                for (int i = 0; i < devices.Length; i++)
                {
                    if (!devices[i].isFrontFacing)
                    {
                        backCamera = new WebCamTexture(devices[i].name, Screen.width, Screen.height);
                        break;
                    }
                }
            }

            if (backCamera != null)
            {
                cameraBackground.gameObject.SetActive(true);
                cameraBackground.texture = backCamera;
                backCamera.Play();
            }
            else
            {
                // No back camera found on this device!
                if (fallbackBackground != null) fallbackBackground.SetActive(true);
                if (cameraBackground != null) cameraBackground.gameObject.SetActive(false);
            }
        }
        else
        {
            // Permission was denied or is still pending!
            if (fallbackBackground != null) fallbackBackground.SetActive(true);
            if (cameraBackground != null) cameraBackground.gameObject.SetActive(false);
        }

        // 6. Show the Rotate Avatar Button
        if (rotateAvatarButton != null) rotateAvatarButton.SetActive(true);

        // 7. Swap the toggle button icon to '2D'
        if (toggleButtonImage != null && icon2D != null) toggleButtonImage.sprite = icon2D;
    }

    private void StopAR()
    {
        // 1. Stop Camera Feed
        if (backCamera != null && backCamera.isPlaying)
        {
            backCamera.Stop();
        }
        if (cameraBackground != null) cameraBackground.gameObject.SetActive(false);
        if (fallbackBackground != null) fallbackBackground.SetActive(false);

        // 2. Show the 3D Mapbox Map, Map Pin, Compass, & Recenter Button
        if (mapRoot != null) mapRoot.SetActive(true);
        if (mapPin != null) mapPin.SetActive(true);
        if (compassUI != null) compassUI.SetActive(true);
        if (recenterButton != null) recenterButton.SetActive(true);

        // 3. Hide and Detach the Custom 3D Avatar
        if (customAvatar != null)
        {
            customAvatar.transform.SetParent(originalAvatarParent, false);
            customAvatar.transform.localScale = originalAvatarScale;
            customAvatar.transform.localPosition = originalAvatarLocalPos;
            customAvatar.SetActive(false);
            customAvatar.transform.rotation = Quaternion.identity;
        }

        // 4. Put the Camera back on the GPS Tracker (PlayerAvatar)
        if (mapGPSNode != null)
        {
            mainCamera.transform.SetParent(mapGPSNode);
        }
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = originalOrthoSize;
        mainCamera.transform.localPosition = originalCameraPos;
        mainCamera.transform.localRotation = originalCameraRot;

        // 5. Hide the Rotate Avatar Button
        if (rotateAvatarButton != null) rotateAvatarButton.SetActive(false);

        // 6. Swap the toggle button icon back to '3D'
        if (toggleButtonImage != null && icon3D != null) toggleButtonImage.sprite = icon3D;
    }

    public void ToggleAvatarFacing()
    {
        isFacingUser = !isFacingUser;
        if (isARMode)
        {
            UpdateAvatarRotation();
        }
    }

    private void UpdateAvatarRotation()
    {
        if (customAvatar == null) return;

        if (isFacingUser)
        {
            customAvatar.transform.localRotation = Quaternion.Euler(0, 180f, 0);
        }
        else
        {
            customAvatar.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void Update()
    {
        // Fix the camera feed rotation/stretching which flips sideways natively on phones
        if (isARMode && backCamera != null && backCamera.isPlaying && cameraBackground != null)
        {
            float ratio = (float)backCamera.width / (float)backCamera.height;
            if (backgroundFitter != null)
            {
                backgroundFitter.aspectRatio = ratio;
            }

            float scaleY = backCamera.videoVerticallyMirrored ? -1f : 1f;
            cameraBackground.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

            int orient = -backCamera.videoRotationAngle;
            cameraBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        }
    }
}
