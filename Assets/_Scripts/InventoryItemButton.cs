using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemButton : MonoBehaviour
{
    [Header("Data Fields (Fed by InventoryManager)")]
    public string itemId;                  // Matches the unique database item ID string
    public string category;                // "Head", "Torso", "Legs", or "Feet"
    public GameObject itemPrefab;          // The 3D mesh model reference
    public int unlockCost;                 // Cost to purchase this item

    [Header("UI Visual Components")]
    public TextMeshProUGUI txtOutfitName;  // Displays pretty display name
    public Image imgCharacterIcon;         // Slot for your NOBG UI Sprites
    public TextMeshProUGUI txtEquipStatus; // Tracks "EQUIP" vs "EQUIPPED" labels

    private void Start()
    {
        // Automatically hook up the click listener if a Button component is attached
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(EquipThisItem);
        }
    }

    // Fed by your inventory manager generation loop to initialize values
    public void SetupButtonDetails(string id, string cat, GameObject prefab, Sprite uniqueIcon, int cost)
    {
        itemId = id;
        category = cat;
        itemPrefab = prefab;
        unlockCost = cost;

        if (txtOutfitName != null)
        {
            txtOutfitName.text = id; // Fallback text setup
        }

        // Swaps the blank white texture placeholder for your actual clothing sprite
        if (imgCharacterIcon != null && uniqueIcon != null)
        {
            imgCharacterIcon.sprite = uniqueIcon;
            imgCharacterIcon.color = Color.white; // Ensures full image visibility
        }

        // Dynamically assign Rarity Color to the background image!
        Image bgImage = GetComponent<Image>();
        if (bgImage != null && InventoryManager.Instance != null)
        {
            bgImage.color = InventoryManager.Instance.GetRarityColor(unlockCost);
        }

        RefreshVisibility();
    }

    // Checks active inventory manager variables to toggle text labels & colors dynamically
    public void RefreshVisibility()
    {
        if (InventoryManager.Instance == null) return;

        bool isEquipped = false;
        string cleanCategory = string.IsNullOrEmpty(category) ? "" : category.ToLower().Trim();
        
        // Compare this card's item ID against what the manager tracks as currently worn
        if (cleanCategory == "head" && InventoryManager.Instance.equippedHeadId == itemId) isEquipped = true;
        else if ((cleanCategory == "torso" || cleanCategory == "body") && InventoryManager.Instance.equippedBodyId == itemId) isEquipped = true;
        else if ((cleanCategory == "legs" || cleanCategory == "pants") && InventoryManager.Instance.equippedLegsId == itemId) isEquipped = true;
        else if ((cleanCategory == "feet" || cleanCategory == "shoes") && InventoryManager.Instance.equippedFeetId == itemId) isEquipped = true;
        else if (cleanCategory == "accessory" && InventoryManager.Instance.equippedAccessoryId == itemId) isEquipped = true;

        // Check if item is unlocked!
        bool isUnlocked = InventoryManager.Instance.IsItemUnlocked(itemId);

        // Toggle status text feedback states smoothly
        if (txtEquipStatus != null)
        {
            if (isEquipped)
            {
                txtEquipStatus.text = "EQUIPPED";
                txtEquipStatus.color = Color.green;
            }
            else if (isUnlocked)
            {
                txtEquipStatus.text = "EQUIP";
                txtEquipStatus.color = Color.white;
            }
            else
            {
                // Show purchase price if locked
                txtEquipStatus.text = $"BUY {unlockCost}c";
                txtEquipStatus.color = Color.yellow;
            }
        }
    }

    // Runs instantly when the student taps anywhere on this item card frame
    public void EquipThisItem()
    {
        AvatarCustomizer customizer = FindAnyObjectByType<AvatarCustomizer>();
        if (customizer == null || InventoryManager.Instance == null) return;

        if (itemPrefab == null)
        {
            Debug.LogWarning($"[Wardrobe Engine] No 3D prefab model assigned to item asset: {itemId}");
            return;
        }

        // ECONOMY CHECK: Is the item locked?
        if (!InventoryManager.Instance.IsItemUnlocked(itemId))
        {
            // Attempt to buy it
            if (InventoryManager.Instance.SpendCoins(unlockCost))
            {
                InventoryManager.Instance.UnlockItem(itemId);
                Debug.Log($"[Wardrobe Engine] Successfully purchased {itemId} for {unlockCost} coins!");
            }
            else
            {
                Debug.LogWarning($"[Wardrobe Engine] Not enough coins to buy {itemId}!");
                
                // Visual feedback for being broke
                if (txtEquipStatus != null)
                {
                    txtEquipStatus.text = "NOT ENOUGH COINS";
                    txtEquipStatus.color = Color.red;
                    Invoke("RefreshVisibility", 1.5f); // Reset text after 1.5s
                }
                return; // Stop here, do not equip
            }
        }

        // 1. Use the new centralized Equip Engine to handle saving, UI refreshing, and Mesh swapping!
        // This completely replaces the old "Approach A" customizer code, fixing all naming bugs!
        InventoryManager.Instance.EquipItem(itemId, category);
    }
}