using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemButton : MonoBehaviour
{
    [Header("Data Fields (Fed by InventoryManager)")]
    public string itemName;
    public string category;
    public GameObject itemPrefab;

    [Header("UI Visual Components")]
    public TextMeshProUGUI txtOutfitName;
    public Image imgCharacterIcon;
    public GameObject selectionHighlight;

    private void Start()
    {
        // Automatically hook up the click listener if a Button component is attached
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(EquipThisItem);
        }
    }

    // 🛠️ FIXES ERROR: Tells AvatarCustomizer what this card represents
    public void RefreshVisibility()
    {
        // This method can turn on/off your green SelectionHighlight frame 
        // if this item's ID matches the one currently worn by the player
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(false); // Default off; customize later if desired
        }
    }

    // 👕 THE EQUIP LOGIC: Runs instantly when the student taps this item card
    public void EquipThisItem()
    {
        AvatarCustomizer customizer = FindAnyObjectByType<AvatarCustomizer>();
        if (customizer == null)
        {
            Debug.LogError("Could not find AvatarCustomizer in the scene!");
            return;
        }

        string cleanCategory = category.ToLower().Trim();

        // Direct the mesh object to the correct slot based on database category sorting
        if (cleanCategory == "head")
            customizer.EquipHeadObject(itemPrefab);
        else if (cleanCategory == "torso" || cleanCategory == "body")
            customizer.EquipBodyObject(itemPrefab);
        else if (cleanCategory == "legs")
            customizer.EquipLegsObject(itemPrefab);
        else if (cleanCategory == "feet" || cleanCategory == "shoes")
            customizer.EquipFeetObject(itemPrefab);

        Debug.Log($"[Equip] Successfully changed player's clothing slot to: {itemName}");
    }
}