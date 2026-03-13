using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // 必须引用
using System.Collections.Generic;

public class EventSystemSpy : MonoBehaviour
{
    void Update()
    {
        if (Cursor.lockState == CursorLockMode.Locked || EventSystem.current == null) return;

        // 🌟 兼容新版 Input System 的坐标获取
        Vector2 mousePos = Mouse.current.position.ReadValue();

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            Debug.Log($"<color=yellow>侦察兵：当前鼠标下是 [{results[0].gameObject.name}]，层级是 {LayerMask.LayerToName(results[0].gameObject.layer)}</color>");
        }
    }
}