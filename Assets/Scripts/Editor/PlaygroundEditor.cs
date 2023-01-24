using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Playground))]
public class FactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var playground = (Playground)target;
        if (GUILayout.Button("Add Agents"))
        {
            playground.GenerateAgents();
        }
    }
}
