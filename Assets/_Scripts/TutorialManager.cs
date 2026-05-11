using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject tutorialHubPanel;

    // Call this from the Main Menu button to open the hub
    public void OpenTutorialHub()
    {
        tutorialHubPanel.SetActive(true);
    }

    // Call this from your new Close button to hide the hub
    public void CloseTutorialHub()
    {
        tutorialHubPanel.SetActive(false);
    }

    // Call this from your Posture, Cooldown, WarmUp, and FitnessTips buttons!
    public void OpenWebsiteLink(string url)
    {
        Debug.Log("Opening companion website: " + url);
        Application.OpenURL(url); // This will open the phone's default web browser
    }
}