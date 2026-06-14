using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Mapbox.Unity.Map;
using Mapbox.Utils;

[System.Serializable]
public class YelpResponse
{
    public YelpBusiness[] businesses;
}

[System.Serializable]
public class YelpBusiness
{
    public string id;
    public string name;
    public string image_url;
    public YelpCoordinates coordinates;
}

[System.Serializable]
public class YelpCoordinates
{
    public double latitude;
    public double longitude;
}

public class YelpPlacesManager : MonoBehaviour
{
    public static YelpPlacesManager Instance { get; private set; }

    [Header("Yelp API Configuration")]
    [Tooltip("Paste your Yelp API Key here!")]
    public string yelpApiKey = "K0OhxkJ5Ig_1RkykCaAnU6Ico1FOsKmW951hL3w1B3OjGrBKoRSqTT8fIdAhMmDq5N4BSot3BeUbdQYwcT1yaEdMGTw3MXM0WB65GkqXxZAt7e9UiVGjwP_cG8gsanYx";
    
    [Tooltip("How many locations to show? Limit to 5 or 10 for performance.")]
    public int searchLimit = 5;

    [Tooltip("Search radius in meters.")]
    public int searchRadius = 1000;

    [Header("Mapbox Integration")]
    public AbstractMap mapManager;

    [Header("UI Prefab")]
    public GameObject mapPinBillboardPrefab;

    // Track active pins so we don't spawn duplicates or leave old ones forever
    private Dictionary<string, GameObject> activePins = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public void FetchNearbyPlaces(Vector2d playerLocation)
    {
        if (string.IsNullOrEmpty(yelpApiKey))
        {
            Debug.LogError("[YelpManager] API Key is missing!");
            return;
        }

        StartCoroutine(RequestYelpData(playerLocation));
    }

    private IEnumerator RequestYelpData(Vector2d location)
    {
        // We removed the strict category filters so it will ALWAYS find places, even in residential areas!
        string url = $"https://api.yelp.com/v3/businesses/search?latitude={location.x}&longitude={location.y}&radius={searchRadius}&limit={searchLimit}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            webRequest.SetRequestHeader("Authorization", "Bearer " + yelpApiKey);

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Parse the Yelp JSON response using Unity's built-in JSON system
                YelpResponse response = JsonUtility.FromJson<YelpResponse>(webRequest.downloadHandler.text);
                
                if (response != null && response.businesses != null)
                {
                    Debug.Log($"[YelpManager] Successfully found {response.businesses.Length} cool places nearby!");
                    ProcessBusinesses(response.businesses);
                }
            }
            else
            {
                Debug.LogError($"[YelpManager] API Request Failed! Error: {webRequest.error}");
            }
        }
    }

    private void ProcessBusinesses(YelpBusiness[] businesses)
    {
        // Optional: If you want to clear old pins when you move far away, you would do it here.
        // For now, we will just spawn new ones and avoid duplicates.

        foreach (var biz in businesses)
        {
            if (activePins.ContainsKey(biz.id)) continue; // We already spawned this pin!

            // Spawn the billboard!
            StartCoroutine(SpawnAndBuildPin(biz));
        }
    }

    private IEnumerator SpawnAndBuildPin(YelpBusiness biz)
    {
        if (mapPinBillboardPrefab == null || mapManager == null)
        {
            Debug.LogWarning("[YelpManager] Missing MapManager or Pin Prefab!");
            yield break;
        }

        // 1. Calculate where on the 3D map this business actually is
        Vector2d geoCoord = new Vector2d(biz.coordinates.latitude, biz.coordinates.longitude);
        Vector3 worldPosition = mapManager.GeoToWorldPosition(geoCoord, true);
        
        // Float the billboard slightly above the ground
        worldPosition.y += 10f; 

        // 2. Spawn the Billboard UI
        GameObject newPin = Instantiate(mapPinBillboardPrefab, worldPosition, Quaternion.identity);
        newPin.transform.SetParent(mapManager.transform, true); // Parent to map so it moves when map moves!
        newPin.name = $"YelpPin_{biz.name}";

        activePins.Add(biz.id, newPin);

        // 3. Set the Title Text
        TextMeshProUGUI titleText = newPin.GetComponentInChildren<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = biz.name;
        }

        // 4. Download and set the photo!
        if (!string.IsNullOrEmpty(biz.image_url))
        {
            RawImage photoImage = newPin.GetComponentInChildren<RawImage>();
            if (photoImage != null)
            {
                using (UnityWebRequest imgReq = UnityWebRequestTexture.GetTexture(biz.image_url))
                {
                    yield return imgReq.SendWebRequest();

                    if (imgReq.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D downloadedTex = DownloadHandlerTexture.GetContent(imgReq);
                        photoImage.texture = downloadedTex;
                    }
                }
            }
        }
    }
}
