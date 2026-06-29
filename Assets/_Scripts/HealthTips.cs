using UnityEngine;

public class HealthTips : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject healthTipsHubPanel;

    // Call this from the Main Menu button to open the hub
    public void OpenHealthTipsHub()
    {
        if (healthTipsHubPanel != null) healthTipsHubPanel.SetActive(true);
    }

    // Call this from your new Close button to hide the hub
    public void CloseHealthTipsHub()
    {
        if (healthTipsHubPanel != null) healthTipsHubPanel.SetActive(false);
    }

    // Call this from your Posture, Cooldown, WarmUp, and FitnessTips buttons!
    public void OpenWebsiteLink(string url)
    {
        Debug.Log("Opening companion website: " + url);
        Application.OpenURL(url); // This will open the phone's default web browser
    }
}