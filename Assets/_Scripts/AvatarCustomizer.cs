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

    [Header("Gender Containers")]
    public GameObject femaleAvatar;
    public GameObject maleAvatar;

    [Header("UI Sub-Panels")]
    public GameObject[] subPanels;

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

    [Header("Default Casual Objects (Assign inside Avatar Hierarchy)")]
    public GameObject defaultMaleHead;
    public GameObject defaultMaleBody;
    public GameObject defaultMaleLegs;
    public GameObject defaultMaleFeet;
    [Space]
    public GameObject defaultFemaleHead;
    public GameObject defaultFemaleBody;
    public GameObject defaultFemaleLegs;
    public GameObject defaultFemaleFeet;

    [Header("Gender Armature Target")]
    public Transform activeArmatureRoot; 

    [Header("Data & Economy Persistence")]
    public List<string> purchasedItemIDs = new List<string>() { "default_casual" };

    // Kept for structural compatibility, no longer needs to destroy objects dynamically
    private Dictionary<string, GameObject> spawnedParts = new Dictionary<string, GameObject>();

    private void Start()
    {
        InitializeAvatarState();
        UpdateCoinDisplay();
        ToggleShopPanel(false);
    }

    public void InitializeAvatarState()
    {
        bool isMale = maleAvatar != null && maleAvatar.activeSelf;
        
        UpdateArmatureTarget(isMale);
        ResetToDefaults(isMale);
    }

    public void ResetToDefaults(bool isMale)
    {
        DeactivateCurrentSet();
        spawnedParts.Clear();

        if (isMale)
        {
            currentHead = defaultMaleHead;
            currentBody = defaultMaleBody;
            currentLegs = defaultMaleLegs;
            currentFeet = defaultMaleFeet;
        }
        else
        {
            currentHead = defaultFemaleHead;
            currentBody = defaultFemaleBody;
            currentLegs = defaultFemaleLegs;
            currentFeet = defaultFemaleFeet;
        }

        if (currentHead != null) currentHead.SetActive(true);
        if (currentBody != null) currentBody.SetActive(true);
        if (currentLegs != null) currentLegs.SetActive(true);
        if (currentFeet != null) currentFeet.SetActive(true);
        if (currentAccessory != null) currentAccessory.SetActive(false);
    }

    private void DeactivateCurrentSet()
    {
        if (currentHead != null) currentHead.SetActive(false);
        if (currentBody != null) currentBody.SetActive(false);
        if (currentLegs != null) currentLegs.SetActive(false);
        if (currentFeet != null) currentFeet.SetActive(false);
        if (currentAccessory != null) currentAccessory.SetActive(false);
    }

    public void UpdateCoinDisplay()
    {
        if (coinTextDisplayTMP != null) coinTextDisplayTMP.text = currentCoins.ToString();
        if (coinTextDisplay != null) coinTextDisplay.text = currentCoins.ToString();
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
        GameObject activeAvatarRoot = (maleAvatar != null && maleAvatar.activeSelf) ? maleAvatar : femaleAvatar;
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
        if (maleAvatar != null) maleAvatar.SetActive(isMale);
        if (femaleAvatar != null) femaleAvatar.SetActive(!isMale);
        
        UpdateArmatureTarget(isMale);
        ResetToDefaults(isMale);
    }

    private void UpdateArmatureTarget(bool isMale)
    {
        GameObject activeAvatarRoot = isMale ? maleAvatar : femaleAvatar;
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
    }

    public bool AttemptItemPurchase(string itemID, int cost)
    {
        if (purchasedItemIDs.Contains(itemID))
        {
            Debug.LogWarning($"Item {itemID} is already unlocked!");
            return true; 
        }

        if (currentCoins >= cost)
        {
            currentCoins -= cost;
            purchasedItemIDs.Add(itemID); 
            UpdateCoinDisplay();
            
            Debug.Log($"Successfully purchased: {itemID}. Remaining Coins: {currentCoins}");
            return true;
        }

        Debug.LogError("Not enough coins to purchase item!");
        return false;
    }

    public bool CheckIfItemOwned(string itemID)
    {
        return purchasedItemIDs.Contains(itemID);
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }
}