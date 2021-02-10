using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HierarchyIcons
{
    private static readonly Texture2D TOOLTIP;

    static HierarchyIcons()
    {
        TOOLTIP = AssetDatabase.LoadAssetAtPath("Assets/Tooltip/Gizmos/TooltipTrigger Icon.png", typeof(Texture2D)) as Texture2D;

        if (TOOLTIP == null)
        {
            return;
        } 

        EditorApplication.hierarchyWindowItemOnGUI += DrawTooltipIconOnInspectorWindow;
    }

    private static void DrawTooltipIconOnInspectorWindow(int instanceID, Rect rect)
    {
        if (TOOLTIP == null)
        {
            return;
        }

        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null)
        {
            return;
        }

        Tooltip.TooltipTrigger tooltip = gameObject.GetComponent<Tooltip.TooltipTrigger>();
        if (tooltip == null) return;
        const float iconWidth = 20;
        EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
        Vector2 padding = new Vector2(5, 0);
        Rect iconDrawRect = new Rect(
            rect.xMax - (iconWidth + padding.x), 
            rect.yMin, 
            rect.width, 
            rect.height);
        GUI.color = Color.Lerp(Color.white, Color.red, 0.6f);
        GUIContent iconGUIContent = new GUIContent(TOOLTIP);
        EditorGUI.LabelField(iconDrawRect, iconGUIContent);
        GUI.color = Color.white;
        EditorGUIUtility.SetIconSize(Vector2.zero);
    }
}