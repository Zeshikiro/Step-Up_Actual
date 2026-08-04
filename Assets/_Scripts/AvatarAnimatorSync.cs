using UnityEngine;
using System.Collections.Generic;

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

    // Cached parameters
    private int _speedHash;
    private int _isWalkingHash;

    // Track which animators actually belong to avatars!
    private List<Animator> _validAnimators = new List<Animator>();
    private float _lastScanTime = 0f;

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

        // Scan for new animators every 2 seconds in case an avatar was spawned dynamically
        if (Time.time - _lastScanTime > 2.0f)
        {
            _lastScanTime = Time.time;
            ScanForValidAnimators();
        }

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
        bool isStepping = (Time.time - _lastStepTime) < 3.0f;  // Extended to 3s for smoother animation
        bool isSliding = (Time.time - _lastMoveTime) < 1.5f;

        bool shouldWalk = isStepping || isSliding;

        foreach (var anim in _validAnimators)
        {
            if (anim == null || !anim.isActiveAndEnabled) continue;
            
            if (shouldWalk)
            {
                float targetSpeed = (stepManager.CurrentSpeedMPS > 2.5f) ? 2.0f : 1.0f;
                anim.SetBool(_isWalkingHash, true);
                anim.SetFloat(_speedHash, targetSpeed);
            }
            else
            {
                anim.SetBool(_isWalkingHash, false);
                anim.SetFloat(_speedHash, 0f);
            }
        }
    }

    private void ScanForValidAnimators()
    {
        _validAnimators.Clear();
        Animator[] allAnimators = Object.FindObjectsByType<Animator>(FindObjectsSortMode.None);
        
        // Setup fallback hashes just in case
        int fallbackWalkingHash1 = Animator.StringToHash("Walk");
        int fallbackWalkingHash2 = Animator.StringToHash("Walking");
        int fallbackWalkingHash3 = Animator.StringToHash("isWalking");
        int fallbackSpeedHash1 = Animator.StringToHash("Blend");

        foreach (var anim in allAnimators)
        {
            if (anim == null || anim.runtimeAnimatorController == null) continue;
            
            bool hasWalk = false;
            bool hasSpeed = false;

            foreach (var p in anim.parameters)
            {
                if ((p.nameHash == _isWalkingHash || p.nameHash == fallbackWalkingHash1 || p.nameHash == fallbackWalkingHash2 || p.nameHash == fallbackWalkingHash3) && p.type == AnimatorControllerParameterType.Bool)
                {
                    hasWalk = true;
                    _isWalkingHash = p.nameHash;
                }
                if ((p.nameHash == _speedHash || p.nameHash == fallbackSpeedHash1) && p.type == AnimatorControllerParameterType.Float)
                {
                    hasSpeed = true;
                    _speedHash = p.nameHash;
                }
            }

            // Only store animators that actually have AT LEAST ONE of the required parameters!
            if (hasWalk || hasSpeed)
            {
                _validAnimators.Add(anim);
                Debug.Log($"[AvatarAnimatorSync] Valid Avatar Animator found: '{anim.name}' (HasWalk={hasWalk}, HasSpeed={hasSpeed})");
            }
        }
    }
}
