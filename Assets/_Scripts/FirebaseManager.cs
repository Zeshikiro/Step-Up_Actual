using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseManager : MonoBehaviour
{
    DatabaseReference dbReference;

void Start()
{
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
            
            // 1. The Power: Initialize the database
            FirebaseDatabase dbInstance = FirebaseDatabase.GetInstance("https://step-up-your-url.firebaseio.com/");
            dbReference = dbInstance.RootReference;
            dbInstance.SetPersistenceEnabled(true);
            
            Debug.Log("Firebase Initialized!");

            // ---------------------------------------------------------
            // 2. THE CAMERA: Paste the Listener code right here!
            // ---------------------------------------------------------
            dbReference.Child("users").Child("test_user_001").Child("totalSteps")
                .ValueChanged += (object sender, ValueChangedEventArgs args) => {
                    if (args.DatabaseError != null) {
                        Debug.LogError(args.DatabaseError.Message);
                        return;
                    }
                    
                    // This will fire every time you (or anyone else) changes the data in the browser
                    Debug.Log("Cloud data changed! New total from Firebase: " + args.Snapshot.Value);
                };
            // ---------------------------------------------------------

        }
    });
}

    public void UpdateStepsInCloud(int newSteps)
    {
        if (dbReference == null) 
        {
            Debug.LogWarning("Cannot sync: Firebase not initialized yet!");
            return;
        }

        string userId = "test_user_001"; 
        
        // Sends the step count to users/test_user_001/totalSteps
        dbReference.Child("users").Child(userId).Child("totalSteps").SetValueAsync(newSteps)
            .ContinueWithOnMainThread(task => {
                if (task.IsCompleted) {
                    Debug.Log($"Steps synced to cloud: {newSteps}");
                }
            });
    }
}