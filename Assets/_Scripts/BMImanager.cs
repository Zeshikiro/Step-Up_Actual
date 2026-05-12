using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BMIManager : MonoBehaviour
{
    [Header("Input Fields (The Survey)")]
    public TMP_InputField ageInput; 
    public TMP_InputField heightInput;
    public TMP_InputField weightInput;

    [Header("Display Elements")]
    public TextMeshProUGUI resultText;
    public GameObject continueButton;

    [Header("Routing")]
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // Routes to your new Main Menu!

    void Start()
    {
        // Hide the continue button until they calculate
        if (continueButton != null) 
        {
            continueButton.SetActive(false);
        }
    }

    public void CalculateBMI()
    {
        // Check if they filled out the survey (Age, Height, Weight)
        if (int.TryParse(ageInput.text, out int age) && 
            float.TryParse(heightInput.text, out float heightCm) && 
            float.TryParse(weightInput.text, out float weightKg))
        {
            // Standard BMI Math
            float heightM = heightCm / 100f; 
            float bmi = weightKg / (heightM * heightM);
            
            int stepGoal = 10000;
            string category = "Normal";

            // WHO Guidelines Logic
            if (bmi < 18.5f) { category = "Underweight"; stepGoal = 8000; } 
            else if (bmi >= 18.5f && bmi <= 24.9f) { category = "Normal Weight"; stepGoal = 10000; } 
            else if (bmi >= 25f && bmi <= 29.9f) { category = "Overweight"; stepGoal = 12000; } 
            else if (bmi >= 30f) { category = "Obese"; stepGoal = 8000; }

            // Save the calculated goal and category so the Missions panel can use them!
            PlayerPrefs.SetInt("DailyStepGoal", stepGoal);
            PlayerPrefs.SetString("BMICategory", category);
            PlayerPrefs.Save();
            
            // Display results
            resultText.text = $"Your BMI: {bmi:F1}\nCategory: {category}\n\n<b>Daily Target: {stepGoal} Steps</b>";
            
            // Show the continue button
            continueButton.SetActive(true);
        }
        else
        {
            resultText.text = "<color=red>Please enter valid numbers in all fields!</color>";
        }
    }

    // Call this from the "Continue" button
    public void GoToMainMenu()
    {
        PlayerPrefs.SetInt("OnboardingComplete", 1); 
        PlayerPrefs.Save();
        bmiPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}