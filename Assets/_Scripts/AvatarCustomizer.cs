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
    // Stores the names of all items the user has purchased
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

    [Header("Current Active GameObjects (Runtime)")]
    public GameObject currentHead;
    public GameObject currentBody;
    public GameObject currentLegs;
    public GameObject currentFeet;
    public GameObject currentAccessory;

    [Header("Default Casual Objects (Assign in Inspector)")]
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
    // This list stores the IDs of everything the user has successfully bought
    public List<string> purchasedItemIDs = new List<string>() { "default_casual" };

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
        
        // 🔥 FIX 1: Auto-detect active armature root right at the start
        UpdateArmatureTarget(isMale);
        
        ResetToDefaults(isMale);
    }

    public void ResetToDefaults(bool isMale)
    {
        DeactivateCurrentSet();

        foreach (GameObject spawnedItem in spawnedParts.Values)
        {
            if (spawnedItem != null) Destroy(spawnedItem);
        }
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

    /// <summary>
    /// Adds an item name to our master unlocked inventory list.
    /// </summary>
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

    // ==========================================
    // 🔄 ALIASES FOR YOUR SHOP / INVENTORY BUTTONS
    // ==========================================
    public void ChangeMeshHead(GameObject newHeadObject) { EquipHeadObject(newHeadObject); }
    public void ChangeMeshBody(GameObject newBodyObject) { EquipBodyObject(newBodyObject); }
    public void ChangeMeshLegs(GameObject newLegsObject) { EquipLegsObject(newLegsObject); }
    public void ChangeMeshFeet(GameObject newFeetObject) { EquipFeetObject(newFeetObject); }
    public void ChangeMeshAccessory(GameObject newAccessoryObject) { EquipAccessoryObject(newAccessoryObject); }

    // ==========================================
    // 🦴 CORE ATTACHMENT ENGINE (With Rigid & Skinned Asset Support)
    // ==========================================
    private GameObject HandleObjectEquip(GameObject prefab, string category, GameObject currentActiveObject)
    {
        if (prefab == null) return currentActiveObject;

        // Clean up previous item tracking
        if (currentActiveObject != null)
        {
            if (spawnedParts.ContainsKey(category) && spawnedParts[category] == currentActiveObject)
            {
                Destroy(currentActiveObject);
                spawnedParts.Remove(category);
            }
            else
            {
                currentActiveObject.SetActive(false);
            }
        }

        // Fallback to transform parent if armature is unassigned
        Transform spawnParent = activeArmatureRoot != null ? activeArmatureRoot.parent : transform;

        GameObject newPart = Instantiate(prefab, spawnParent);
        newPart.name = prefab.name;
        
        newPart.transform.localPosition = Vector3.zero;
        newPart.transform.localRotation = Quaternion.identity;
        newPart.transform.localScale = Vector3.one;
        
        newPart.SetActive(true);
        spawnedParts[category] = newPart;

        SkinnedMeshRenderer newRenderer = newPart.GetComponentInChildren<SkinnedMeshRenderer>();
        
        if (newRenderer != null && activeArmatureRoot != null)
        {
            // It's a clothing asset item -> Remap its skin bones
            RemapBonesToActiveArmature(newRenderer);
        }
        else if (activeArmatureRoot != null)
        {
            // 🔥 FIX 2: RIGID ITEM AUTO-ANCHOR (For Backpacks, Hats, Helmets)
            Transform targetBone = null;

            if (category == "Head") 
                targetBone = FindChildRecursive(activeArmatureRoot, "Head");
            else if (category == "Accessory") 
                targetBone = FindChildRecursive(activeArmatureRoot, "Spine2") ?? FindChildRecursive(activeArmatureRoot, "Spine_03") ?? FindChildRecursive(activeArmatureRoot, "Chest");

            if (targetBone != null)
            {
                newPart.transform.SetParent(targetBone);
                newPart.transform.localPosition = Vector3.zero;
                newPart.transform.localRotation = Quaternion.identity;
                newPart.transform.localScale = Vector3.one;
            }
        }

        return newPart;
    }

    private void RemapBonesToActiveArmature(SkinnedMeshRenderer targetRenderer)
    {
        Dictionary<string, Transform> activeBoneMap = new Dictionary<string, Transform>();
        foreach (Transform bone in activeArmatureRoot.GetComponentsInChildren<Transform>())
        {
            if (!activeBoneMap.ContainsKey(bone.name))
                activeBoneMap.Add(bone.name, bone);
        }

        Transform[] originalBones = targetRenderer.bones;
        Transform[] remappedBones = new Transform[originalBones.Length];

        for (int i = 0; i < originalBones.Length; i++)
        {
            string boneName = originalBones[i].name;
            if (activeBoneMap.TryGetValue(boneName, out Transform liveBone))
            {
                remappedBones[i] = liveBone;
            }
        }

        targetRenderer.bones = remappedBones;

        if (targetRenderer.rootBone != null && activeBoneMap.TryGetValue(targetRenderer.rootBone.name, out Transform liveRoot))
        {
            targetRenderer.rootBone = liveRoot;
        }
    }

    public void SetGender(bool isMale)
    {
        if (maleAvatar != null) maleAvatar.SetActive(isMale);
        if (femaleAvatar != null) femaleAvatar.SetActive(!isMale);
        
        // 🔥 FIX 3: Automatically swap the bone armature system to match gender selection
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

    /// <summary>
/// Validates coin balance, handles deductions, and unlocks items.
/// </summary>
public bool AttemptItemPurchase(string itemID, int cost)
{
    // Check if already owned
    if (purchasedItemIDs.Contains(itemID))
    {
        Debug.LogWarning($"Item {itemID} is already unlocked!");
        return true; 
    }

    // Check if player can afford it
    if (currentCoins >= cost)
    {
        currentCoins -= cost;
        purchasedItemIDs.Add(itemID); // 🔑 The item is officially unlocked!
        UpdateCoinDisplay();
        
        Debug.Log($"Successfully purchased: {itemID}. Remaining Coins: {currentCoins}");
        return true;
    }

    Debug.LogError("Not enough coins to purchase item!");
    return false;
    }

    /// <summary>
    /// Helper check used by your Inventory UI buttons to see if they should display.
    /// </summary>
    public bool CheckIfItemOwned(string itemID)
    {
        return purchasedItemIDs.Contains(itemID);
    }

    public void GoToLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }
}