using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Agent))]
public class AgentEditor : Editor
{
    static Agent sLastSelectedAgent;
    static Agent sOtherAgent;

    static Agent[] sOtherAgents;


    static float sRange;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (Playground.Instance == null)
            return;

        if (sLastSelectedAgent != null)
            sLastSelectedAgent.RemoveHighlight();

        sLastSelectedAgent = (Agent)target;
        sLastSelectedAgent.Highlight(Color.green);

        if (GUILayout.Button("Find Closest"))
        {
            if (sOtherAgent != null)
                sOtherAgent.RemoveHighlight();
            var other = Playground.Instance.FindClosetsAgentTo(sLastSelectedAgent);
            other.Highlight(Color.red);
            sOtherAgent = other;
        }

        sRange = EditorGUILayout.FloatField(sRange);

        if (GUILayout.Button("Find In Range"))
        {
            if (sOtherAgents != null)
                Array.ForEach(sOtherAgents, a => a.RemoveHighlight());

            Debug.Log(sRange);
            sOtherAgents = Playground.Instance.FindInRegion(sLastSelectedAgent, sRange);
            Array.ForEach(sOtherAgents, a => a.Highlight(Color.yellow));
        }
    }
}
