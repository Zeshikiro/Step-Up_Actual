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

        // NEW: Automatically stitch/rebind any broken clothing meshes to the active skeleton!
        AutoRebindBones(activeAvatarRoot.transform);

        // 4. HARD OPTIMIZATION: Instantly turn off EVERY mesh first. 
        DeactivateAllMeshes(activeAvatarRoot.transform);

        // 5. Turn ON only the exact 5 equipped pieces using their actual Mesh Names
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedHeadId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedBodyId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedLegsId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedFeetId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedAccessoryId));
    }

    private void AutoRebindBones(Transform root)
    {
        // Find the main skeleton (Armature) that is a DIRECT child of this character
        Transform armature = root.Find("CharacterArmature");
        if (armature == null) 
        {
            Debug.LogWarning($"[AvatarLoader] CRITICAL: Could not find main CharacterArmature directly under {root.name}!");
            return;
        }

        // Get all available bones in the MAIN skeleton
        Transform[] allBones = armature.GetComponentsInChildren<Transform>(true);

        // Scan every piece of clothing in this character
        foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            // If the clothing has no bones, or is already bound to the MAIN skeleton, skip it
            if (smr.bones == null || smr.bones.Length == 0) continue;
            if (smr.rootBone != null && smr.rootBone.IsChildOf(armature)) continue;

            // Otherwise, this clothing came from a different FBX! We must stitch it to our bones!
            Transform[] newBones = new Transform[smr.bones.Length];
            int matchedBones = 0;
            
            for (int i = 0; i < smr.bones.Length; i++)
            {
                if (smr.bones[i] == null) continue;
                string targetBoneName = smr.bones[i].name;

                // Find the matching bone in OUR skeleton
                foreach (Transform bone in allBones)
                {
                    if (bone.name == targetBoneName)
                    {
                        newBones[i] = bone;
                        matchedBones++;
                        break;
                    }
                }
            }

            // Apply the stitched bones!
            smr.bones = newBones;
            smr.rootBone = armature;
            
            // CRITICAL FIXES FOR INVISIBLE MESHES:
            // 1. Force position, rotation, AND scale to be perfect for the mesh itself
            smr.transform.localPosition = Vector3.zero;
            smr.transform.localRotation = Quaternion.identity;
            smr.transform.localScale = Vector3.one;
            
            // 2. Prevent Unity from making it invisible due to broken bounds!
            smr.updateWhenOffscreen = true;

            // 3. THE MAGIC TELEPORTATION & SCALE FIX!
            // Snap the clothing folder exactly to the skeleton's coordinates!
            if (smr.transform.parent != root)
            {
                smr.transform.parent.localPosition = armature.localPosition;
                smr.transform.parent.localRotation = armature.localRotation;
                smr.transform.parent.localScale = armature.localScale;
            }

            Debug.LogWarning($"[AvatarLoader] Restitched {smr.name}! Bones Matched: {matchedBones}/{smr.bones.Length}.");
        }
    }

    private void DeactivateAllMeshes(Transform root)
    {
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
            SkinnedMeshRenderer smr = meshTransform.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
            {
                Debug.LogWarning($"[AvatarLoader] VICTORY DIAGNOSTIC: {meshName} is Active! World Pos: {meshTransform.position} | World Scale: {meshTransform.lossyScale} | Vertices: {smr.sharedMesh.vertexCount}");
            }
        }
        else
        {
            // Dump the ENTIRE hierarchy so we can see what Unity sees!
            string allNames = "";
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                allNames += "[" + t.name + "] ";
            }
            Debug.LogWarning($"[AvatarLoader] CRITICAL ERROR! Searched inside '{root.name}' for '{meshName}'. It is NOT in this list: {allNames}");
        }
    }

    private Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return null;
        
        string cleanTarget = targetName.Trim().ToLower();
        string cleanParent = parent.name.Trim().ToLower();

        if (cleanParent == cleanTarget) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), targetName);
            if (result != null) return result;
        }
        return null;
    }
}
