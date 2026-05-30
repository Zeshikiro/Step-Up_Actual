using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemButton : MonoBehaviour
{
    [Header("Data Fields (Fed by InventoryManager)")]
    public string itemId;                  // Matches the unique database item ID string
    public string category;                // "Head", "Torso", "Legs", or "Feet"
    public GameObject itemPrefab;          // The 3D mesh model reference

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
    public void SetupButtonDetails(string id, string cat, GameObject prefab, Sprite uniqueIcon)
    {
        itemId = id;
        category = cat;
        itemPrefab = prefab;

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

        // Toggle status text feedback states smoothly
        if (txtEquipStatus != null)
        {
            txtEquipStatus.text = isEquipped ? "EQUIPPED" : "EQUIP";
            txtEquipStatus.color = isEquipped ? Color.green : Color.white;
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

        string cleanCategory = string.IsNullOrEmpty(category) ? "" : category.ToLower().Trim();

        // 1. Direct the 3D model piece to the character's bone hierarchy and save its state data
        switch (cleanCategory)
        {
            case "head":
                customizer.EquipHeadObject(itemPrefab);
                InventoryManager.Instance.equippedHeadId = itemId;
                break;

            case "torso":
            case "body":
                customizer.EquipBodyObject(itemPrefab);
                InventoryManager.Instance.equippedBodyId = itemId;
                break;

            case "legs":
            case "pants":
                customizer.EquipLegsObject(itemPrefab);
                InventoryManager.Instance.equippedLegsId = itemId;
                break;

            case "feet":
            case "shoes":
                customizer.EquipFeetObject(itemPrefab);
                InventoryManager.Instance.equippedFeetId = itemId;
                break;

            case "accessory":
                customizer.EquipAccessoryObject(itemPrefab);
                break;

            default:
                Debug.LogError($"[Wardrobe Engine] Unrecognized category '{category}' on item {itemId}. Cannot equip.");
                return;
        }

        // 2. Loop through all active buttons in the scrollview to update text statuses instantly
        InventoryItemButton[] allButtons = FindObjectsByType<InventoryItemButton>(FindObjectsSortMode.None);
        foreach (InventoryItemButton button in allButtons)
        {
            button.RefreshVisibility();
        }

        Debug.Log($"[Wardrobe Engine] Successfully equipped {itemId} into the {category} panel slot.");
    }
}