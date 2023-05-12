using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/**
 *  @brief  : Grphic Component의 RaycastTarget 설정이 가능한 토글박스를 Hierachy에 표시.
 *  @ref    : https://baba-s.hatenablog.com/entry/2019/04/01/092000
 */
public static class GraphicRaycastTargetToggle
{
    private const int WIDTH = 16;

    [InitializeOnLoadMethod]
    private static void RayTargetView()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
    }

    private static void OnGUI(int instanceID, Rect selectionRect)
    {
        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go == null) return;

        var com = go.GetComponent<Graphic>();

        if (com == null) return;

        var pos = selectionRect;
        pos.x = pos.xMax - WIDTH;
        pos.width = WIDTH;

        var raycastTarget = GUI.Toggle(pos, com.raycastTarget, string.Empty);

        if (raycastTarget == com.raycastTarget) return;

        com.raycastTarget = raycastTarget;
    }
}
