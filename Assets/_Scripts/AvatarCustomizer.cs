using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AvatarCustomizer : MonoBehaviour
{
    [Header("Main UI Pages")]
    public GameObject shopPage;
    public GameObject customizePage;

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

    private void Start()
    {
        InitializeAvatarState();
        UpdateCoinDisplay();
        ToggleShopPanel(false);
    }

    public void InitializeAvatarState()
    {
        bool isMale = maleAvatar != null && maleAvatar.activeSelf;
        ResetToDefaults(isMale);
    }

    public void ResetToDefaults(bool isMale)
    {
        DeactivateCurrentSet();

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

    // Fixed: Public and explicit context recognition
    public void UpdateCoinDisplay()
    {
        if (coinTextDisplayTMP != null) coinTextDisplayTMP.text = currentCoins.ToString();
        if (coinTextDisplay != null) coinTextDisplay.text = currentCoins.ToString();
    }

    // Route A Equip Endpoints
    public void EquipHeadObject(GameObject newHeadObject)
    {
        if (currentHead != null) currentHead.SetActive(false);
        currentHead = newHeadObject;
        if (currentHead != null) currentHead.SetActive(true);
    }

    public void EquipBodyObject(GameObject newBodyObject)
    {
        if (currentBody != null) currentBody.SetActive(false);
        currentBody = newBodyObject;
        if (currentBody != null) currentBody.SetActive(true);
    }

    public void EquipLegsObject(GameObject newLegsObject)
    {
        if (currentLegs != null) currentLegs.SetActive(false);
        currentLegs = newLegsObject;
        if (currentLegs != null) currentLegs.SetActive(true);
    }

    public void EquipFeetObject(GameObject newFeetObject)
    {
        if (currentFeet != null) currentFeet.SetActive(false);
        currentFeet = newFeetObject;
        if (currentFeet != null) currentFeet.SetActive(true);
    }

    public void EquipAccessoryObject(GameObject newAccessoryObject)
    {
        if (currentAccessory != null) currentAccessory.SetActive(false);
        currentAccessory = newAccessoryObject;
        if (currentAccessory != null) currentAccessory.SetActive(true);
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
            // Fixed: Explicit type usage for Unity 6 compatibility
            InventoryItemButton[] invButtons = Object.FindObjectsByType<InventoryItemButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var btn in invButtons) btn.RefreshVisibility();
        }
    }

    public void SetGender(bool isMale)
    {
        if (maleAvatar != null) maleAvatar.SetActive(isMale);
        if (femaleAvatar != null) femaleAvatar.SetActive(!isMale);
        
        ResetToDefaults(isMale);
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

    public void GoToLoginScene()
    {
        SceneManager.LoadScene("LoginScene");
    }
}