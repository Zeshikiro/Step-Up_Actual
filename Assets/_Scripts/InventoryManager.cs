using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("InventoryManager");
                    if (prefab != null)
                    {
                        GameObject obj = Instantiate(prefab);
                        _instance = obj.GetComponent<InventoryManager>();
                        Debug.LogWarning("[InventoryManager] Auto-spawned missing Singleton from Resources!");
                    }
                    else
                    {
                        Debug.LogError("[InventoryManager] Failed to auto-spawn! Prefab not found in Resources/ folder!");
                    }
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemId;               // e.g., "Casual_Head" or "Adventurer_Torso"
        public string associatedOutfitName; // e.g., "Casual" or "Adventurer"
        public string category;             // Must be exactly: "Head", "Torso", "Legs", "Feet", or "Accessory"
        public GameObject itemMeshPrefab;   // The specific 3D model piece to equip
        public Sprite itemIcon;             // UI icon for this individual piece (Your NOBG Sprites!)
        public int unlockCost = 500;        // Cost in coins to buy this item
        public int levelRequirement = 1;    // Level required to equip this item
    }

    [Header("Master Inventory Database")]
    [Tooltip("List every single individual sliced item piece available in the game here")]
    public List<InventoryItemData> masterInventoryList = new List<InventoryItemData>();

    [Header("UI Spawning Setup")]
    public GameObject inventoryButtonPrefab; // Your UI button prefab template

    [Header("Sub-Panel Content Containers")]
    public Transform headContentGrid;
    public Transform torsoContentGrid;
    public Transform legsContentGrid;
    public Transform feetContentGrid;
    public Transform accessoryContentGrid;

    [Header("Player Wallet")]
    public int coins = 0; // Starting coins balance

    [Header("Saved Look (Item IDs)")]
    // Gender override removed to allow dynamic mixing!
    public string equippedHeadId = "MCasual2_Head";
    public string equippedBodyId = "MCasual2_Body";
    public string equippedLegsId = "MCasual2_Legs";
    public string equippedFeetId = "MCasual2_Feet";
    public string equippedAccessoryId = "None";

    // Global event that fires whenever the player equips a new item
    public static event Action OnAvatarEquipmentsChanged;

    // Track unlocked item IDs
    private HashSet<string> unlockedItems = new HashSet<string>();

    // Keep track of active buttons in the scene so we can refresh text labels instantly
    private List<InventoryItemButton> spawnedButtons = new List<InventoryItemButton>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadUnlockedItems();
        LoadCoins();

        // Pre-unlock the default casual items so they are always in the inventory
        UnlockFullOutfitBundle("MCasual2");
        UnlockFullOutfitBundle("FCasual");
    }

    private void Start()
    {
        if (Instance != this) return; // 🚨 CRITICAL ANTI-CRASH FIX: Prevent dying clones from running logic!

        // Safely wait for Firebase to be ready before loading the cloud save
        // NEW: Load the avatar instantly from local storage before waiting for Firebase!
        LoadAvatarLocal();

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                LoadAvatarFromCloud();
            }
        });
    }

    public void SaveAvatarToCloud()
    {
        // 1. Save locally for instant offline loading
        PlayerPrefs.SetString("equippedHeadId", equippedHeadId);
        PlayerPrefs.SetString("equippedBodyId", equippedBodyId);
        PlayerPrefs.SetString("equippedLegsId", equippedLegsId);
        PlayerPrefs.SetString("equippedFeetId", equippedFeetId);
        PlayerPrefs.SetString("equippedAccessoryId", equippedAccessoryId);
        PlayerPrefs.Save();

        // 2. Save to Firebase
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth == null || auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // SYNC INVENTORY DATA (Coins and Unlocked Items)
        string[] array = new string[unlockedItems.Count];
        unlockedItems.CopyTo(array);
        string joinedUnlocked = string.Join(",", array);

        Dictionary<string, object> avatarData = new Dictionary<string, object>
        {
            {"equippedHeadId", equippedHeadId},
            {"equippedBodyId", equippedBodyId},
            {"equippedLegsId", equippedLegsId},
            {"equippedFeetId", equippedFeetId},
            {"equippedAccessoryId", equippedAccessoryId},
            {"coins", coins},
            {"unlockedItems", joinedUnlocked}
        };

        dbRef.Child("users").Child(uid).Child("avatar").SetValueAsync(avatarData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted) Debug.Log("Avatar cloud save successful!");
        });
    }

    public void LoadAvatarLocal()
    {
        // NEW: Load coins locally instantly so they don't default to 0 before the cloud syncs!
        LoadCoins();

        if (PlayerPrefs.HasKey("equippedHeadId"))
        {
            equippedHeadId = PlayerPrefs.GetString("equippedHeadId", "MCasual2_Head");
            equippedBodyId = PlayerPrefs.GetString("equippedBodyId", "MCasual2_Body");
            equippedLegsId = PlayerPrefs.GetString("equippedLegsId", "MCasual2_Legs");
            equippedFeetId = PlayerPrefs.GetString("equippedFeetId", "MCasual2_Feet");
            equippedAccessoryId = PlayerPrefs.GetString("equippedAccessoryId", "None");
            
            Debug.Log("[InventoryManager] Avatar loaded locally from PlayerPrefs instantly!");
            OnAvatarEquipmentsChanged?.Invoke();
        }
    }

    public void LoadAvatarFromCloud()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth == null || auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("users").Child(uid).Child("avatar").GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted) return;
            DataSnapshot snapshot = task.Result;

            if (snapshot.Exists)
            {
                if (snapshot.Child("equippedHeadId").Value != null)
                    equippedHeadId = snapshot.Child("equippedHeadId").Value.ToString();
                if (snapshot.Child("equippedBodyId").Value != null)
                    equippedBodyId = snapshot.Child("equippedBodyId").Value.ToString();
                if (snapshot.Child("equippedLegsId").Value != null)
                    equippedLegsId = snapshot.Child("equippedLegsId").Value.ToString();
                if (snapshot.Child("equippedFeetId").Value != null)
                    equippedFeetId = snapshot.Child("equippedFeetId").Value.ToString();
                if (snapshot.Child("equippedAccessoryId").Value != null)
                    equippedAccessoryId = snapshot.Child("equippedAccessoryId").Value.ToString();

                // SAVE TO PLAYER PREFS SO IT PERSISTS OFFLINE ON NEW DEVICES!
                PlayerPrefs.SetString("equippedHeadId", equippedHeadId);
                PlayerPrefs.SetString("equippedBodyId", equippedBodyId);
                PlayerPrefs.SetString("equippedLegsId", equippedLegsId);
                PlayerPrefs.SetString("equippedFeetId", equippedFeetId);
                PlayerPrefs.SetString("equippedAccessoryId", equippedAccessoryId);
                PlayerPrefs.Save();

                // RESTORE COINS
                if (snapshot.Child("coins").Value != null)
                {
                    int cloudCoins = 0;
                    int.TryParse(snapshot.Child("coins").Value.ToString(), out cloudCoins);
                    
#if UNITY_EDITOR
                    if (overrideCoins) cloudCoins = debugCoinAmount;
#endif

                    if (cloudCoins > coins)
                    {
                        coins = cloudCoins;
                        SaveCoins();
                    }
                    
#if UNITY_EDITOR
                    // If the cheat is strictly lower than cloud (e.g. testing poverty), force it
                    if (overrideCoins) coins = debugCoinAmount;
#endif
                }

                // RESTORE UNLOCKED ITEMS
                if (snapshot.Child("unlockedItems").Value != null)
                {
                    string cloudItems = snapshot.Child("unlockedItems").Value.ToString();
                    string[] items = cloudItems.Split(',');
                    bool changed = false;
                    foreach (string item in items)
                    {
                        if (!string.IsNullOrEmpty(item) && !unlockedItems.Contains(item.Trim()))
                        {
                            unlockedItems.Add(item.Trim());
                            changed = true;
                        }
                    }
                    if (changed)
                    {
                        SaveUnlockedItems();
                    }
                }

                Debug.Log("Avatar cloud save loaded successfully!");
                
                // NEW: Force the AvatarLoader to rebuild the character immediately after we download their saved look!
                // This fixes the bug where the avatar is invisible at launch because it was waiting for Firebase.
                OnAvatarEquipmentsChanged?.Invoke();
            }
        });
    }

    // [Deprecated] SwapGenderPrefixes and TranslateID removed.
    // The game no longer attempts to auto-guess or force prefixes for Male/Female.

    public bool IsItemUnlocked(string itemId)
    {
        return unlockedItems.Contains(itemId);
    }

    // We now return the raw itemId directly so that the M/F prefixes are preserved 
    // when searching the Avatar's Hierarchy!
    public string GetMeshNameFromItemId(string itemId)
    {
        if (string.IsNullOrEmpty(itemId) || itemId == "None") return "None";

        // LEGACY SAVE DATA PATCH (if an old player had 'Casual_Head' saved without a gender prefix, default to Male to prevent missing meshes)
        if (itemId.StartsWith("Casual_"))
        {
            return itemId.Replace("Casual_", "MCasual2_");
        }

        // We now blindly trust the exact ID given to us by the Master Inventory List!
        return itemId; 
    }

    public void UnlockItem(string itemId)
    {
        if (!unlockedItems.Contains(itemId))
        {
            unlockedItems.Add(itemId);
            SaveUnlockedItems();
        }
    }

    public void SaveUnlockedItems()
    {
        string[] array = new string[unlockedItems.Count];
        unlockedItems.CopyTo(array);
        string joined = string.Join(",", array);
        PlayerPrefs.SetString("UnlockedItems", joined);
        PlayerPrefs.Save();

        // Push update to cloud!
        SaveAvatarToCloud();
    }

    public void LoadUnlockedItems()
    {
        if (PlayerPrefs.HasKey("UnlockedItems"))
        {
            string joined = PlayerPrefs.GetString("UnlockedItems");
            string[] items = joined.Split(',');
            foreach (string item in items)
            {
                if (!string.IsNullOrEmpty(item)) unlockedItems.Add(item.Trim());
            }
        }
    }

    public void UnlockFullOutfitBundle(string outfitName)
    {
        UnlockItem(outfitName); 
        
        // Strip out the underscore and gender prefixes to get the raw core name
        // Example: "M_Adventurer" -> "Adventurer", "F_Punk" -> "Punk", "Suit" -> "Suit"
        string coreName = outfitName.Replace("_", "");
        if (coreName.StartsWith("M")) coreName = coreName.Substring(1);
        else if (coreName.StartsWith("F")) coreName = coreName.Substring(1);

        // Unlock EVERY POSSIBLE VARIATION to guarantee the Wardrobe finds it!
        string[] prefixes = new string[] { "M", "F", "M_", "F_", "" };
        string[] suffixes = new string[] { "", "_Head", "_Torso", "_Body", "_Legs", "_Feet", "_Accessory" };

        foreach (string p in prefixes)
        {
            foreach (string s in suffixes)
            {
                UnlockItem(p + coreName + s);
            }
        }
    }

    // Helper to calculate the player's current level using the same logic as ProfileManager
    public int GetCurrentLevel()
    {
        int missionXPEarned = PlayerPrefs.GetInt("MissionXPEarned", 0);
        int totalXP = missionXPEarned;
        return (totalXP / 5000) + 1; // Level up every 5000 XP
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveCoins();
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCoins();
    }

    [Header("Debug / Cheats")]
    [Tooltip("Check this to override the player's saved coins with the amount below")]
    public bool overrideCoins;
    [Tooltip("The amount of coins to give the player for testing")]
    public int debugCoinAmount;

    public void SaveCoins()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.Save();
        // Force refresh UI so the coin counter instantly updates if we are in CustomizeScene
        RefreshButtonLabels();
        
        // Push update to cloud!
        SaveAvatarToCloud();
    }

    public void LoadCoins()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0); // Defaults to 0 for new players

