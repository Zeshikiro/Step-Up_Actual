using System.Collections.Generic;
using UnityEngine;

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
        UnlockItem("Casual_Torso"); // Safety net matching bundle naming extensions
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

    public void UnlockFullOutfitBundle(string outfitName)
    {
        UnlockItem(outfitName); 
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
                            buttonScript.txtOutfitName.text = item.associatedOutfitName;
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
        switch (category)
        {
            case "Head":
                equippedHeadId = itemId;
                break;
            case "Torso":
                equippedBodyId = itemId;
                break;
            case "Legs":
                equippedLegsId = itemId;
                break;
            case "Feet":
                equippedFeetId = itemId;
                break;
            case "Accessory":
                equippedAccessoryId = itemId;
                break;
        }

        Debug.Log($"[Wardrobe Engine] Successfully equipped {itemId} into the {category} slot.");

        // 🔁 Instantly update button action text labels without expensive redrawing halts
        RefreshButtonLabels();

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
            switch (button.category)
            {
                case "Head": isCurrentEquipped = (button.itemId == equippedHeadId); break;
                case "Torso": isCurrentEquipped = (button.itemId == equippedBodyId); break;
                case "Legs": isCurrentEquipped = (button.itemId == equippedLegsId); break;
                case "Feet": isCurrentEquipped = (button.itemId == equippedFeetId); break;
                case "Accessory": isCurrentEquipped = (button.itemId == equippedAccessoryId); break;
            }

            // Apply minimalist UX text states cleanly
            button.txtEquipStatus.text = isCurrentEquipped ? "EQUIPPED" : "EQUIP";
        }
    }

    private Transform GetTargetGrid(string category)
    {
        switch (category)
        {
            case "head": return headContentGrid;
            case "torso": return torsoContentGrid;
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