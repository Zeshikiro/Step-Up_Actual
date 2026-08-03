using UnityEngine;

public class AvatarAnimatorSync : MonoBehaviour
{
    [Header("References")]
    public Animator avatarAnimator;
    public StepManager stepManager;

    [Header("Animator Parameters")]
    [Tooltip("The exact name of your float parameter for Speed (e.g. 'Speed')")]
    public string speedParameter = "Speed";
    [Tooltip("The exact name of your boolean parameter for Walking (e.g. 'IsWalking')")]
    public string isWalkingParameter = "IsWalking";

    private int _lastStepCount;
    private float _lastStepTime;

    // GPS Movement Animation Fallback
    private Vector3 _lastPosition;
    private float _lastMoveTime;

    // Track the MapAvatarTracker parent for position-based movement detection
    private Transform _trackerTransform;

    // Parameter hash caching for safety
    private int _speedHash;
    private int _isWalkingHash;

    // Cached parameter existence flags (avoids scanning every frame)
    private bool _hasSpeedParam = false;
    private bool _hasIsWalkingParam = false;
    private bool _paramsCached = false;

    void Start()
    {
        Debug.Log("[AvatarAnimatorSync] Script is ALIVE and running on: " + gameObject.name);
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();
        if (stepManager != null) _lastStepCount = stepManager.currentDailySteps;

        // Cache parameter hashes
        _speedHash = Animator.StringToHash(speedParameter);
        _isWalkingHash = Animator.StringToHash(isWalkingParameter);

        // Find the MapAvatarTracker (the object that actually moves via GPS)
        _trackerTransform = FindTrackerParent();
        if (_trackerTransform != null)
        {
            _lastPosition = _trackerTransform.position;
            Debug.Log("[AvatarAnimatorSync] Found GPS tracker parent: " + _trackerTransform.name);
        }
        else
        {
            _lastPosition = transform.position;
            Debug.LogWarning("[AvatarAnimatorSync] Could not find MapAvatarTracker parent, using own transform.");
        }
    }

    private Transform FindTrackerParent()
    {
        // Walk up the hierarchy to find the MapAvatarTracker component
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.GetComponent<MapAvatarTracker>() != null)
                return current;
            current = current.parent;
        }
        // Also check siblings / scene root
        MapAvatarTracker tracker = FindFirstObjectByType<MapAvatarTracker>();
        return tracker != null ? tracker.transform : null;
    }

    void Update()
    {
        if (stepManager == null)
        {
            stepManager = FindFirstObjectByType<StepManager>();
            if (stepManager == null) return;
        }

        // AR avatars might be completely separate GameObjects in the scene hierarchy!
        // FindObjectsByType guarantees we find the AR avatar's Animator even if it's not a child of this script.
        Animator[] allAnimators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        if (allAnimators.Length == 0) return;

        // 1. Check for Pedometer Steps
        if (stepManager.currentDailySteps > _lastStepCount)
        {
            _lastStepCount = stepManager.currentDailySteps;
            _lastStepTime = Time.time;
            Debug.Log($"[AvatarAnimatorSync] Step detected! Count: {_lastStepCount}");
        }

        // 2. Check for Physical GPS Sliding on the TRACKER transform (not this object!)
        Transform posSource = _trackerTransform != null ? _trackerTransform : transform;
        float distanceMoved = Vector3.Distance(posSource.position, _lastPosition);
        if (distanceMoved > 0.02f) // Moved more than 2cm this frame
        {
            _lastMoveTime = Time.time;
        }
        _lastPosition = posSource.position;

        // If we stepped recently OR moved physically recently, we are walking!
        bool isStepping = (Time.time - _lastStepTime) < 3.0f;  // Extended to 3s for smoother animation
        bool isSliding = (Time.time - _lastMoveTime) < 1.5f;

        bool shouldWalk = isStepping || isSliding;

        foreach (var anim in allAnimators)
        {
            if (anim == null || !anim.isActiveAndEnabled) continue;
            if (anim.runtimeAnimatorController == null) continue;
            
            // STRICT FILTER: Only sync parameters to the actual 3D Avatar!
            // This prevents us from spamming UI animators with "IsWalking" errors
            if (!anim.runtimeAnimatorController.name.Contains("AvatarBrain") && !anim.name.Contains("Avatar")) continue;

            // Re-cache if we haven't found the parameters yet! (Crucial for dynamically loaded avatars)
            if (!_hasIsWalkingParam || !_hasSpeedParam)
            {
                CacheParameterExistence(anim);
            }

            if (shouldWalk)
            {
                float targetSpeed = (stepManager.CurrentSpeedMPS > 2.5f) ? 2.0f : 1.0f;
                if (_hasIsWalkingParam) anim.SetBool(_isWalkingHash, true);
                if (_hasSpeedParam) anim.SetFloat(_speedHash, targetSpeed);
            }
            else
            {
                if (_hasIsWalkingParam) anim.SetBool(_isWalkingHash, false);
                if (_hasSpeedParam) anim.SetFloat(_speedHash, 0f);
            }
        }
    }

    private void CacheParameterExistence(Animator anim)
    {
        // Add fallback parameter names just in case the user named them differently
        int fallbackWalkingHash1 = Animator.StringToHash("Walk");
        int fallbackWalkingHash2 = Animator.StringToHash("Walking");
        int fallbackWalkingHash3 = Animator.StringToHash("isWalking");
        int fallbackSpeedHash1 = Animator.StringToHash("Blend");

        foreach (var p in anim.parameters)
        {
            if ((p.nameHash == _speedHash || p.nameHash == fallbackSpeedHash1) && p.type == AnimatorControllerParameterType.Float)
            {
                _hasSpeedParam = true;
                _speedHash = p.nameHash; // Update to the actual working hash
            }
            if ((p.nameHash == _isWalkingHash || p.nameHash == fallbackWalkingHash1 || p.nameHash == fallbackWalkingHash2 || p.nameHash == fallbackWalkingHash3) && p.type == AnimatorControllerParameterType.Bool)
            {
                _hasIsWalkingParam = true;
                _isWalkingHash = p.nameHash; // Update to the actual working hash
            }
        }
        
        Debug.Log($"[AvatarAnimatorSync] Scanning '{anim.name}': HasSpeed={_hasSpeedParam}, HasIsWalking={_hasIsWalkingParam}");
    }
}
