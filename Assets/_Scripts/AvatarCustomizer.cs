using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    [Header("Main UI Pages")]
    public GameObject shopPage;
    public GameObject customizePage;

    [Header("Gender Containers")]
    public GameObject femaleAvatar;
    public GameObject maleAvatar;

    [Header("Female Body Part Renderers")]
    public SkinnedMeshRenderer femaleHead;
    public SkinnedMeshRenderer femaleBody;
    public SkinnedMeshRenderer femaleLegs;
    public SkinnedMeshRenderer femaleFeet;
    public SkinnedMeshRenderer femaleAccessory; // Added

    [Header("Male Body Part Renderers")]
    public SkinnedMeshRenderer maleHead;
    public SkinnedMeshRenderer maleBody;
    public SkinnedMeshRenderer maleLegs;
    public SkinnedMeshRenderer maleFeet;
    public SkinnedMeshRenderer maleAccessory; // Added

    [Header("UI Sub-Panels")]
    public GameObject[] subPanels;

    public void ShowShopPage(bool showShop)
    {
        if (shopPage != null) shopPage.SetActive(showShop);
        if (customizePage != null) customizePage.SetActive(!showShop);
    }

    public void SetGender(bool isMale)
    {
        maleAvatar.SetActive(isMale);
        femaleAvatar.SetActive(!isMale);
    }

    public void OpenPanel(int panelIndex)
    {
        for (int i = 0; i < subPanels.Length; i++)
        {
            subPanels[i].SetActive(i == panelIndex);
        }
    }

    // MESH SWAPPING WITH SAFETY CHECKS
    public void ChangeMeshHead(Mesh newMesh)
    {
        if (maleAvatar.activeSelf && maleHead != null) maleHead.sharedMesh = newMesh;
        else if (femaleHead != null) femaleHead.sharedMesh = newMesh;
    }

    public void ChangeMeshBody(Mesh newMesh)
    {
        if (maleAvatar.activeSelf && maleBody != null) maleBody.sharedMesh = newMesh;
        else if (femaleBody != null) femaleBody.sharedMesh = newMesh;
    }

    public void ChangeMeshLegs(Mesh newMesh)
    {
        if (maleAvatar.activeSelf && maleLegs != null) maleLegs.sharedMesh = newMesh;
        else if (femaleLegs != null) femaleLegs.sharedMesh = newMesh;
    }

    public void ChangeMeshFeet(Mesh newMesh)
    {
        if (maleAvatar.activeSelf && maleFeet != null) maleFeet.sharedMesh = newMesh;
        else if (femaleFeet != null) femaleFeet.sharedMesh = newMesh;
    }

    // Handles items like backpacks cleanly!
    public void ChangeMeshAccessory(Mesh newMesh)
    {
        SkinnedMeshRenderer targetRenderer = maleAvatar.activeSelf ? maleAccessory : femaleAccessory;
        
        if (targetRenderer != null)
        {
            targetRenderer.sharedMesh = newMesh;
            // Automatically turn off the renderer object if no accessory mesh is passed!
            targetRenderer.gameObject.SetActive(newMesh != null);
        }
    }

    // COLOR/MATERIAL CUSTOMIZATION
    public void CustomizeHead(Material newMaterial)
    {
        if (maleAvatar.activeSelf) maleHead.material = newMaterial;
        else femaleHead.material = newMaterial;
    }

    public void CustomizeBody(Material newMaterial)
    {
        if (maleAvatar.activeSelf) maleBody.material = newMaterial;
        else femaleBody.material = newMaterial;
    }

    public void CustomizeLegs(Material newMaterial)
    {
        if (maleAvatar.activeSelf) maleLegs.material = newMaterial;
        else femaleLegs.material = newMaterial;
    }

    public void CustomizeFeet(Material newMaterial)
    {
        if (maleAvatar.activeSelf) maleFeet.material = newMaterial;
        else femaleFeet.material = newMaterial;
    }
}