#if UNITY_EDITOR
        if (overrideCoins)
        {
            coins = debugCoinAmount;
            PlayerPrefs.SetInt("PlayerCoins", coins);
            PlayerPrefs.Save();
        }
#endif
    }

    // 🔄 CALL THIS ONCE WHEN THE STUDENT OPENS THEIR WARDROBE/CUSTOMIZATION PAGE
    public void GenerateInventoryUI()
    {
        // 1. Clear out old layout entries and tracked button references
        ClearGrid(headContentGrid);
        ClearGrid(torsoContentGrid);
        ClearGrid(legsContentGrid);
        ClearGrid(feetContentGrid);
        ClearGrid(accessoryContentGrid);
        spawnedButtons.Clear();

        int playerLevel = GetCurrentLevel(); // Fetch player level once before loop

        // 2. Loop through our master item slice database
        foreach (InventoryItemData item in masterInventoryList)
        {
            if (item == null) continue;

            // Only spawn the item if the player has actually unlocked/purchased it!
            if (IsItemUnlocked(item.itemId) || IsItemUnlocked(item.associatedOutfitName))
            {
                Transform targetGrid = GetTargetGrid(item.category);
                if (targetGrid != null)
                {
                    // Spawn the inventory item slot button inside the correct tab view container
                    GameObject newButton = Instantiate(inventoryButtonPrefab, targetGrid);
                    
                    // Route values into the spawned UI button component
                    InventoryItemButton buttonScript = newButton.GetComponent<InventoryItemButton>();
                    if (buttonScript != null)
                    {
                        int actualCost = GetDynamicCostForOutfit(item.associatedOutfitName);

                        // 👗 Directs the exact database ID, categories, prefabs, and 2D textures safely
                        buttonScript.SetupButtonDetails(item.itemId, item.category, item.itemMeshPrefab, item.itemIcon, actualCost, item.levelRequirement, playerLevel);
                        
                        // 🏷️ Overwrite the display label text using the clean, pretty outfit name
                        if (buttonScript.txtOutfitName != null)
                        {
                            string displayName = item.associatedOutfitName;
                            if (displayName.StartsWith("M_") || displayName.StartsWith("F_"))
                            {
                                displayName = displayName.Substring(2);
                            }
                            buttonScript.txtOutfitName.text = displayName;
                        }

                        // Track this button script so we can update its labels dynamically later
                        spawnedButtons.Add(buttonScript);
                    }
                }
            }
        }

        // 🎯 Initial text synchronization pass once generation loops complete
        RefreshButtonLabels();

        // 🚀 Add a "Coming Soon!" placeholder to the Accessory Tab if it's empty
        if (accessoryContentGrid != null && accessoryContentGrid.childCount == 0 && inventoryButtonPrefab != null)
        {
            GameObject newButton = Instantiate(inventoryButtonPrefab, accessoryContentGrid);
            InventoryItemButton buttonScript = newButton.GetComponent<InventoryItemButton>();
            if (buttonScript != null)
            {
                // Give it impossible reqLevel so they can't equip it, or just let it fail gracefully
                buttonScript.SetupButtonDetails("Coming Soon!", "Accessory", null, null, 0, 999, playerLevel);
                if (buttonScript.txtOutfitName != null) buttonScript.txtOutfitName.text = "COMING SOON";
                if (buttonScript.txtEquipStatus != null) buttonScript.txtEquipStatus.text = "WIP";
            }
        }
    }

    // ⚡ NEW PLAY: The structural execution engine for equipping clothing assets
    public void EquipItem(string itemId, string category)
    {
        // Update our active saved appearance IDs based strictly on item slot classification
        if (string.IsNullOrEmpty(category)) return;

        switch (category.ToLower().Trim())
        {
            case "head":
                equippedHeadId = itemId;
                break;
            case "torso":
            case "body":
                equippedBodyId = itemId;
                break;
            case "legs":
                equippedLegsId = itemId;
                break;
            case "feet":
                equippedFeetId = itemId;
                break;
            case "accessory":
                equippedAccessoryId = itemId;
                break;
        }

        Debug.Log($"[Wardrobe Engine] Successfully equipped {itemId} into the {category} slot.");

        // 💡 Instantly update button action text labels without expensive redrawing halts
        RefreshButtonLabels();
        
        // 💾 Save changes to the cloud automatically!
        SaveAvatarToCloud();

        // 🚀 Trigger ALL AvatarLoaders globally to instantly refresh meshes!
        OnAvatarEquipmentsChanged?.Invoke();
    }

    // 🎨 NEW PLAY: Smart text state refresher loop
    public void RefreshButtonLabels()
    {
        // CRITICAL FIX: Clean out any lingering destroyed buttons from previous scene visits!
        spawnedButtons.RemoveAll(b => b == null);

        foreach (InventoryItemButton button in spawnedButtons)
        {
            if (button.txtEquipStatus == null) continue;

            // Delegate UI updates to the button itself so it handles BUY vs EQUIP logic!
            button.RefreshVisibility();
        }
    }

    private int GetDynamicCostForOutfit(string outfitName)
    {
        if (string.IsNullOrEmpty(outfitName)) return 500;
        
        string cleanName = outfitName.ToUpper();
        
        // Tier 1: Basic Outfits (200 coins)
        if (cleanName.Contains("CASUAL") || cleanName.Contains("WORKER")) return 200;
        
        // Tier 2: Mid Outfits (500 coins)
        if (cleanName.Contains("FARMER") || cleanName.Contains("BEACH") || cleanName.Contains("PUNK") || cleanName.Contains("ADVENTURER")) return 500;
        
        // Tier 3: High Outfits (1000 coins)
        if (cleanName.Contains("SWAT") || cleanName.Contains("SOLDIER") || cleanName.Contains("SUIT") || cleanName.Contains("FORMAL") || cleanName.Contains("MEDIEVAL")) return 1000;
        
        // Tier 4: Epic Outfits (2000 coins)
        if (cleanName.Contains("KING") || cleanName.Contains("SPACESUIT") || cleanName.Contains("SCIFI") || cleanName.Contains("WITCH")) return 2000;
        
        return 500; // default
    }

    public Color GetRarityColor(int price)
    {
        // Using soft pastel colors so the dark text and icons remain visible
        if (price >= 2000) return new Color(1.0f, 0.9f, 0.6f); // Epic: Soft Gold
        if (price >= 1000) return new Color(0.8f, 0.6f, 1.0f); // Elite: Soft Purple
        if (price >= 500) return new Color(0.6f, 0.8f, 1.0f);  // Standard: Soft Blue
        return new Color(0.9f, 0.9f, 0.9f);                    // Basic: Light Gray
    }

    private Transform GetTargetGrid(string category)
    {
        if (string.IsNullOrEmpty(category)) return null;

        switch (category.ToLower().Trim())
        {
            case "head": return headContentGrid;
            case "torso": 
            case "body": return torsoContentGrid;
            case "legs": return legsContentGrid;
            case "feet": return feetContentGrid;
            case "accessory": return accessoryContentGrid;
            default: return null;
        }
    }

    private void ClearGrid(Transform grid)
    {
        if (grid == null) return;
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }
    }
}