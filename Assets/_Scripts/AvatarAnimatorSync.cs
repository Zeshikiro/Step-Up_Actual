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

        [Header("Animation Speeds")]
    [Tooltip("The animator Speed value for idle/standing still")]
    public float idleAnimValue = 1.0f;
    [Tooltip("The animator Speed value for walking")]
    public float walkAnimValue = 1.0f;
    [Tooltip("The animator Speed value for running")]
    public float runAnimValue = 2.0f;
    [Tooltip("The speed at which the animation smoothly transitions between walking and running (higher = faster snap)")]
    public float animationLerpSpeed = 3.0f;

    // Smoothly track current animation speed to stop "tweaking/jittering"
    private float _currentSmoothSpeed = 1.0f;

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
        }        // 2. Check for Physical GPS Sliding on the TRACKER transform (not this object!)
        Transform posSource = _trackerTransform != null ? _trackerTransform : transform;
        float distanceMoved = Vector3.Distance(posSource.position, _lastPosition);
        
        // FIX: Only trigger sliding if we are actively moving fast enough via GPS (ignore GPS drift while phone is on table!)
        if (distanceMoved > 0.05f && stepManager != null && stepManager.CurrentSpeedMPS > 0.5f) 
        {
            _lastMoveTime = Time.time;
        }
        _lastPosition = posSource.position;

        // 3. Accelerometer Visual Hack (Instant Responsiveness!)
        // Check if the phone is physically bouncing (magnitude != 1G gravity).
        // If the magnitude deviates by 0.15G, the user is walking/bobbing the phone!
        float accelMagnitude = 1.0f;
        try 
        {
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Accelerometer.current != null)
                accelMagnitude = UnityEngine.InputSystem.Accelerometer.current.acceleration.ReadValue().magnitude;
            else
#endif
                accelMagnitude = Input.acceleration.magnitude;
        }
        catch (System.Exception) { /* Ignored if unavailable */ }
        
        // When human walks, the phone bobs up and down, changing G-force.
        // Lowered threshold to 0.15G so normal smooth walking triggers it without needing to violently bounce the phone!
        if (Mathf.Abs(accelMagnitude - 1.0f) > 0.15f)
        {
            _lastMoveTime = Time.time; 
        }

        // Reduced pedometer timeout to 2 seconds so the avatar instantly stops walking when you stop!
        // But since the pedometer only batches every 10 seconds, this was causing stuttering.
        // The bounce detection (_lastMoveTime) is now the primary driver for smooth animation!
        bool isStepping = (Time.time - _lastStepTime) < 2.0f;  
        bool isSliding = (Time.time - _lastMoveTime) < 1.0f; // Maintains walk state for 1s after last bounce

        bool shouldWalk = isStepping || isSliding;
        
        // 3. Smooth the target speed so it doesn't instantly snap (causing the 'tweaking/jittering' look)
        float targetSpeed = idleAnimValue;
        if (shouldWalk)
        {
            // Lowered run threshold to 2.2 m/s (approx 8 km/h or a light jog) so running indoors actually triggers the run animation!
            targetSpeed = (stepManager != null && stepManager.CurrentSpeedMPS > 2.2f) ? runAnimValue : walkAnimValue;
        }

        // Halved the lerp speed from 3.0 to 1.5 so it smoothly blends between walk/run/idle instead of snapping!
        _currentSmoothSpeed = Mathf.Lerp(_currentSmoothSpeed, targetSpeed, Time.deltaTime * 1.5f);

        foreach (var anim in _validAnimators)
        {
            if (anim == null || !anim.isActiveAndEnabled) continue;
            
            if (shouldWalk)
            {
                anim.SetBool(_isWalkingHash, true);
                anim.SetFloat(_speedHash, _currentSmoothSpeed);
            }
            else
            {
                anim.SetBool(_isWalkingHash, false);
                // We let it blend down to 0 smoothly even if IsWalking is false
                anim.SetFloat(_speedHash, _currentSmoothSpeed);
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


