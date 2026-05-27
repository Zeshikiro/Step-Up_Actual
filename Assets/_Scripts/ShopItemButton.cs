using UnityEngine;
using UnityEngine.UI;

public class ShopItemButton : MonoBehaviour
{
    [Header("Outfit Meshes to Equip")]
    public Mesh headMesh;
    public Mesh bodyMesh;
    public Mesh legsMesh;
    public Mesh feetMesh;
    public Mesh accessoryMesh; // Added (Optional Backpack/Hat/etc.)

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

    public void EquipOutfit()
    {
        if (customizer == null) return;

        if (headMesh != null) customizer.ChangeMeshHead(headMesh);
        if (bodyMesh != null) customizer.ChangeMeshBody(bodyMesh);
        if (legsMesh != null) customizer.ChangeMeshLegs(legsMesh);
        if (feetMesh != null) customizer.ChangeMeshFeet(feetMesh);
        
        // This will send the backpack asset if it exists, or clear it if it doesn't!
        customizer.ChangeMeshAccessory(accessoryMesh);
        
        Debug.Log($"Equipped outfit items from: {gameObject.name}");
    }
}