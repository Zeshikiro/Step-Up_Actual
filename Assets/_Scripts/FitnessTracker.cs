using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TrailRenderer))]
public class FitnessTracker : MonoBehaviour
{
    [Header("Tracking Status")]
    public bool isTracking = false;
    
    [Header("Timer Display")]
    public TextMeshProUGUI timerText; // Drag a UI Text here

    [Header("Button Graphics")]
    public Image playPauseButtonImage; // Drag your Play/Pause button here
    public Sprite playSprite; // Drag your Play icon here
    public Sprite pauseSprite; // Drag your Stop icon here

    [Header("UI Routing")]
    public GameplayUIManager uiManager; // Drag your GameplayUIManager here!
    
    private float activeTime = 0f;
    private TrailRenderer trail;

    void Start()
    {
        // Force the timer to wait for the play button, ignoring the Inspector checkbox
        isTracking = false; 

        trail = GetComponent<TrailRenderer>();
        
        // Setup a nice default Strava-style trail automatically
        trail.startWidth = 2f;
        trail.endWidth = 2f;
        
        // Use a simple unlit material so the color pops
        trail.material = new Material(Shader.Find("Sprites/Default"));
        
        // Classic Strava Orange
        trail.startColor = new Color(1f, 0.3f, 0f, 0.8f); 
        trail.endColor = new Color(1f, 0.5f, 0f, 0.8f);
        
        trail.time = Mathf.Infinity; // The trail stays forever during this session
        trail.minVertexDistance = 1f; // Only draw when moving at least 1 unit to save performance
    }

    void Update()
    {
        if (isTracking)
        {
            activeTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(activeTime / 60F);
            int seconds = Mathf.FloorToInt(activeTime - minutes * 60);
            
            // Format as MM:SS
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Call these methods from UI buttons if you want Start/Pause buttons!
    public void PauseTracking()
    {
        isTracking = false;
    }

    public void ResumeTracking()
    {
        isTracking = true;
    }

    public void ToggleTracking()
    {
        isTracking = !isTracking;

        // Swap the button image!
        if (playPauseButtonImage != null)
        {
            if (isTracking)
            {
                playPauseButtonImage.sprite = pauseSprite; // If we are tracking, show the Stop icon
            }
            else
            {
                playPauseButtonImage.sprite = playSprite; // If we are stopped, show the Play icon
            }
        }

        // If they just hit STOP, route them to the Summary Panel and reset the timer!
        if (!isTracking)
        {
            // Reset the active time so the next run starts at 00:00
            activeTime = 0f;
            UpdateTimerUI();

            if (uiManager != null)
            {
                uiManager.OpenSummaryPanel();
            }
        }
    }
}
