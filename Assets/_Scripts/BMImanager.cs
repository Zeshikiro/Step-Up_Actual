using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Database;

public class BMIManager : MonoBehaviour
{
    [Header("Input Fields (The Survey)")]
    public TMP_InputField ageInput; 
    public TMP_InputField heightInput;
    public TMP_InputField weightInput;

    [Header("Display Elements")]
    public TextMeshProUGUI resultText;
    
    [Header("Buttons")]
    public GameObject calculateButton; // The main "Calculate BMI" button
    public GameObject secondaryButtonGroup; // The layout group holding Re-Calculate & Continue

    [Header("Routing")]
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // Routes to your new Main Menu!

    void Start()
    {
        // Initial state: Show Calculate, Hide the secondary buttons
        if (calculateButton != null) calculateButton.SetActive(true);
        if (secondaryButtonGroup != null) secondaryButtonGroup.SetActive(false);
    }

    void OnEnable()
    {
        // Pre-fill the fields with saved data if it exists!
        if (ageInput != null && PlayerPrefs.HasKey("SavedAge"))
            ageInput.text = PlayerPrefs.GetString("SavedAge");
            
        if (heightInput != null && PlayerPrefs.HasKey("SavedHeight"))
            heightInput.text = PlayerPrefs.GetString("SavedHeight");
            
        if (weightInput != null && PlayerPrefs.HasKey("SavedWeight"))
            weightInput.text = PlayerPrefs.GetString("SavedWeight");
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

            // Save the calculated goal and category
            PlayerPrefs.SetInt("DailyStepGoal", stepGoal);
            PlayerPrefs.SetString("BMICategory", category);

            // Save the raw inputs so they can be loaded later!
            PlayerPrefs.SetString("SavedAge", ageInput.text);
            PlayerPrefs.SetString("SavedHeight", heightInput.text);
            PlayerPrefs.SetString("SavedWeight", weightInput.text);

            // Mark the BMI profile setup as officially done
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                PlayerPrefs.SetInt("BMI_Setup_Complete_" + userId, 1); 
            }

            PlayerPrefs.Save();

            // Push BMI to Firebase
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                dbRef.Child("users").Child(userId).Child("bmi").SetValueAsync(bmi);
                dbRef.Child("users").Child(userId).Child("bmiCategory").SetValueAsync(category);
                dbRef.Child("users").Child(userId).Child("stepGoal").SetValueAsync(stepGoal);
            }
            
            // Display results in a summarized format to fit the box
            resultText.text = $"{bmi:F1}\n<size=50%>{category}</size>";
            
            // SWAP BUTTONS! Hide calculate, show the Continue/Recalculate group
            if (calculateButton != null) calculateButton.SetActive(false);
            if (secondaryButtonGroup != null) secondaryButtonGroup.SetActive(true);
        }
        else
        {
            resultText.text = "<color=red>Please enter valid numbers in all fields!</color>";
        }
    }

    // Call this from the "Re-Calculate" button
    public void ResetCalculator()
    {
        // Clear result
        if (resultText != null) resultText.text = "Result is 0";
        
        // SWAP BUTTONS! Show calculate, hide the group
        if (calculateButton != null) calculateButton.SetActive(true);
        if (secondaryButtonGroup != null) secondaryButtonGroup.SetActive(false);
    }

    // Call this from the "Continue" button
    public void GoToMainMenu()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
            PlayerPrefs.SetInt("OnboardingComplete_" + userId, 1); 
        }
        PlayerPrefs.Save();
        
        if (bmiPanel != null) bmiPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
}