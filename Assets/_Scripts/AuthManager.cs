using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions; 
using TMPro;
using UnityEngine.SceneManagement; // This allows us to switch scenes!

public class AuthManager : MonoBehaviour
{
    public TMP_InputField emailField;
    public TMP_InputField passwordField;
    private FirebaseAuth auth;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase Auth is ready!");
            } else {
                Debug.LogError("Could not resolve Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    public void RegisterUser()
    {
        if (auth == null) { Debug.LogError("Auth not ready yet!"); return; }

        auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) {
                Debug.LogError("Registration failed: " + task.Exception.Flatten().InnerException.Message);
                return;
            }
            Debug.Log("Student Registered: " + task.Result.User.Email);
            
            // THE BRIDGE: Go to the Map Scene!
            SceneManager.LoadScene("SampleScene"); 
        });
    }

    public void LoginUser()
    {
        if (auth == null) { Debug.LogError("Auth not ready yet!"); return; }

        auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(task => {
            if (task.IsFaulted) {
                Debug.LogError("Login failed: " + task.Exception.Flatten().InnerException.Message);
                return;
            }
            Debug.Log("Login Successful!");
            
            // THE BRIDGE: Go to the Map Scene!
            SceneManager.LoadScene("SampleScene");
        });
    }
}