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

    void Start()
    {
        // BUG FIX: The Animator is usually attached to the mesh deep inside the prefab!
        if (avatarAnimator == null) avatarAnimator = GetComponentInChildren<Animator>();
        if (stepManager == null) stepManager = FindFirstObjectByType<StepManager>();

        if (stepManager != null)
        {
            _lastStepCount = stepManager.currentDailySteps;
        }
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
            // Set the boolean to true for state transitions
            avatarAnimator.SetBool(isWalkingParameter, true);

            // If GPS says we are moving faster than 2.5 meters/second, feed a high speed for Run blends!
            if (stepManager.CurrentSpeedMPS > 2.5f)
            {
                avatarAnimator.SetFloat(speedParameter, 2.0f); // 2.0 = Running
            }
            else
            {
                avatarAnimator.SetFloat(speedParameter, 1.0f); // 1.0 = Walking
            }
        }
        else
        {
            avatarAnimator.SetBool(isWalkingParameter, false);
            avatarAnimator.SetFloat(speedParameter, 0f);       // 0.0 = Idle
        }
    }
}
