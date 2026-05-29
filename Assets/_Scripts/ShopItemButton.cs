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
    public ShopItem shopItem;

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

        // Transaction Logic
        if (!shopItem.isPurchased)
        {
            if (avatarCustomizer.currentCoins >= shopItem.price)
        {
            avatarCustomizer.currentCoins -= shopItem.price;
            shopItem.isPurchased = true;
    
            // 1. Tell your runtime scene customize manager it's owned
            avatarCustomizer.RegisterPurchasedItem(shopItem.itemName);
    
            // 2. NEW CONNECTION: Tell your persistent inventory storage manager to unpack the sliced parts!
            if (InventoryManager.Instance != null)
            {
            InventoryManager.Instance.UnlockFullOutfitBundle(shopItem.itemName);
            }
    
            avatarCustomizer.UpdateCoinDisplay(); 
            RefreshButtonState();
        }
        }

        // 👕 EQUIP THE WHOLE BUNDLE AT ONCE FOR PREVIEW/EQUIP
        if (shopItem.headPrefab != null)       avatarCustomizer.EquipHeadObject(shopItem.headPrefab);
        if (shopItem.torsoPrefab != null)      avatarCustomizer.EquipBodyObject(shopItem.torsoPrefab);
        if (shopItem.legsPrefab != null)       avatarCustomizer.EquipLegsObject(shopItem.legsPrefab);
        if (shopItem.feetPrefab != null)       avatarCustomizer.EquipFeetObject(shopItem.feetPrefab);
        if (shopItem.accessoryPrefab != null)  avatarCustomizer.EquipAccessoryObject(shopItem.accessoryPrefab);
    }
}