using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIPanelAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How fast the panel slides and fades in.")]
    public float animationDuration = 0.3f;
    
    [Tooltip("How far down (in pixels) the panel starts before sliding up.")]
    public float slideOffset = -150f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool positionSaved = false;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        // Save the perfect resting position
        originalPosition = rectTransform.anchoredPosition;
        positionSaved = true;
    }

    // OnEnable is called automatically every time the panel is turned on (SetActive(true))
    void OnEnable()
    {
        if (canvasGroup == null || rectTransform == null) return;
        
        if (!positionSaved)
        {
            originalPosition = rectTransform.anchoredPosition;
            positionSaved = true;
        }

        // Reset to starting position
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + slideOffset);

        // Stop any old animations and start a new one
        StopAllCoroutines();
        StartCoroutine(AnimateIn());
    }

    private IEnumerator AnimateIn()
    {
        float timeElapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;

        while (timeElapsed < animationDuration)
        {
            timeElapsed += Time.deltaTime;
            
            // Calculate a smooth mathematical curve (Ease Out)
            float t = timeElapsed / animationDuration;
            float easeOutT = 1f - Mathf.Pow(1f - t, 3f); // Creates a snappy, arcade-like deceleration

            // Apply fade, slide, AND soft scale popup!
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeOutT);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPosition, easeOutT);
            
            // Starts slightly smaller (0.9x) and pops into normal size (1x)
            float scale = Mathf.Lerp(0.9f, 1f, easeOutT);
            rectTransform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        // Snap to exact final values
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = originalPosition;
        rectTransform.localScale = Vector3.one;
    }
}
