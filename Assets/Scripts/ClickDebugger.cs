using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        // Use New Input System safely
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = UnityEngine.InputSystem.Mouse.current.position.ReadValue() };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            if (results.Count > 0)
            {
                Debug.Log($"[Click Debugger] YOU JUST CLICKED ON: {results[0].gameObject.name} (Parent: {results[0].gameObject.transform.parent.name})");
            }
            else
            {
                Debug.Log("[Click Debugger] You clicked on NOTHING. (No Raycast Target was hit)");
            }
        }
    }
}