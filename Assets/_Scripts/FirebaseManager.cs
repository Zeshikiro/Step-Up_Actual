using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    DatabaseReference dbReference;
    [Header("Leaderboard UI")]
    public GameObject playerRowPrefab; // PlayerRow Blueprint
    public GameObject leaderboardPanel; // The whole panel we want to hide/show
    public Transform leaderboardContent; // The "Content" folder inside the Scroll View 
void Start()
{
    FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available) {
            
            // 1. The Power: Initialize the database
            FirebaseDatabase dbInstance = FirebaseDatabase.GetInstance("https://step-up-72811-default-rtdb.firebaseio.com/");
            dbReference = dbInstance.RootReference;
            dbInstance.SetPersistenceEnabled(true);
            
            Debug.Log("Firebase Initialized!");

            LoadLeaderboard();

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
    public void LoadLeaderboard()
{
    // Ask Firebase to order by steps and grab the highest 10
    dbReference.Child("users").OrderByChild("totalSteps").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(task => {
        if (task.IsFaulted) {
            Debug.LogError("Failed to get leaderboard: " + task.Exception);
            return;
        }

        DataSnapshot snapshot = task.Result;

        // Destroy any old UI rows before loading new ones
        foreach (Transform child in leaderboardContent) {
            Destroy(child.gameObject);
        }

        // Loop through the top players we got back
        foreach (DataSnapshot childSnapshot in snapshot.Children) {
            
            // THE FIX: Skip any broken users (like test_user_001) that don't have an email!
            if (!childSnapshot.HasChild("email")) 
            {
                Debug.LogWarning("Skipping a user because they have no email setup.");
                continue; 
            }

            // Extract the data safely
            string userEmail = childSnapshot.Child("email").Value.ToString();
            string userSteps = childSnapshot.Child("totalSteps").Value.ToString();

            // Spawn a new UI Row
            GameObject newRow = Instantiate(playerRowPrefab, leaderboardContent);
            
            // Find the text boxes inside the new row and fill them
            TextMeshProUGUI[] texts = newRow.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = userEmail; 
            texts[1].text = userSteps; 

            // BONUS FIX: Firebase sorts lowest-to-highest. 
            // This forces the highest step counts to spawn at the TOP of your UI list!
            newRow.transform.SetAsFirstSibling();
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

        // 1. Ask the Passport Office who is currently playing
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser != null)
        {
            // 2. Grab their unique, encrypted ID and their Email
            string userId = currentUser.UserId; 
            string userEmail = currentUser.Email;

            // 3. Save the steps to THEIR specific folder, not test_user_001
        dbReference.Child("users").Child(userId).Child("totalSteps").SetValueAsync(newSteps).ContinueWithOnMainThread(task => {
            // We added this check to catch silent rejections!
        if (task.IsFaulted) {
            Debug.LogError("Cloud rejected the save! Reason: " + task.Exception.GetBaseException().Message);
        } 
        else if (task.IsCompleted) {
            Debug.Log($"Steps synced to cloud for {userEmail}: {newSteps}");
        }
        });

            // 4. Also save their email so we can display it on the Leaderboard later!
            dbReference.Child("users").Child(userId).Child("email").SetValueAsync(userEmail);
        }
        else
        {
            Debug.LogError("Wait! No one is logged in. Cannot save steps.");
        }
    }
    // --- APP NAVIGATION ---

    public void ToggleLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            // This flips the switch: If it's on, turn it off. If it's off, turn it on.
            bool isCurrentlyOpen = leaderboardPanel.activeSelf;
            leaderboardPanel.SetActive(!isCurrentlyOpen);

            // If we just opened it, tell Firebase to download the freshest data!
            if (!isCurrentlyOpen) 
            {
                LoadLeaderboard();
            }
        }
    }

    public void LogoutUser()
    {
        // 1. Tell the Passport Office to destroy the current session
        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("User logged out successfully.");

        // 2. Teleport the user back to the Login Screen
        SceneManager.LoadScene("LoginScene");
    }
}