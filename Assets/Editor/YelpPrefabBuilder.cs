using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class YelpPrefabBuilder : EditorWindow
{
    [MenuItem("Step Up Tools/Generate Yelp Map Pin Prefab")]
    public static void GeneratePrefab()
    {
        // 1. Create the base Canvas
        GameObject root = new GameObject("GoogleMapPin");
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRT = root.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(400, 300);
        
        // Scale it down heavily because World Space canvases are massive (1 pixel = 1 meter)
        root.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

        // Add Billboard Facer script so it always looks at the camera
        root.AddComponent<YelpBillboardFacer>();

        // 2. Create the Background Panel
        GameObject panel = new GameObject("BackgroundPanel");
        panel.transform.SetParent(root.transform, false);
        Image bgImage = panel.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f); // Dark grey background
        
        RectTransform panelRT = panel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0, 0);
        panelRT.anchorMax = new Vector2(1, 1);
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // 3. Create the RawImage for the Yelp Photo
        GameObject photo = new GameObject("YelpPhoto");
        photo.transform.SetParent(panel.transform, false);
        RawImage rawImage = photo.AddComponent<RawImage>();
        rawImage.color = Color.white;
        
        RectTransform photoRT = photo.GetComponent<RectTransform>();
        photoRT.anchorMin = new Vector2(0, 0.2f); // Leaves bottom 20% for text
        photoRT.anchorMax = new Vector2(1, 1);
        photoRT.offsetMin = new Vector2(10, 10);
        photoRT.offsetMax = new Vector2(-10, -10);

        // 4. Create the TextMeshPro label
        GameObject label = new GameObject("TitleText");
        label.transform.SetParent(panel.transform, false);
        TextMeshProUGUI tmp = label.AddComponent<TextMeshProUGUI>();
        tmp.text = "Loading Place...";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.CenterGeoAligned;
        tmp.color = Color.white;
        
        RectTransform labelRT = label.GetComponent<RectTransform>();
        labelRT.anchorMin = new Vector2(0, 0);
        labelRT.anchorMax = new Vector2(1, 0.2f);
        labelRT.offsetMin = new Vector2(10, 0);
        labelRT.offsetMax = new Vector2(-10, 0);

        // 5. Save it as a Prefab automatically
        string localPath = "Assets/_Prefabs/GoogleMapPin.prefab";
        
        // Make sure the directory exists
        if (!AssetDatabase.IsValidFolder("Assets/_Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "_Prefabs");
        }

        PrefabUtility.SaveAsPrefabAsset(root, localPath);
        DestroyImmediate(root); // Delete the temporary scene object

        Debug.Log("[YelpPrefabBuilder] Successfully created and saved GoogleMapPin prefab at " + localPath);
    }
}
