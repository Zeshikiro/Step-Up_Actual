using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EULAManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Toggle agreementToggle;
    public TextMeshProUGUI warningText;
    
    [Header("Panels")]
    public GameObject eulaPanel;
    public GameObject bmiPanel;

    void Start()
    {
        // Ensure the warning is hidden when the screen first loads
        if (warningText != null) 
        {
            warningText.gameObject.SetActive(false);
        }
    }

    public void TryToContinue()
    {
        // Did they check the box?
        if (agreementToggle.isOn)
        {
            // Yes! Hide the EULA, show the BMI panel
            eulaPanel.SetActive(false);
            bmiPanel.SetActive(true);
        }
        else
        {
            // No! Show the red warning text
            warningText.gameObject.SetActive(true);
        }
    }
}