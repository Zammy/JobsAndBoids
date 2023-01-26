using System.Collections.Generic;
using UnityEngine;

public class Playground : Monotone<Playground>
{
    public int SpawnNum = 100;
    public Vector2 Size;
    public GameObject AgentPrefab;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        _agents = new List<Agent>(SpawnNum);
        _qt = new RegionQT<Agent>();
        GenerateAgents();
    }

    void Update()
    {
        _qt.Build(_agents);
    }

    public void GenerateAgents()
    {
        if (!Application.isPlaying)
            return;

        for (int i = 0; i < SpawnNum; i++)
        {
            var pos = new Vector3(Random.Range(-Size.x, Size.x), 0f, Random.Range(-Size.y, Size.y));
            var newAgentGo = Instantiate(AgentPrefab, pos, Quaternion.identity, transform);
            var newAgent = newAgentGo.GetComponent<Agent>();
            _agents.Add(newAgent);
        }
        _qt.Build(_agents);
    }

    public Agent FindClosetsAgentTo(Agent agent)
    {
        return _qt.FindClosest(agent);
    }

    public Agent[] FindInRegion(Agent agent, float radius)
    {
        return _qt.AllInRegion(agent.GetPos(), radius);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(Size.x * 2, 0.01f, Size.y * 2));

        //if (_agentsSortedX == null)
        //    return;

        //for (int i = 0; i < _agentsSortedX.Count; i++)
        //{
        //    var agent = _agentsSortedX[i];
        //    Handles.Label(agent.transform.position, i.ToString());
        //}
    }

    List<Agent> _agents;

    RegionQT<Agent> _qt;
}
