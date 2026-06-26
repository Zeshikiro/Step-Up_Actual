using UnityEngine;

public class AvatarPreviewCamera : MonoBehaviour
{
    [Header("Target References")]
    [Tooltip("Drag your PlayerAvatar GameObject here")]
    public Transform targetAvatar; 
    
    [Header("Camera Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 30f; // Speed of auto-rotation

    [Header("Zoom Targets (World Offsets)")]
    public Vector3 fullBodyOffset = new Vector3(0, 1.0f, -3.5f);
    public Vector3 headOffset     = new Vector3(0, 1.6f, -1.2f);
    public Vector3 torsoOffset    = new Vector3(0, 1.1f, -1.5f);
    public Vector3 legsOffset     = new Vector3(0, 0.6f, -1.5f);
    public Vector3 shoesOffset    = new Vector3(0, 0.2f, -1.2f);

    private Vector3 currentTargetOffset;

    void Start()
    {
        currentTargetOffset = fullBodyOffset;
        if (targetAvatar != null)
        {
            // Snap camera instantly on start
            transform.position = targetAvatar.position + currentTargetOffset;
            transform.LookAt(targetAvatar.position + Vector3.up * currentTargetOffset.y);
        }
    }

    void Update()
    {
        if (targetAvatar == null) return;

        // 1. Auto-Rotate the Avatar (Smooth spinning like a jewelry display)
        targetAvatar.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // 2. Calculate where the camera SHOULD be
        Vector3 desiredPosition = targetAvatar.position + currentTargetOffset;
        
        // 3. Smoothly glide the camera to the target offset
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * moveSpeed);
        
        // 4. Smoothly tilt the camera to look perfectly at the selected body part height
        Vector3 lookTarget = targetAvatar.position + Vector3.up * currentTargetOffset.y;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * moveSpeed);
    }

    // ==========================================
    // 🖲️ UI BUTTON TRIGGERS 
    // Link these directly to your OnClick() events!
    // ==========================================
    
    public void ZoomToFullBody() { currentTargetOffset = fullBodyOffset; }
    public void ZoomToHead()     { currentTargetOffset = headOffset; }
    public void ZoomToTorso()    { currentTargetOffset = torsoOffset; }
    public void ZoomToLegs()     { currentTargetOffset = legsOffset; }
    public void ZoomToShoes()    { currentTargetOffset = shoesOffset; }
    
    // Bonus: Call this if they purchase/equip an item for a cool dramatic spin!
    public void CelebrationSpin()
    {
        rotationSpeed = 200f; // Fast spin!
        CancelInvoke("ResetRotationSpeed");
        Invoke("ResetRotationSpeed", 1.0f);
    }
    
    private void ResetRotationSpeed()
    {
        rotationSpeed = 30f; // Back to chill display spin
    }
}
