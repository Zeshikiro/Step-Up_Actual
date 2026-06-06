using UnityEngine;

public class AvatarAnimatorSync : MonoBehaviour
{
    [Header("References")]
    public Animator avatarAnimator;
    public StepManager stepManager;

    [Header("Animation State Names")]
    [Tooltip("Exact name of the Idle animation state in your Animator")]
    public string idleStateName = "CharacterArmature|Idle";
    [Tooltip("Exact name of the Walk animation state in your Animator")]
    public string walkStateName = "CharacterArmature|Walk";
    [Tooltip("Exact name of the Run animation state in your Animator")]
    public string runStateName = "CharacterArmature|Run";

    private int _lastStepCount;
    private float _lastStepTime;
    private string _currentState = "";

    void Start()
    {
        if (avatarAnimator == null) avatarAnimator = GetComponent<Animator>();
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();

        if (stepManager != null)
        {
            _lastStepCount = stepManager.currentDailySteps;
        }

        PlayAnimation(idleStateName);
    }

    void Update()
    {
        if (stepManager == null || avatarAnimator == null) return;

        // Did we just take a step?
        if (stepManager.currentDailySteps > _lastStepCount)
        {
            _lastStepCount = stepManager.currentDailySteps;
            _lastStepTime = Time.time;
        }

        // If we took a step within the last 1.5 seconds, we are moving!
        bool isMoving = (Time.time - _lastStepTime) < 1.5f;

        if (isMoving)
        {
            // If GPS says we are moving faster than 2.5 meters/second, it's a run!
            if (stepManager.CurrentSpeedMPS > 2.5f)
            {
                PlayAnimation(runStateName);
            }
            else
            {
                PlayAnimation(walkStateName);
            }
        }
        else
        {
            PlayAnimation(idleStateName);
        }
    }

    private void PlayAnimation(string stateName)
    {
        if (string.IsNullOrEmpty(stateName)) return;

        // Only CrossFade if we aren't already playing this exact state
        if (_currentState != stateName)
        {
            avatarAnimator.CrossFadeInFixedTime(stateName, 0.25f);
            _currentState = stateName;
        }
    }
}
