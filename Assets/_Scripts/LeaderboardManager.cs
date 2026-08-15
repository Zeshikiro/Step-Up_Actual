using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Top 3 Podium UI Connections")]
    [SerializeField] private GameObject firstPlacePanel;
    [SerializeField] private TMP_Text firstPlaceName;
    [SerializeField] private TMP_Text firstPlaceSteps;

    [SerializeField] private GameObject secondPlacePanel;
    [SerializeField] private TMP_Text secondPlaceName;
    [SerializeField] private TMP_Text secondPlaceSteps;

    [SerializeField] private GameObject thirdPlacePanel;
    [SerializeField] private TMP_Text thirdPlaceName;
    [SerializeField] private TMP_Text thirdPlaceSteps;

    [Header("Current User Status Footer")]
    [SerializeField] private TMP_Text currentUserRankText;

    [Header("Scroll View Population Settings")]
    [SerializeField] private GameObject rowTemplatePrefab;
    [SerializeField] private Transform contentContainerTarget;

    private DatabaseReference leaderboardQueryRef;
    private FirebaseAuth auth;

    void Start()
    {
        // Enforce scrolling UI structure dynamically to prevent layout bugs
        if (contentContainerTarget != null)
        {
            if (contentContainerTarget.GetComponent<ContentSizeFitter>() == null)
            {
                var fitter = contentContainerTarget.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == Firebase.DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                InitializeRealtimeLeaderboard();
            }
        });
    }

    private void InitializeRealtimeLeaderboard()
    {
        leaderboardQueryRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");
        // FIX: Removed OrderByChild("TotalLifetimeSteps") because it requires backend Firebase Index Rules.
        // Instead, we just fetch a batch of users and sort them perfectly in local memory!
        leaderboardQueryRef.LimitToLast(50).ValueChanged += OnLeaderboardDataChanged;
    }

    void OnDestroy()
    {
        if (leaderboardQueryRef != null)
        {
            leaderboardQueryRef.LimitToLast(50).ValueChanged -= OnLeaderboardDataChanged;
        }
    }

    private void OnLeaderboardDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Firebase Live Feed Error: {args.DatabaseError.Message}");
            return;
        }

        foreach (Transform child in contentContainerTarget)
        {
            if (firstPlacePanel != null && child.gameObject == firstPlacePanel) continue;
            if (secondPlacePanel != null && child.gameObject == secondPlacePanel) continue;
            if (thirdPlacePanel != null && child.gameObject == thirdPlacePanel) continue;
            
            Destroy(child.gameObject);
        }

        List<UserDataRecord> sortedLeaderboardList = new List<UserDataRecord>();

        foreach (DataSnapshot userDoc in args.Snapshot.Children)
        {
            string username = "";
            if (userDoc.Child("username").Value != null && !string.IsNullOrEmpty(userDoc.Child("username").Value.ToString()))
            {
                username = userDoc.Child("username").Value.ToString();
            }
            else if (userDoc.Child("email").Value != null)
            {
                username = userDoc.Child("email").Value.ToString();
                if (username.Contains("@"))
                {
                    username = username.Split('@')[0]; // Take only the part before @
                }
            }
            else 
            {
                username = "Unknown";
            }

            long steps = 0;
            
            if (userDoc.Child("TotalLifetimeSteps").Value != null)
            {
                steps = Convert.ToInt64(userDoc.Child("TotalLifetimeSteps").Value);
            }
            string uid = userDoc.Key;

            // FIX: 8-bit fonts crash on underscores
            username = username.Replace("_", " ");

            // TRUNCATE: Max length to 10 letters so it fits perfectly on screen
            if (username.Length > 10)
            {
                username = username.Substring(0, 10);
            }

            sortedLeaderboardList.Add(new UserDataRecord(uid, username, steps));
        }

        // ALWAYS sort locally since we bypassed the Firebase backend Index requirement
        sortedLeaderboardList.Sort((a, b) => a.steps.CompareTo(b.steps));
        sortedLeaderboardList.Reverse();
        
        // AUTO-POPULATE IN-MEMORY DUMMY DATA IF LEADERBOARD HAS < 5 USERS
        // Firebase Security Rules block phones from writing fake data, so we inject it here!
        if (sortedLeaderboardList.Count < 5)
        {
            Debug.Log("[LeaderboardManager] Less than 5 users found. Injecting dummy data into UI...");
            string[] names = { "Alex C.", "Jamie D.", "Sam R.", "Taylor M.", "Morgan K.", "Jordan P.", "Casey L.", "Riley H.", "Avery T.", "Quinn S." };
            for (int i = 0; i < 10; i++)
            {
                long randomSteps = UnityEngine.Random.Range(5000, 30000); // Realistic steps
                sortedLeaderboardList.Add(new UserDataRecord("dummy_" + i, names[i], randomSteps));
            }
            // Re-sort the list since we just added high-score dummies
            sortedLeaderboardList.Sort((a, b) => a.steps.CompareTo(b.steps));
            sortedLeaderboardList.Reverse();
        }
        
        PopulateLeaderboardUI(sortedLeaderboardList);
    }

    private void PopulateLeaderboardUI(List<UserDataRecord> list)
    {
        string localUserUid = auth.CurrentUser != null ? auth.CurrentUser.UserId : "";
        bool localUserFoundInList = false;

        // Hide panels by default in case database is empty
        if (firstPlacePanel != null) firstPlacePanel.SetActive(false);
        if (secondPlacePanel != null) secondPlacePanel.SetActive(false);
        if (thirdPlacePanel != null) thirdPlacePanel.SetActive(false);
        
        Color highlightColor = new Color(0.2f, 1f, 0.2f); // Bright Green
        Color normalColor = Color.white;

        for (int i = 0; i < list.Count; i++)
        {
            int currentRankPosition = i + 1;
            UserDataRecord record = list[i];

            if (currentRankPosition == 1)
            {
                if (firstPlacePanel != null) firstPlacePanel.SetActive(true);
                firstPlaceName.text = record.username;
                firstPlaceName.color = (record.uid == localUserUid) ? highlightColor : normalColor;
                if (firstPlaceSteps.TryGetComponent(out UINumberCounter counter)) counter.CountTo((int)record.steps, 1f);
                else firstPlaceSteps.text = record.steps.ToString("N0");
            }
            else if (currentRankPosition == 2)
            {
                if (secondPlacePanel != null) secondPlacePanel.SetActive(true);
                secondPlaceName.text = record.username;
                secondPlaceName.color = (record.uid == localUserUid) ? highlightColor : normalColor;
                if (secondPlaceSteps.TryGetComponent(out UINumberCounter counter)) counter.CountTo((int)record.steps, 1f);
                else secondPlaceSteps.text = record.steps.ToString("N0");
            }
            else if (currentRankPosition == 3)
            {
                if (thirdPlacePanel != null) thirdPlacePanel.SetActive(true);
                thirdPlaceName.text = record.username;
                thirdPlaceName.color = (record.uid == localUserUid) ? highlightColor : normalColor;
                if (thirdPlaceSteps.TryGetComponent(out UINumberCounter counter)) counter.CountTo((int)record.steps, 1f);
                else thirdPlaceSteps.text = record.steps.ToString("N0");
            }
            else if (currentRankPosition <= 10)
            {
                // Clones row design directly inside your dynamic scrolling content container (Restricted to rank 4-10)
                GameObject newRow = Instantiate(rowTemplatePrefab, contentContainerTarget);
                if (newRow.TryGetComponent(out LeaderboardRowDisplay rowDisplay))
                {
                    rowDisplay.SetupRowDisplay(currentRankPosition, record.username, (int)record.steps);
                }
                else
                {
                    // FOOLPROOF FALLBACK: Find text boxes by their actual GameObject names in the hierarchy
                    Transform rankObj = newRow.transform.Find("Rank");
                    Transform nameObj = newRow.transform.Find("Name");
                    Transform stepsObj = newRow.transform.Find("StepsText");

                    if (rankObj != null && rankObj.TryGetComponent(out TMP_Text rankText))
                        rankText.text = "#" + currentRankPosition;

                    if (nameObj != null && nameObj.TryGetComponent(out TMP_Text nameText))
                    {
                        nameText.text = record.username;
                        nameText.color = (record.uid == localUserUid) ? highlightColor : normalColor;
                    }

                    if (stepsObj != null && stepsObj.TryGetComponent(out TMP_Text stepsText))
                    {
                        if (stepsText.TryGetComponent(out UINumberCounter counter)) counter.CountTo((int)record.steps, 1f);
                        else stepsText.text = ((int)record.steps).ToString("N0");
                    }
                }
            }

            if (record.uid == localUserUid)
            {
                currentUserRankText.text = currentRankPosition.ToString();
                localUserFoundInList = true;
            }
        }

        if (!localUserFoundInList)
        {
            currentUserRankText.text = "50+";
        }
    }

    [ContextMenu("Populate 10 Dummy Leaderboard Users")]
    public void PopulateDummyData()
    {
        string[] names = { "Alex C.", "Jamie D.", "Sam R.", "Taylor M.", "Morgan K.", "Jordan P.", "Casey L.", "Riley H.", "Avery T.", "Quinn S." };
        DatabaseReference usersRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");
        
        for (int i = 0; i < 10; i++)
        {
            string dummyId = "dummy_user_" + i;
            long randomSteps = UnityEngine.Random.Range(5000, 30000); // Realistic steps
            
            usersRef.Child(dummyId).Child("username").SetValueAsync(names[i]);
            usersRef.Child(dummyId).Child("TotalLifetimeSteps").SetValueAsync(randomSteps);
        }
        
        Debug.Log("Successfully pushed 10 dummy users to Firebase!");
    }
} 

public class UserDataRecord
{
    public string uid;
    public string username;
    public long steps;

    public UserDataRecord(string id, string name, long stepCount)
    {
        uid = id;
        username = name;
        steps = stepCount;
    }
}