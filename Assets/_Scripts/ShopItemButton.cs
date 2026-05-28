using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [Header("Item ID & Economy Settings")]
    public string itemId;
    public int itemPrice = 500;

    [Header("UI Text Reference")]
    public TextMeshProUGUI priceText; // Drag your card's price text component here

    [Header("Outfit Meshes to Preview")]
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
            button.onClick.AddListener(BuyOutfit);
        }
        
        RefreshButtonState();
    }

    private void OnEnable()
    {
        RefreshButtonState();
    }

    // Changes the button text or interactable state if already bought
    public void RefreshButtonState()
    {
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.IsItemUnlocked(itemId))
        {
            if (priceText != null) priceText.text = "OWNED";
            if (button != null) button.interactable = false; // Prevents buying twice
        }
        else
        {
            if (priceText != null) priceText.text = $"{itemPrice} P";
            if (button != null) button.interactable = true;
        }
    }

    public void BuyOutfit()
    {
        if (InventoryManager.Instance == null || customizer == null) return;

        // 1. Double check ownership state
        if (InventoryManager.Instance.IsItemUnlocked(itemId)) return;

        // 2. Try to spend coins
        if (InventoryManager.Instance.SpendCoins(itemPrice))
        {
            // 3. Unlock item in global save layer
            InventoryManager.Instance.UnlockItem(itemId);
            
            // 4. Instantly preview the item on the character model!
            PreviewPurchasedOutfit();

            // 5. Refresh UI layout displays
            RefreshButtonState();
            customizer.UpdateCoinDisplay(); 
        }
        else
        {
            Debug.LogWarning("Insufficient funds to buy this character outfit!");
        }
    }

    private void PreviewPurchasedOutfit()
    {
        if (headMesh != null) customizer.ChangeMeshHead(headMesh);
        if (bodyMesh != null) customizer.ChangeMeshBody(bodyMesh);
        if (legsMesh != null) customizer.ChangeMeshLegs(legsMesh);
        if (feetMesh != null) customizer.ChangeMeshFeet(feetMesh);
        customizer.ChangeMeshAccessory(accessoryMesh);
    }
}