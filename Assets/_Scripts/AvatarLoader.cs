using UnityEngine;

public class AvatarLoader : MonoBehaviour
{
    [Header("Avatar Container")]
    [Tooltip("Drag the single unified AvatarContainer here")]
    public GameObject avatarContainer;

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

        // We now use a single unified Avatar Container
        GameObject activeAvatarRoot = avatarContainer; 
        
        if (activeAvatarRoot != null) activeAvatarRoot.SetActive(true);

        if (activeAvatarRoot == null) return;

        // NEW: Automatically fix massive FBX position offsets by shifting the root!
        CenterAvatar(activeAvatarRoot.transform);

        // NEW: Automatically stitch/rebind any broken clothing meshes to the active skeleton!
        AutoRebindBones(activeAvatarRoot.transform);

        // NEW: Force the AvatarAnimatorSync script to attach so we NEVER have broken animations!
        if (activeAvatarRoot.GetComponent<AvatarAnimatorSync>() == null)
        {
            activeAvatarRoot.AddComponent<AvatarAnimatorSync>();
            Debug.Log("[AvatarLoader] Dynamically attached AvatarAnimatorSync to fix missing script!");
        }

        // 4. HARD OPTIMIZATION: Instantly turn off EVERY mesh first. 
        DeactivateAllMeshes(activeAvatarRoot.transform);

        // 5. Turn ON only the exact 5 equipped pieces using their actual Mesh Names
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedHeadId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedBodyId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedLegsId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedFeetId));
        EquipMesh(activeAvatarRoot.transform, InventoryManager.Instance.GetMeshNameFromItemId(InventoryManager.Instance.equippedAccessoryId));
    }

    private void CenterAvatar(Transform root)
    {
        // 2. Find ALL skeletons in the hierarchy and fix their massive Blender offsets!
        Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "CharacterArmature" && t.parent != null)
            {
                // Shift the immediate parent backwards to exactly cancel out the armature's imported offset.
                // This guarantees every single skeleton perfectly collapses to exactly (0,0,0)!
                t.parent.localPosition = -t.localPosition;
            }
        }

        // Restore the camera to perfectly track the AvatarPivot (which is at 0,0,0 with 0,0,0 rotation).
        // (Tracking the imported armatures caused the camera offsets to break due to their 90-degree Blender rotations!)
        Camera previewCam = GameObject.Find("previewcamera")?.GetComponent<Camera>();
        if (previewCam != null)
        {
            AvatarPreviewCamera previewScript = previewCam.GetComponent<AvatarPreviewCamera>();
            if (previewScript != null)
            {
                previewScript.enabled = true;
                Transform pivot = GameObject.Find("AvatarPivot")?.transform;
                if (pivot != null) previewScript.targetAvatar = pivot;
            }
        }
    }

    private void AutoRebindBones(Transform root)
    {
        // Find the main skeleton (Armature) recursively 
        Transform armature = FindChildRecursive(root, "CharacterArmature");
        if (armature == null) 
        {
            Debug.LogWarning($"[AvatarLoader] CRITICAL: Could not find main CharacterArmature anywhere under {root.name}!");
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
            // Prevent Unity from aggressively culling the mesh when the camera looks at the bones!
            // Because the FBX folders have massive offsets, Unity's bounds calculations are completely wrong.
            // Setting this permanently prevents the "blue skybox" invisible mesh bug!
            smr.updateWhenOffscreen = true;

            Debug.LogWarning($"[AvatarLoader] Restitched {smr.name}! Bones Matched: {matchedBones}/{smr.bones.Length}.");
        }
    }

    private void DeactivateAllMeshes(Transform root)
    {
        foreach (SkinnedMeshRenderer smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            // Protect the root container and structural folders from disabling themselves!
            if (smr.gameObject != root.gameObject && smr.gameObject.name != "Male_Casual")
            {
                smr.gameObject.SetActive(false);
            }
        }
        
        foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>(true))
        {
            if (mr.gameObject != root.gameObject && mr.gameObject.name != "Male_Casual")
            {
                mr.gameObject.SetActive(false);
            }
        }
    }

    private void EquipMesh(Transform root, string meshName)
    {
        if (string.IsNullOrEmpty(meshName) || meshName == "None") return;

        Transform target = FindChildRecursive(root, meshName);
        if (target != null)
        {
            // Turn on the specific mesh
            target.gameObject.SetActive(true);
            
            // CRITICAL: Also force its immediate parent folder (like "Casual_2") to be ON!
            // Unity sometimes attaches hidden MeshRenderers to FBX parent folders. If DeactivateAllMeshes 
            // accidentally turned off the "Casual_2" folder, this guarantees it gets turned back on so the mesh is visible!
            if (target.parent != null && target.parent != root)
            {
                target.parent.gameObject.SetActive(true);
            }

            SkinnedMeshRenderer smr = target.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
            {
                Debug.LogWarning($"[AvatarLoader] VICTORY DIAGNOSTIC: {meshName} is Active! World Pos: {target.position} | World Scale: {target.lossyScale} | Vertices: {smr.sharedMesh.vertexCount}");
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
