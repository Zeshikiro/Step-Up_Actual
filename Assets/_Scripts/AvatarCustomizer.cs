using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AvatarCustomizer : MonoBehaviour
{
    [Header("Main UI Pages")]
    public GameObject shopPage;
    public GameObject customizePage;

    [Header("Master Inventory System")]
    public System.Collections.Generic.List<string> purchasedItemNames = new System.Collections.Generic.List<string>();

    [Header("Avatar Container")]
    [Tooltip("Drag the single unified AvatarContainer here")]
    public GameObject avatarContainer;

    [Header("UI Sub-Panels")]
    public GameObject[] subPanels;
    public GameObject settingsPanel;
    
    [Header("UI Tab Buttons (For Highlighting)")]
    public Image[] tabButtons;
    public Color activeTabColor = new Color(1f, 0.8f, 0.2f, 1f); // Orange-ish highlight
    public Color inactiveTabColor = Color.white;

    [Header("Wallet UI Components")]
    public Text coinTextDisplay; 
    public TextMeshProUGUI coinTextDisplayTMP;
    public int currentCoins = 500;

    [Header("Current Active GameObjects (Runtime Visibility)")]
    public GameObject currentHead;
    public GameObject currentBody;
    public GameObject currentLegs;
    public GameObject currentFeet;
    public GameObject currentAccessory;

    [Header("Gender Armature Target")]
    public Transform activeArmatureRoot; 

    [Header("Wardrobe UI Grids (Drag 'Content' objects here)")]
    public Transform wardrobeHeadGrid;
    public Transform wardrobeTorsoGrid;
    public Transform wardrobeLegsGrid;
    public Transform wardrobeFeetGrid;
    public Transform wardrobeAccessoryGrid;

    [Header("Data & Economy Persistence")]
    public List<string> purchasedItemIDs = new List<string>() { "default_casual" };

    // Kept for structural compatibility, no longer needs to destroy objects dynamically
    private Dictionary<string, GameObject> spawnedParts = new Dictionary<string, GameObject>();

    private void Start()
    {
        // CLEAR ANY ACCIDENTAL INSPECTOR ASSIGNMENTS!
        // These fields are strictly for runtime tracking. If they were accidentally populated in the Inspector,
        // the HandleObjectEquip function would literally turn off the AvatarContainer!
        currentHead = null;
        currentBody = null;
        currentLegs = null;
        currentFeet = null;
        currentAccessory = null;

        if (avatarContainer != null) NormalizeScales(avatarContainer.transform);

        // Dynamically pass our local UI grids to the persistent InventoryManager!
        if (InventoryManager.Instance != null)
        {
            if (wardrobeHeadGrid != null) InventoryManager.Instance.headContentGrid = wardrobeHeadGrid;
            if (wardrobeTorsoGrid != null) InventoryManager.Instance.torsoContentGrid = wardrobeTorsoGrid;
            if (wardrobeLegsGrid != null) InventoryManager.Instance.legsContentGrid = wardrobeLegsGrid;
            if (wardrobeFeetGrid != null) InventoryManager.Instance.feetContentGrid = wardrobeFeetGrid;
            if (wardrobeAccessoryGrid != null) InventoryManager.Instance.accessoryContentGrid = wardrobeAccessoryGrid;

            // Generate the buttons for this scene's wardrobe UI
            InventoryManager.Instance.GenerateInventoryUI();
        }

        InitializeAvatarState();
        UpdateCoinDisplay();
        ToggleShopPanel(false);
    }

    private void NormalizeScales(Transform obj)
    {
        obj.localScale = Vector3.one;
        foreach (Transform child in obj)
        {
            NormalizeScales(child);
        }
    }

    public void InitializeAvatarState()
    {
        // We now use a single unified container, so we just target it directly!
        UpdateArmatureTarget(true);
    }

    public void UpdateCoinDisplay()
    {
        int displayCoins = currentCoins;
        if (InventoryManager.Instance != null)
        {
            displayCoins = InventoryManager.Instance.coins;
        }

        if (coinTextDisplayTMP != null) coinTextDisplayTMP.text = displayCoins.ToString();
        if (coinTextDisplay != null) coinTextDisplay.text = displayCoins.ToString();
    }

    public void RegisterPurchasedItem(string itemName)
    {
        if (!purchasedItemNames.Contains(itemName))
        {
            purchasedItemNames.Add(itemName);
            Debug.Log($"[Inventory] Master registry updated! Unlocked: {itemName}");
        }
    }

    // ==========================================
    // 🛠️ INTERACTION ENDPOINTS 
    // ==========================================
    public void EquipHeadObject(GameObject newHeadObject) { currentHead = HandleObjectEquip(newHeadObject, "Head", currentHead); }
    public void EquipBodyObject(GameObject newBodyObject) { currentBody = HandleObjectEquip(newBodyObject, "Body", currentBody); }
    public void EquipLegsObject(GameObject newLegsObject) { currentLegs = HandleObjectEquip(newLegsObject, "Legs", currentLegs); }
    public void EquipFeetObject(GameObject newFeetObject) { currentFeet = HandleObjectEquip(newFeetObject, "Feet", currentFeet); }
    public void EquipAccessoryObject(GameObject newAccessoryObject) { currentAccessory = HandleObjectEquip(newAccessoryObject, "Accessory", currentAccessory); }

    public void ChangeMeshHead(GameObject newHeadObject) { EquipHeadObject(newHeadObject); }
    public void ChangeMeshBody(GameObject newBodyObject) { EquipBodyObject(newBodyObject); }
    public void ChangeMeshLegs(GameObject newLegsObject) { EquipLegsObject(newLegsObject); }
    public void ChangeMeshFeet(GameObject newFeetObject) { EquipFeetObject(newFeetObject); }
    public void ChangeMeshAccessory(GameObject newAccessoryObject) { EquipAccessoryObject(newAccessoryObject); }

    // ==========================================
    // 🦴 APPROACH A VISIBILITY TOGGLE ENGINE
    // ==========================================
    private GameObject HandleObjectEquip(GameObject prefab, string category, GameObject currentActiveObject)
    {
        if (prefab == null) return currentActiveObject;

        // 1. Identify which gender avatar root is currently active in your scene hierarchy
        GameObject activeAvatarRoot = avatarContainer;
        if (activeAvatarRoot == null) return currentActiveObject;

        // 2. Search recursively inside the active character structure for a child object matching the prefab name
        Transform targetMeshTransform = FindChildRecursive(activeAvatarRoot.transform, prefab.name);

        if (targetMeshTransform != null)
        {
            // 3. Turn OFF the old apparel item in this category
            if (currentActiveObject != null)
            {
                currentActiveObject.SetActive(false);
            }

            // 4. Turn ON the target clothing mesh that belongs directly to this character skeleton
            GameObject newPart = targetMeshTransform.gameObject;
            newPart.SetActive(true);

            // Track it inside the internal lookup dictionary
            spawnedParts[category] = newPart;
            
            Debug.Log($"[Customizer] Approach A active: Swapped visible {category} mesh to pre-skinned asset: '{newPart.name}'");
            return newPart;
        }
        else
        {
            Debug.LogWarning($"[Customizer] Approach A Error: Could not find an internal child mesh named '{prefab.name}' inside the {activeAvatarRoot.name} asset object structure. Check your naming conversions.");
            return currentActiveObject;
        }
    }

    public void SetGender(bool isMale)
    {
        if (InventoryManager.Instance != null)
        {
            // The Male/Female button is now strictly a Shop UI Filter!
            // It does NOT change the physical character on the screen.
            InventoryManager.Instance.isMaleAvatar = isMale;
            
            // Regenerate the Shop UI dynamically so it shows the selected wardrobe!
            InventoryManager.Instance.GenerateInventoryUI();
        }
    }

    private void UpdateArmatureTarget(bool isMale)
    {
        // We now only use a single unified AvatarContainer
        GameObject activeAvatarRoot = avatarContainer;
        if (activeAvatarRoot != null)
        {
            Transform foundArmature = FindChildRecursive(activeAvatarRoot.transform, "CharacterArmature");
            if (foundArmature != null)
            {
                activeArmatureRoot = foundArmature;
            }
        }
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent.name == targetName) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), targetName);
            if (result != null) return result;
        }
        return null;
    }

    public void ToggleShopPanel(bool openShopView)
    {
        if (shopPage != null) shopPage.SetActive(openShopView);
        if (customizePage != null) customizePage.SetActive(!openShopView);

        if (openShopView)
        {
            ShopItemButton[] shopButtons = Object.FindObjectsByType<ShopItemButton>(FindObjectsSortMode.None);
            foreach (var btn in shopButtons) btn.RefreshButtonState();
        }
        else
        {
            InventoryItemButton[] invButtons = Object.FindObjectsByType<InventoryItemButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in invButtons) btn.RefreshVisibility();
        }
    }

    public void OpenPanel(int panelIndex)
    {
        for (int i = 0; i < subPanels.Length; i++)
        {
            if (subPanels[i] != null)
            {
                subPanels[i].SetActive(i == panelIndex);
            }
        }

        // Apply visual color highlight to the active tab button
        if (tabButtons != null && tabButtons.Length > 0)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null)
                {
                    tabButtons[i].color = (i == panelIndex) ? activeTabColor : inactiveTabColor;
                }
            }
        }
    }

    public bool AttemptItemPurchase(string itemID, int cost)
    {
        if (purchasedItemIDs.Contains(itemID))
        {
            Debug.LogWarning($"Item {itemID} is already unlocked!");
            return true; 
        }

        bool purchaseSuccessful = false;

        // Try to spend from the global Master Inventory first
        if (InventoryManager.Instance != null)
        {
            purchaseSuccessful = InventoryManager.Instance.SpendCoins(cost);
        }
        else if (currentCoins >= cost)
        {
            currentCoins -= cost;
            purchaseSuccessful = true;
        }

        if (purchaseSuccessful)
        {
            purchasedItemIDs.Add(itemID); 
            UpdateCoinDisplay();
            
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.UnlockItem(itemID);
            }

            Debug.Log($"Successfully purchased: {itemID}.");
            return true;
        }

        Debug.LogError("Not enough coins to purchase item!");
        return false;
    }

    public bool CheckIfItemOwned(string itemID)
    {
        return purchasedItemIDs.Contains(itemID);
    }

    public void GoToSampleScene()
    {
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("SampleScene");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public void GoToMainMenu()
    {
        if (SceneLoader.Instance == null) 
        {
            SceneLoader.Instance = FindFirstObjectByType<SceneLoader>(FindObjectsInactive.Include);
            if (SceneLoader.Instance != null) SceneLoader.Instance.gameObject.SetActive(true);
        }

        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene("LoginScene");
        else UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}