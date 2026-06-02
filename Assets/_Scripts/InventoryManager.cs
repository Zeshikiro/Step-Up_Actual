using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemId;               // e.g., "Casual_Head" or "Adventurer_Torso"
        public string associatedOutfitName; // e.g., "Casual" or "Adventurer"
        public string category;             // Must be exactly: "Head", "Torso", "Legs", "Feet", or "Accessory"
        public GameObject itemMeshPrefab;   // The specific 3D model piece to equip
        public Sprite itemIcon;             // UI icon for this individual piece (Your NOBG Sprites!)
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
    public int coins = 1500; // Starting coins balance

    [Header("Saved Look (Item IDs)")]
    public bool isMaleAvatar = true; // Tracks explicit gender choice globally
    public string equippedHeadId = "Casual_Head";
    public string equippedBodyId = "Casual_Body";
    public string equippedLegsId = "Casual_Legs";
    public string equippedFeetId = "Casual_Feet";
    public string equippedAccessoryId = "None";

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

        // Pre-unlock the default casual items so they are always in the inventory
        UnlockItem("Casual_Head");
        UnlockItem("Casual_Body");
        UnlockItem("Casual_Torso"); 
        UnlockItem("Casual_Legs");
        UnlockItem("Casual_Feet");

        // Also unlock the prefixed versions based on the new naming convention
        UnlockFullOutfitBundle("M_Casual");
        UnlockFullOutfitBundle("F_Casual");
    }

    private void Start()
    {
        // Safely wait for Firebase to be ready before loading the cloud save
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                LoadAvatarFromCloud();
            }
        });
    }

    public void SaveAvatarToCloud()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        if (auth == null || auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;
        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        Dictionary<string, object> avatarData = new Dictionary<string, object>
        {
            {"isMaleAvatar", isMaleAvatar},
            {"equippedHeadId", equippedHeadId},
            {"equippedBodyId", equippedBodyId},
            {"equippedLegsId", equippedLegsId},
            {"equippedFeetId", equippedFeetId},
            {"equippedAccessoryId", equippedAccessoryId}
        };

        dbRef.Child("users").Child(uid).Child("avatar").SetValueAsync(avatarData).ContinueWithOnMainThread(task => {
            if (task.IsCompleted) Debug.Log("Avatar cloud save successful!");
        });
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
                if (snapshot.Child("isMaleAvatar").Value != null)
                    isMaleAvatar = (bool)snapshot.Child("isMaleAvatar").Value;
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

                Debug.Log("Avatar cloud save loaded successfully!");
            }
        });
    }

    public bool IsItemUnlocked(string itemId)
    {
        return unlockedItems.Contains(itemId);
    }

    public void UnlockItem(string itemId)
    {
        if (!unlockedItems.Contains(itemId))
        {
            unlockedItems.Add(itemId);
        }
    }

    public void UnlockFullOutfitBundle(string outfitName)
    {
        UnlockItem(outfitName); 
        
        // Strip out the underscore so "M_Adventurer" becomes "MAdventurer" 
        // to match the Item Id the user typed in the Inspector!
        string strippedName = outfitName.Replace("_", "");

        UnlockItem(strippedName + "_Head");
        UnlockItem(strippedName + "_Torso");
        UnlockItem(strippedName + "_Body"); // Unlock both Torso and Body just in case!
        UnlockItem(strippedName + "_Legs");
        UnlockItem(strippedName + "_Feet");
        UnlockItem(strippedName + "_Accessory");
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            return true;
        }
        return false;
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

        // 2. Loop through our master item slice database
        foreach (InventoryItemData item in masterInventoryList)
        {
            // 3. Check if either the specific slice ID OR the entire outfit bundle package name is unlocked
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
                        // 🌟 Directs the exact database ID, categories, prefabs, and 2D textures safely
                        buttonScript.SetupButtonDetails(item.itemId, item.category, item.itemMeshPrefab, item.itemIcon);
                        
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

        // 🔁 Instantly update button action text labels without expensive redrawing halts
        RefreshButtonLabels();
        
        // ☁️ Save changes to the cloud automatically!
        SaveAvatarToCloud();

        // TODO: Trigger your 3D Avatar/Character Mesh Swapper script updates here!
    }

    // 🎨 NEW PLAY: Smart text state refresher loop
    public void RefreshButtonLabels()
    {
        foreach (InventoryItemButton button in spawnedButtons)
        {
            if (button == null || button.txtEquipStatus == null) continue;

            // Isolate matching target allocations to see if this specific item asset is active
            bool isCurrentEquipped = false;
            string cat = button.category != null ? button.category.ToLower().Trim() : "";
            
            switch (cat)
            {
                case "head": isCurrentEquipped = (button.itemId == equippedHeadId); break;
                case "torso": 
                case "body": isCurrentEquipped = (button.itemId == equippedBodyId); break;
                case "legs": isCurrentEquipped = (button.itemId == equippedLegsId); break;
                case "feet": isCurrentEquipped = (button.itemId == equippedFeetId); break;
                case "accessory": isCurrentEquipped = (button.itemId == equippedAccessoryId); break;
            }

            // Apply minimalist UX text states cleanly
            button.txtEquipStatus.text = isCurrentEquipped ? "EQUIPPED" : "EQUIP";
        }
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