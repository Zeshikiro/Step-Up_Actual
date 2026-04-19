using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Needed to load the Map!

public class BMIManager : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField heightInput; // Expecting Centimeters
    public TMP_InputField weightInput; // Expecting Kilograms

    [Header("Display Elements")]
    public TextMeshProUGUI resultText;
    public GameObject continueButton;
    [Header("Routing")]
    public GameObject loginPanel;
    public GameObject bmiPanel;

    void Start()
    {
        // Hide the continue button until they actually calculate their BMI
        if (continueButton != null) 
        {
            continueButton.SetActive(false);
        }
    }

    public void CalculateBMI()
    {
        // Check if the user typed actual numbers
        if (float.TryParse(heightInput.text, out float heightCm) && float.TryParse(weightInput.text, out float weightKg))
        {
            // Math: BMI = kg / m^2
            float heightM = heightCm / 100f; 
            float bmi = weightKg / (heightM * heightM);
            
            int stepGoal = 10000;
            string category = "Normal";

            // WHO Guidelines Logic
            if (bmi < 18.5f) 
            {
                category = "Underweight";
                stepGoal = 8000; // Gradual start
            } 
            else if (bmi >= 18.5f && bmi <= 24.9f) 
            {
                category = "Normal Weight";
                stepGoal = 10000; // Standard baseline
            } 
            else if (bmi >= 25f && bmi <= 29.9f) 
            {
                category = "Overweight";
                stepGoal = 12000; // Increased for weight management
            } 
            else if (bmi >= 30f) 
            {
                category = "Obese";
                stepGoal = 8000; // Lowered to protect joints during early fitness
            }

            // Display the results formatting to 1 decimal place (e.g., 24.5)
            resultText.text = $"Your BMI: {bmi:F1}\nCategory: {category}\n\n<b>Daily Target: {stepGoal} Steps</b>";
            
            // Un-hide the continue button so they can enter the game
            continueButton.SetActive(true);
        }
        else
        {
            resultText.text = "<color=red>Please enter valid numbers!</color>";
        }
    }

    // This is the final bridge to the map!
    public void EnterGame()
    {
        // Hide the BMI Panel and Show the Login Panel
        bmiPanel.SetActive(false);
        loginPanel.SetActive(true);
    }
}