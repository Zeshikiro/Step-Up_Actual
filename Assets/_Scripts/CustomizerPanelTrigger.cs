using UnityEngine;

public class CustomizerPanelTrigger : MonoBehaviour
{
    private void OnEnable()
    {
        // Fires automatically every single time the student opens this page panel view
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.GenerateInventoryUI();
            Debug.Log("[UI Initialize] Successfully generated active wardrobe item assets.");
        }
    }
}