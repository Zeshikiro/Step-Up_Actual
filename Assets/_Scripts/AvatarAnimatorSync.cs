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

    void Start()
    {
        Debug.Log("[AvatarAnimatorSync] Script is ALIVE and running on: " + gameObject.name);
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();
        if (stepManager != null) _lastStepCount = stepManager.currentDailySteps;

        // Cache parameter hashes
        _speedHash = Animator.StringToHash(speedParameter);
        _isWalkingHash = Animator.StringToHash(isWalkingParameter);

        // Find the MapAvatarTracker (the object that actually moves via GPS)
        // AvatarAnimatorSync sits on avatarContainer, which is a CHILD of the MapAvatarTracker.
        // We need to track the parent's world position to detect GPS sliding!
        _trackerTransform = FindTrackerParent();
        if (_trackerTransform != null)
        {
            _lastPosition = _trackerTransform.position;
            Debug.Log("[AvatarAnimatorSync] Found GPS tracker parent: " + _trackerTransform.name);
        }
        else
        {
            // Fallback: use our own position
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

        // Grab ALL active animators every frame (AvatarLoader swaps meshes dynamically)
        Animator[] allAnimators = GetComponentsInChildren<Animator>(false);
        if (allAnimators.Length == 0) return;

        // 1. Check for Pedometer Steps
        if (stepManager.currentDailySteps > _lastStepCount)
        {
            _lastStepCount = stepManager.currentDailySteps;
            _lastStepTime = Time.time;
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
        bool isStepping = (Time.time - _lastStepTime) < 2.0f;
        bool isSliding = (Time.time - _lastMoveTime) < 1.0f;

        bool shouldWalk = isStepping || isSliding;

        foreach (var anim in allAnimators)
        {
            if (anim == null || !anim.isActiveAndEnabled) continue;
            if (anim.runtimeAnimatorController == null) continue;

            if (shouldWalk)
            {
                float targetSpeed = (stepManager.CurrentSpeedMPS > 2.5f) ? 2.0f : 1.0f;
                SafeSetBool(anim, isWalkingParameter, _isWalkingHash, true);
                SafeSetFloat(anim, speedParameter, _speedHash, targetSpeed);
            }
            else
            {
                SafeSetBool(anim, isWalkingParameter, _isWalkingHash, false);
                SafeSetFloat(anim, speedParameter, _speedHash, 0f);
            }
        }
    }

    // Only set the parameter if it actually exists on the animator controller!
    private void SafeSetBool(Animator anim, string paramName, int hash, bool value)
    {
        foreach (var p in anim.parameters)
        {
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(hash, value);
                return;
            }
        }
    }

    private void SafeSetFloat(Animator anim, string paramName, int hash, float value)
    {
        foreach (var p in anim.parameters)
        {
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Float)
            {
                anim.SetFloat(hash, value);
                return;
            }
        }
    }
}
