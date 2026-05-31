using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions; 
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections; 

public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    public GameObject loginPanel;
    public GameObject eulaPanel;
    public GameObject bmiPanel;
    public GameObject mainMenuPanel; // We need this to skip the BMI!

    [Header("Lockout System")]
    public Button loginButton; 
    public TextMeshProUGUI statusText; 
    private int failedAttempts = 0; 

    [Header("Password Visibility")]
    private bool isPasswordVisible = false; // Tracks if the eye is open or closed

    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                auth = FirebaseAuth.DefaultInstance;

                // ==============================================
                // "REMEMBER ME"
                // ==============================================
                if (auth.CurrentUser != null) {
                    if (!auth.CurrentUser.IsEmailVerified) {
                        auth.SignOut();
                    } else {
                        Debug.Log("User recognized! Bypassing login...");
                        if (loginPanel != null) loginPanel.SetActive(false);

                        // Save email to DB to ensure leaderboard visibility
                        FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(auth.CurrentUser.UserId).Child("email").SetValueAsync(auth.CurrentUser.Email);

                        // --- THE NEW ONBOARDING LOCK CHECK ---
                        if (PlayerPrefs.GetInt("OnboardingComplete", 0) == 1) 
                        {
                            if (eulaPanel != null) eulaPanel.SetActive(false);
                            if (bmiPanel != null) bmiPanel.SetActive(false);
                            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
                        }
                        else 
                        {
                            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
                            if (bmiPanel != null) bmiPanel.SetActive(false);
                            if (eulaPanel != null) eulaPanel.SetActive(true);
                        }
                    }
                }

            } else {
                Debug.LogError("Could not resolve Firebase dependencies: " + dependencyStatus);
            }
        });

        // Make sure the password field starts hidden (as dots)
        if (passwordField != null) {
            passwordField.contentType = TMP_InputField.ContentType.Password;
            passwordField.ForceLabelUpdate();
        }
    }

    // ==============================================
    // NEW: SHOW/HIDE PASSWORD FUNCTION
    // ==============================================
    public void TogglePasswordVisibility()
    {
        if (passwordField == null) return;

        isPasswordVisible = !isPasswordVisible; // Flip the true/false state

        if (isPasswordVisible)
        {
            // Show the actual letters
            passwordField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            // Hide the letters with dots/stars
            passwordField.contentType = TMP_InputField.ContentType.Password;
        }

        // This forces TMPro to instantly redraw the text on screen!
        passwordField.ForceLabelUpdate(); 
    }

    public void RegisterUser()
    {
        if (auth == null) return;
        if (loginButton != null) loginButton.interactable = false;
        if (statusText != null) statusText.text = "Registering...";
        
        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) {
                if(statusText != null) statusText.text = "Registration Failed.";
                if (loginButton != null) loginButton.interactable = true;
                return;
            }
            
            FirebaseUser newUser = auth.CurrentUser;
            if (newUser != null)
            {
                newUser.SendEmailVerificationAsync().ContinueWithOnMainThread(emailTask => {
                    if (loginButton != null) loginButton.interactable = true;
                    if (emailTask.IsFaulted || emailTask.IsCanceled) {
                        if(statusText != null) statusText.text = "Failed to send verification email.";
                        return;
                    }
                    if(statusText != null) statusText.text = "Verification email sent! Please check your inbox.";
                    auth.SignOut(); // Force them to log in later
                });
            }
        });
    }

    public void LoginUser()
    {
        if (auth == null) return;

        if (loginButton != null) loginButton.interactable = false;
        if (statusText != null) statusText.text = "Checking credentials..."; 

        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                
                failedAttempts++;
                if (failedAttempts >= 3)
                {
                    StartCoroutine(LockoutRoutine());
                }
                else
                {
                    if(statusText != null) 
                        statusText.text = $"Wrong password/email. You only have {3 - failedAttempts} entries left.";
                    
                    if (loginButton != null) loginButton.interactable = true; 
                }
                return; 
            }
            
            FirebaseUser user = auth.CurrentUser;
            if (user != null && !user.IsEmailVerified)
            {
                if(statusText != null) statusText.text = "Please verify your email first!";
                if (loginButton != null) loginButton.interactable = true;
                auth.SignOut();
                return;
            }

            failedAttempts = 0; 
            if(statusText != null) statusText.text = "Login Successful!";

            // Save email to Realtime Database so Leaderboard doesn't skip them
            if (user != null)
            {
                FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(user.UserId).Child("email").SetValueAsync(user.Email);
            }

            if (loginButton != null) loginButton.interactable = true;
            loginPanel.SetActive(false);
            
            if (PlayerPrefs.GetInt("OnboardingComplete", 0) == 1) 
            {
                if (eulaPanel != null) eulaPanel.SetActive(false);
                if (bmiPanel != null) bmiPanel.SetActive(false);
                if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            }
            else 
            {
                if (eulaPanel != null) eulaPanel.SetActive(true);
            }
        });
    }

    private IEnumerator LockoutRoutine()
    {
        if (loginButton != null) loginButton.interactable = false; 
        
        for (int i = 30; i > 0; i--)
        {
            if(statusText != null) 
                statusText.text = $"You cannot log in for another {i}...";
            
            yield return new WaitForSeconds(1f); 
        }

        failedAttempts = 0; 
        if (loginButton != null) loginButton.interactable = true; 
        if(statusText != null) statusText.text = "Ready to log in.";
    }
}