using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class UINumberCounter : MonoBehaviour
{
    private TMP_Text textComponent;
    private Coroutine countCoroutine;
    
    [Header("Formatting")]
    [Tooltip("Text added after the number (e.g. ' Steps')")]
    public string suffix = "";
    
    [Tooltip("If true, adds commas to large numbers (e.g. 10,000)")]
    public bool useCommas = true;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// Call this from any script to make the number rapidly tick up!
    /// Example: GetComponent<UINumberCounter>().CountTo(95000, 1.5f);
    /// </summary>
    public void CountTo(int targetValue, float duration = 1.0f)
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(CountRoutine(targetValue, duration));
    }

    /// <summary>
    /// Call this to animate decimal numbers!
    /// Example: GetComponent<UINumberCounter>().CountToFloat(12.5f, 1.5f, "F1");
    /// </summary>
    public void CountToFloat(float targetValue, float duration = 1.0f, string format = "F2")
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        
        if (countCoroutine != null) StopCoroutine(countCoroutine);
        countCoroutine = StartCoroutine(CountFloatRoutine(targetValue, duration, format));
    }

    private IEnumerator CountRoutine(int targetValue, float duration)
    {
        float timeElapsed = 0f;
        int startValue = 0; // Always start counting from 0 like an arcade machine

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            
            // Fast spin up, slow spin down
            float easeOutT = 1f - Mathf.Pow(1f - t, 3f); 
            
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, easeOutT));
            
            UpdateText(currentValue);
            yield return null;
        }

        // Snap to exact target at the end
        UpdateText(targetValue);
    }

    private IEnumerator CountFloatRoutine(float targetValue, float duration, string format)
    {
        float timeElapsed = 0f;
        float startValue = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = timeElapsed / duration;
            
            float easeOutT = 1f - Mathf.Pow(1f - t, 3f); 
            float currentValue = Mathf.Lerp(startValue, targetValue, easeOutT);
            
            UpdateTextFloat(currentValue, format);
            yield return null;
        }

        UpdateTextFloat(targetValue, format);
    }

    private void UpdateText(int value)
    {
        string formattedNumber = useCommas ? value.ToString("N0") : value.ToString();
        textComponent.text = formattedNumber + suffix;
    }

    private void UpdateTextFloat(float value, string format)
    {
        string formattedNumber = value.ToString(format);
        textComponent.text = formattedNumber + suffix;
    }
}
