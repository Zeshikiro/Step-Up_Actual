using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

public class LeaderboardManager : MonoBehaviour
{
    [Header("Top 3 Podium UI Connections")]
    [SerializeField] private TMP_Text firstPlaceName;
    [SerializeField] private TMP_Text firstPlaceSteps;
    [SerializeField] private Image firstPlaceAvatar;

    [SerializeField] private TMP_Text secondPlaceName;
    [SerializeField] private TMP_Text secondPlaceSteps;
    [SerializeField] private Image secondPlaceAvatar;

    [SerializeField] private TMP_Text thirdPlaceName;
    [SerializeField] private TMP_Text thirdPlaceSteps;
    [SerializeField] private Image thirdPlaceAvatar;

    [Header("Scroll View List Configurations")]
    [SerializeField] private Transform scrollContentContainer;
    [SerializeField] private GameObject rowPrefab;

    [Header("Current User Status Footer")]
    [SerializeField] private TMP_Text currentUserRankText;

    private DatabaseReference leaderboardQueryRef;
    private FirebaseAuth auth;
    
    // CORE FIX: Swapped from Sprite to Texture2D caching to prevent white blocks on panel re-open
    private static Dictionary<string, Texture2D> avatarTextureCache = new Dictionary<string, Texture2D>();
    private List<IEnumerator> activeDownloadRoutines = new List<IEnumerator>();

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        InitializeRealtimeLeaderboard();
    }

    private void InitializeRealtimeLeaderboard()
    {
        leaderboardQueryRef = FirebaseDatabase.DefaultInstance.RootReference.Child("users");
        leaderboardQueryRef.OrderByChild("TotalLifetimeSteps").LimitToLast(50).ValueChanged += OnLeaderboardDataChanged;
    }

    void OnDestroy()
    {
        if (leaderboardQueryRef != null)
        {
            leaderboardQueryRef.OrderByChild("TotalLifetimeSteps").LimitToLast(50).ValueChanged -= OnLeaderboardDataChanged;
        }
        StopAllActiveDownloads();
    }

    private void OnLeaderboardDataChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"Firebase Live Feed Error: {args.DatabaseError.Message}");
            return;
        }

        foreach (Transform child in scrollContentContainer)
        {
            Destroy(child.gameObject);
        }

        List<UserDataRecord> sortedLeaderboardList = new List<UserDataRecord>();

        foreach (DataSnapshot userDoc in args.Snapshot.Children)
        {
            string username = userDoc.Child("username").Value?.ToString() ?? "Unknown";
            string avatarUrl = userDoc.Child("profileImageUrl").Value?.ToString() ?? ""; 
            long steps = 0;
            
            if (userDoc.Child("TotalLifetimeSteps").Value != null)
            {
                steps = Convert.ToInt64(userDoc.Child("TotalLifetimeSteps").Value);
            }
            string uid = userDoc.Key;

            sortedLeaderboardList.Add(new UserDataRecord(uid, username, steps, avatarUrl));
        }

        sortedLeaderboardList.Reverse();
        PopulateLeaderboardUI(sortedLeaderboardList);
    }

    private void PopulateLeaderboardUI(List<UserDataRecord> list)
    {
        StopAllActiveDownloads();
        string localUserUid = auth.CurrentUser != null ? auth.CurrentUser.UserId : "";
        bool localUserFoundInList = false;

        for (int i = 0; i < list.Count; i++)
        {
            int currentRankPosition = i + 1;
            UserDataRecord record = list[i];

            if (currentRankPosition == 1)
            {
                firstPlaceName.text = record.username;
                firstPlaceSteps.text = record.steps.ToString("N0") + " Steps";
                AssignAvatarSafe(record.avatarUrl, firstPlaceAvatar);
            }
            else if (currentRankPosition == 2)
            {
                secondPlaceName.text = record.username;
                secondPlaceSteps.text = record.steps.ToString("N0") + " Steps";
                AssignAvatarSafe(record.avatarUrl, secondPlaceAvatar);
            }
            else if (currentRankPosition == 3)
            {
                thirdPlaceName.text = record.username;
                thirdPlaceSteps.text = record.steps.ToString("N0") + " Steps";
                AssignAvatarSafe(record.avatarUrl, thirdPlaceAvatar);
            }
            else
            {
                GameObject newRow = Instantiate(rowPrefab, scrollContentContainer);
                TMP_Text[] rowTexts = newRow.GetComponentsInChildren<TMP_Text>();
                if (rowTexts.Length >= 3)
                {
                    rowTexts[0].text = "Rank " + currentRankPosition;
                    rowTexts[1].text = record.username;
                    rowTexts[2].text = record.steps.ToString("N0");
                }
            }

            if (record.uid == localUserUid)
            {
                currentUserRankText.text = "Current place: " + GetRankOrdinal(currentRankPosition);
                localUserFoundInList = true;
            }
        }

        if (!localUserFoundInList)
        {
            currentUserRankText.text = "Current place: 50+";
        }
    }

    private void AssignAvatarSafe(string url, Image targetImage)
    {
        if (string.IsNullOrEmpty(url) || targetImage == null) return;

        // CORE FIX: Check if the raw texture asset is safely cached in long-term background memory
        if (avatarTextureCache.ContainsKey(url) && avatarTextureCache[url] != null)
        {
            Texture2D cachedTex = avatarTextureCache[url];
            Sprite freshSprite = Sprite.Create(cachedTex, new Rect(0, 0, cachedTex.width, cachedTex.height), new Vector2(0.5f, 0.5f));
            targetImage.sprite = freshSprite;
        }
        else
        {
            IEnumerator downloadWorker = DownloadAndCacheAvatar(url, targetImage);
            activeDownloadRoutines.Add(downloadWorker);
            StartCoroutine(downloadWorker);
        }
    }

    private IEnumerator DownloadAndCacheAvatar(string url, Image targetImage)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                
                // CORE FIX: Cache the raw texture container instead of the volatile UI component reference
                if (!avatarTextureCache.ContainsKey(url))
                {
                    avatarTextureCache.Add(url, texture);
                }
                else
                {
                    avatarTextureCache[url] = texture;
                }

                if (targetImage != null)
                {
                    Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    targetImage.sprite = newSprite;
                }
            }
        }
    }

    private void StopAllActiveDownloads()
    {
        foreach (var routine in activeDownloadRoutines)
        {
            if (routine != null) StopCoroutine(routine);
        }
        activeDownloadRoutines.Clear();
    }

    private string GetRankOrdinal(int rank)
    {
        if (rank <= 0) return rank.ToString();
        switch (rank % 100)
        {
            case 11: case 12: case 13: return rank + "th";
        }
        switch (rank % 10)
        {
            case 1: return rank + "st";
            case 2: return rank + "nd";
            case 3: return rank + "rd";
            default: return rank + "th";
        }
    }
} 

public class UserDataRecord
{
    public string uid;
    public string username;
    public long steps;
    public string avatarUrl;

    public UserDataRecord(string id, string name, long stepCount, string url)
    {
        uid = id;
        username = name;
        steps = stepCount;
        avatarUrl = url;
    }
}