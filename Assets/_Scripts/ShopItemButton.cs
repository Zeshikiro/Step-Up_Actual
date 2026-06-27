using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemButton : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName; // e.g., "Beach" or "Adventurer"
        public int price;
        public bool isPurchased;

        [Header("Full Outfit Prefabs (Route A)")]
        [Tooltip("Leave unassigned if the outfit doesn't include this part")]
        public GameObject headPrefab;
        public GameObject torsoPrefab;
        public GameObject legsPrefab;
        public GameObject feetPrefab;
        public GameObject accessoryPrefab;
    }

    [Header("Shop Item Data")]
    public ShopItem shopItem; // Fixed typo: Changed lowercase 'shopItem' type to uppercase 'ShopItem'

    [Header("UI Component Links")]
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private AvatarCustomizer avatarCustomizer;

    private void Awake()
    {
        avatarCustomizer = Object.FindAnyObjectByType<AvatarCustomizer>();
    }

    private void Start()
    {
        // Automatically find the Button component and hook up the click event!
        // This prevents the need to manually assign it in the Unity Inspector.
        Button btn = GetComponent<Button>();
        if (btn == null) btn = GetComponentInChildren<Button>();
        
        if (btn != null)
        {
            btn.onClick.AddListener(BuyOrPreviewItem);
        }

        RefreshButtonState();
    }

    public void RefreshButtonState()
    {
        // Sync with the global database on load so it remembers purchases across scenes!
        if (InventoryManager.Instance != null && InventoryManager.Instance.IsItemUnlocked(shopItem.itemName))
        {
            shopItem.isPurchased = true;
        }

        if (priceText != null)
        {
            priceText.text = shopItem.isPurchased ? "Owned" : shopItem.price.ToString();
        }
    }

    // 🔘 LINK THIS TO YOUR SHOP BUTTON'S ONCLICK() EVENT IN THE INSPECTOR
    public void BuyOrPreviewItem()
    {
        Debug.Log($"[ShopItemButton] BuyOrPreviewItem clicked for {shopItem.itemName}");

        if (avatarCustomizer == null) 
        {
            Debug.LogError($"[ShopItemButton] CRITICAL ERROR: avatarCustomizer is NULL! Make sure the AvatarCustomizer script is attached to a GameObject in your scene!");
            return;
        }

        // Transaction Logic: Only runs if the student doesn't own this item card yet
        if (!shopItem.isPurchased)
        {
            Debug.Log($"[ShopItemButton] Attempting purchase for {shopItem.itemName}. Cost: {shopItem.price}");
            bool purchaseSuccessful = false;
            
            // Deduct coins from global Inventory Manager
            if (InventoryManager.Instance != null && InventoryManager.Instance.SpendCoins(shopItem.price))
            {
                purchaseSuccessful = true;
            }
            else if (avatarCustomizer.currentCoins >= shopItem.price) // Fallback for local testing
            {
                avatarCustomizer.currentCoins -= shopItem.price;
                purchaseSuccessful = true;
            }

            if (purchaseSuccessful)
            {
                shopItem.isPurchased = true;
        
                // 1. Tell your runtime scene customize manager it's owned
                avatarCustomizer.RegisterPurchasedItem(shopItem.itemName);
        
                // 2. Tell your persistent inventory storage manager to unpack the sliced parts!
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.UnlockFullOutfitBundle(shopItem.itemName);
                    InventoryManager.Instance.GenerateInventoryUI(); // INSTANTLY REFRESH WARDROBE!
                }
        
                avatarCustomizer.UpdateCoinDisplay(); 
                RefreshButtonState();

                Debug.Log($"[Shop System] Successfully purchased {shopItem.itemName}! It is now unlocked in your wardrobe inventory.");
            }
            else
            {
                Debug.LogWarning($"[Shop System] Denied purchase for {shopItem.itemName}. Insufficient gold balance.");
            }
        }
    }
}