using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

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

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            return true;
        }
        return false;
    }
}