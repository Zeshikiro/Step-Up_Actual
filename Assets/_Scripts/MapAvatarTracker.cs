using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.Location;

public class MapAvatarTracker : MonoBehaviour
{
    [Header("Mapbox References")]
    public AbstractMap mapManager; // Drag your CitySimulatorMap here

    private ILocationProvider _locationProvider;

    void Start()
    {
        // Auto-find the map if you forget to drag it in
        if (mapManager == null) 
        {
            mapManager = FindFirstObjectByType<AbstractMap>();
        }

        // Get the Mapbox GPS Location Provider
        if (LocationProviderFactory.Instance != null)
        {
            _locationProvider = LocationProviderFactory.Instance.DefaultLocationProvider;
        }
    }

    void Update()
    {
        // Don't do anything if the map or GPS isn't ready yet
        if (mapManager == null || _locationProvider == null) 
            return;

        // 1. Get real-world GPS coordinates (Latitude/Longitude)
        var gpsLocation = _locationProvider.CurrentLocation.LatitudeLongitude;

        // 2. Convert real-world GPS into Unity 3D World space
        Vector3 targetPosition = mapManager.GeoToWorldPosition(gpsLocation, true);
        
        // Keep the avatar at ground level (Y = 0) so it doesn't fly or sink
        targetPosition.y = 0f;

        // 3. Smoothly move the Avatar to the new location (Lerp makes walking look smooth instead of teleporting)
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }
}
