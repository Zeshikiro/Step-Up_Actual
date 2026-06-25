using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Juice Settings")]
    [Tooltip("How much smaller the button gets when pressed (0.9 = 90% size)")]
    public float pressedScale = 0.9f;
    [Tooltip("How fast the button pops")]
    public float animationSpeed = 15f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale * pressedScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        while (Vector3.Distance(transform.localScale, targetScale) > 0.01f)
        {
            // Smoothly interpolate towards the target scale
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }
}
