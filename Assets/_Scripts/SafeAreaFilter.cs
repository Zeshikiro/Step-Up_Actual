using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFilter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private Vector2 lastScreenSize = Vector2.zero;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        RefreshSafeArea();
    }

    void Update()
    {
        // Continuously checks if screen changes orientation or resolution scales
        if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            RefreshSafeArea();
        }
    }

    void RefreshSafeArea()
    {
        Rect safeArea = Screen.safeArea;

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2(Screen.width, Screen.height);

        // Convert raw hardware pixel space to normalized Canvas UI space
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Apply constraints dynamically to the panel
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}