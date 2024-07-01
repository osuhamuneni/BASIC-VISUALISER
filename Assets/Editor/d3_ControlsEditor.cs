using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(d3_Controls))]
public class d3_ControlsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        d3_Controls d3Controls = (d3_Controls)target;

        GUILayout.Space(10);

        GUILayout.Label("Toggle Colours", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("RGB Channel", GUILayout.Width(100)))
        {
            d3Controls.ToggleColours(0);
        }
        if (GUILayout.Button("R channel", GUILayout.Width(100)))
        {
            d3Controls.ToggleColours(1);
        }
        //GUILayout.EndHorizontal();
        //GUILayout.BeginHorizontal();
        if (GUILayout.Button("G Channel", GUILayout.Width(100)))
        {
            d3Controls.ToggleColours(2);
        }
        if (GUILayout.Button("B Channel", GUILayout.Width(100)))
        {
            d3Controls.ToggleColours(3);
        }
        GUILayout.EndHorizontal();
    }
}
