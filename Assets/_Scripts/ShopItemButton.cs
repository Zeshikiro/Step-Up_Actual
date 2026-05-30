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
        RefreshButtonState();
    }

    public void RefreshButtonState()
    {
        if (priceText != null)
        {
            priceText.text = shopItem.isPurchased ? "Owned" : shopItem.price.ToString() + " Coins";
        }
    }

    // 🔘 LINK THIS TO YOUR SHOP BUTTON'S ONCLICK() EVENT IN THE INSPECTOR
    public void BuyOrPreviewItem()
    {
        if (avatarCustomizer == null) return;

        // Transaction Logic: Only runs if the student doesn't own this item card yet
        if (!shopItem.isPurchased)
        {
            if (avatarCustomizer.currentCoins >= shopItem.price)
            {
                avatarCustomizer.currentCoins -= shopItem.price;
                shopItem.isPurchased = true;
        
                // 1. Tell your runtime scene customize manager it's owned
                avatarCustomizer.RegisterPurchasedItem(shopItem.itemName);
        
                // 2. Tell your persistent inventory storage manager to unpack the sliced parts!
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.UnlockFullOutfitBundle(shopItem.itemName);
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