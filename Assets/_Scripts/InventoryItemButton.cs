using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    [Header("Item ID Link")]
    public string itemId; // Must match the exact itemId string from the Shop card!

    [Header("Outfit Meshes to Equip")]
    public Mesh headMesh;
    public Mesh bodyMesh;
    public Mesh legsMesh;
    public Mesh feetMesh;
    public Mesh accessoryMesh;

    private AvatarCustomizer customizer;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        customizer = Object.FindFirstObjectByType<AvatarCustomizer>();
    }

    private void Start()
    {
        if (button != null)
        {
            button.onClick.AddListener(EquipOutfit);
        }
    }

    private void OnEnable()
    {
        RefreshVisibility();
    }

    // Checks the InventoryManager to see if this card should be visible
    public void RefreshVisibility()
    {
        if (InventoryManager.Instance == null) return;

        bool standardUnlocked = InventoryManager.Instance.IsItemUnlocked(itemId);
        gameObject.SetActive(standardUnlocked);
    }

    public void EquipOutfit()
    {
        if (customizer == null || InventoryManager.Instance == null) return;

        // 1. Swap model mesh assets instantly
        if (headMesh != null) customizer.ChangeMeshHead(headMesh);
        if (bodyMesh != null) customizer.ChangeMeshBody(bodyMesh);
        if (legsMesh != null) customizer.ChangeMeshLegs(legsMesh);
        if (feetMesh != null) customizer.ChangeMeshFeet(feetMesh);
        customizer.ChangeMeshAccessory(accessoryMesh);

        // 2. Save choice directly into persistent storage tracking references
        InventoryManager.Instance.equippedHeadId = headMesh != null ? headMesh.name : "None";
        InventoryManager.Instance.equippedBodyId = bodyMesh != null ? bodyMesh.name : "None";
        InventoryManager.Instance.equippedLegsId = legsMesh != null ? legsMesh.name : "None";
        InventoryManager.Instance.equippedFeetId = feetMesh != null ? feetMesh.name : "None";
        InventoryManager.Instance.equippedAccessoryId = accessoryMesh != null ? accessoryMesh.name : "None";

        Debug.Log($"Equipped character set from Inventory: {itemId}");
    }
}