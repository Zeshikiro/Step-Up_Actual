using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [System.Serializable]
    public class InventoryItemData
    {
        public string itemId;               // e.g., "Adventurer_Head" or "Casual_Head"
        public string associatedOutfitName; // e.g., "Adventurer" or "Beach"
        public string category;             // Must be exactly: "Head", "Torso", "Legs", "Feet", or "Accessory"
        public GameObject itemMeshPrefab;   // The specific 3D model piece to equip
        public Sprite itemIcon;             // UI icon for this individual piece
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
    public string equippedHeadId = "Casual_Head";
    public string equippedBodyId = "Casual_Body";
    public string equippedLegsId = "Casual_Legs";
    public string equippedFeetId = "Casual_Feet";
    public string equippedAccessoryId = "None";

    // Track unlocked item IDs
    private HashSet<string> unlockedItems = new HashSet<string>();

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
        UnlockItem("Casual_Legs");
        UnlockItem("Casual_Feet");
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

    // 👕 HELPER: Call this when a full bundle is purchased to unlock all its parts at once!
    public void UnlockFullOutfitBundle(string outfitName)
    {
        UnlockItem(outfitName); // Unlocks the master outfit package keyword
        UnlockItem(outfitName + "_Head");
        UnlockItem(outfitName + "_Torso");
        UnlockItem(outfitName + "_Legs");
        UnlockItem(outfitName + "_Feet");
        UnlockItem(outfitName + "_Accessory");
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
        // 1. Clear out old layout entries so they don't duplicate when toggling menus
        ClearGrid(headContentGrid);
        ClearGrid(torsoContentGrid);
        ClearGrid(legsContentGrid);
        ClearGrid(feetContentGrid);
        ClearGrid(accessoryContentGrid);

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
                        buttonScript.itemName = item.associatedOutfitName;
                        buttonScript.category = item.category;
                        buttonScript.itemPrefab = item.itemMeshPrefab;
                        
                        // Set the custom visual thumbnail if available
                        var uiImage = newButton.GetComponentInChildren<UnityEngine.UI.Image>();
                        if (uiImage != null && item.itemIcon != null) 
                        {
                            uiImage.sprite = item.itemIcon;
                        }
                    }
                }
            }
        }
    }

    private Transform GetTargetGrid(string category)
    {
        switch (category)
        {
            case "Head": return headContentGrid;
            case "Torso": return torsoContentGrid;
            case "Legs": return legsContentGrid;
            case "Feet": return feetContentGrid;
            case "Accessory": return accessoryContentGrid;
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