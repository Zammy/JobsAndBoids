using System.Collections.Generic;
using UnityEngine;

public class Playground : Monotone<Playground>
{
    public int SpawnNum = 100;
    public Vector2 Size;
    public bool RandomLeadership = true;
    public int LeadersPer100 = 0;
    public GameObject AgentPrefab;

    public Agent.Settings AgentSettings;

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
        for (int i = 0; i < _agents.Count; i++)
        {
            _agents[i].Tick();
        }
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
            if (RandomLeadership)
            {
                newAgent.Leadership = Random.Range(0f, 1f);
            }
            else if ((i % 100) < LeadersPer100)
            {
                newAgent.Leadership = Random.Range(.9f, 1f);
            }
            else
            {
                newAgent.Leadership = Random.Range(0f, .3f);
            }
            newAgent.transform.localRotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
            _agents.Add(newAgent);
        }
    }

    public void RandomizePositionsOfAgents()
    {
        for (int i = 0; i < _agents.Count; i++)
        {
            var agent = _agents[i];
            agent.transform.position = new Vector3(Random.Range(-Size.x, Size.x), 0f, Random.Range(-Size.y, Size.y));
            agent.transform.localRotation = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up);
        }
    }

    public List<Agent> QueryAgentNeighbourhood(Agent agent)
    {
        return _qt.QueryAgentNeighbourhood(agent, AgentSettings.NeighbourhoodRange);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(Size.x * 2, 0.01f, Size.y * 2));
    }

    readonly Rect _labelRect = new Rect(12, 12, 200, 100);
    void OnGUI()
    {
        GUI.Label(_labelRect, $"Agents:{_agents.Count} - Frame:{Time.deltaTime * 1000f:0.00}ms");
    }

    List<Agent> _agents;

    RegionQT<Agent> _qt;
}
