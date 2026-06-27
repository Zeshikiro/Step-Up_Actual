using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) // Left click or Spacebar
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
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