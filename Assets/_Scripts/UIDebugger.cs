using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current == null)
            {
                Debug.LogError("NO EVENT SYSTEM FOUND IN SCENE!");
                return;
            }

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                Debug.Log("<color=red><b>[UI CLICKED] -> </b></color> " + results[0].gameObject.name + " | <color=yellow>(Parent: " + (results[0].gameObject.transform.parent != null ? results[0].gameObject.transform.parent.name : "None") + ")</color>");
            }
            else
            {
                Debug.Log("<color=orange><b>[UI CLICKED] -> NOTHING!</b></color> (Either the EventSystem is broken, or no UI has Raycast Target enabled here.)");
            }
        }
    }
}
