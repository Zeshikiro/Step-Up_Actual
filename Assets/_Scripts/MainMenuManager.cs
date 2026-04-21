using UnityEngine;
using UnityEngine.SceneManagement; // This is the magic word for changing scenes!

public class MainMenuManager : MonoBehaviour
{
    // This function will be called when you click the Start button
    public void StartGame()
    {
        // Replace "SampleScene" with the EXACT name of your Map scene if it's different!
        SceneManager.LoadScene("SampleScene"); 
    }

    // We'll use these later for your other buttons!
    public void OpenCustomize()
    {
        Debug.Log("Opening Customize Menu...");
    }

    public void OpenSettings()
    {
        Debug.Log("Opening Settings Menu...");
    }

    public void OpenHealthTips()
    {
        Debug.Log("Opening Health Tips...");
    }
}