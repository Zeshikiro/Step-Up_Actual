using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [Header("Gender Containers")]
    [Tooltip("Drag the Male avatar parent object here")]
    public GameObject maleAvatar;
    [Tooltip("Drag the Female avatar parent object here")]
    public GameObject femaleAvatar;

    void OnEnable()
    {
        InventoryManager.OnAvatarEquipmentsChanged += LoadSavedAvatar;
    }

    void OnDisable()
    {
        InventoryManager.OnAvatarEquipmentsChanged -= LoadSavedAvatar;
    }

    void Start()
    {
        LoadSavedAvatar();
    }

    public void LoadSavedAvatar()
    {
        // 1. Safety check
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[AvatarLoader] No InventoryManager found! Spawning default.");
            return;
        }

        // 2. Read the global gender choice
        bool isMale = InventoryManager.Instance.isMaleAvatar;

        // 3. HARD OPTIMIZATION: Completely disable the unused gender to save memory and draw calls
        if (maleAvatar != null) maleAvatar.SetActive(isMale);
        if (femaleAvatar != null) femaleAvatar.SetActive(!isMale);

        GameObject activeAvatarRoot = isMale ? maleAvatar : femaleAvatar;
        if (activeAvatarRoot == null) return;

        // 4. HARD OPTIMIZATION: Instantly turn off EVERY mesh first. 
        // This guarantees no overlapping clothing or hidden meshes are secretly rendering and eating battery!
        DeactivateAllMeshes(activeAvatarRoot.transform);

        // 5. Turn ON only the exact 5 equipped pieces using their actual Mesh Names, NOT their Item IDs!
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedHeadId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedBodyId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedLegsId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedFeetId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedAccessoryId));
    }

    private void DeactivateAllMeshes(Transform root)
    {
        // Find all 3D meshes (clothing, hair, bodies) and turn them off.
        // We do NOT turn off the Armature/Bones, only the visual meshes!
        foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            smr.gameObject.SetActive(false);
        }
        
        foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            mr.gameObject.SetActive(false);
        }
    }

    private void EquipMesh(Transform root, string meshName)
    {
        if (string.IsNullOrEmpty(meshName) || meshName == "None") return;

        Transform meshTransform = FindChildRecursive(root, meshName);
        if (meshTransform != null)
        {
            meshTransform.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[AvatarLoader] Optimization Alert: Could not find equipped mesh: {meshName}. Make sure it is inside the Avatar Prefab!");
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
}
