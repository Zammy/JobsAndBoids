using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [System.Serializable]
    public class Settings
    {
        public float Speed = 10f;

        [Range(0f, 1f)]
        public float RotationCoefficient = 1f;

        public float NeighbourhoodRange = 15f;

        public float ForwardWeight = 1f;
        public float RepulsionWeight = 1f;
        public float CohesionWeight = 1f;
        public float AlignmentWeight = 1f;
        public float ContainmentWeight = 10f;
    }

    [Header("Debug")]
    public Vector3 FlockCenter;
    public float Leadership;
    public Agent Leader;

    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        Highlight(Color.Lerp(Color.blue, Color.green, Leadership));
    }

    public void Tick()
    {
        var settings = Playground.Instance.AgentSettings;

        var neighbourhood = Playground.Instance.QueryAgentNeighbourhood(this);

        var repulsionForce = CalculateSeparationForce(neighbourhood);
        CalculateFlockCenter(neighbourhood);
        var cohesionForce = CalculateCohesionForce(neighbourhood);

        Vector3 alignmentForce = Vector3.zero;
        if (Leader)
            alignmentForce = Leader.transform.forward;

        var containmentForce = PlaygroundContainmentForce();

        float reverseLeadership = (1f - Leadership);
        _desiredDirection = transform.forward * settings.ForwardWeight
                + repulsionForce * settings.RepulsionWeight
                + cohesionForce * settings.CohesionWeight * reverseLeadership
                + alignmentForce * settings.AlignmentWeight * reverseLeadership
                + containmentForce * settings.ContainmentWeight;
        _desiredDirection.Normalize();

        transform.forward = Vector3.Lerp(transform.forward, _desiredDirection, settings.RotationCoefficient);

        var velocity = _desiredDirection * settings.Speed;
        transform.position += velocity * Time.deltaTime;
    }

    Vector3 CalculateSeparationForce(List<Agent> neighbourhood)
    {
        var pos = transform.position;
        var repulsionForce = Vector3.zero;
        for (int i = 0; i < neighbourhood.Count; i++)
        {
            var neighbourAgent = neighbourhood[i];
            var diff = pos - neighbourAgent.transform.position;
            repulsionForce += diff * 1 / diff.sqrMagnitude;
        }
        return repulsionForce;
    }

    void CalculateFlockCenter(List<Agent> neighbourhood)
    {
        Vector3 flockCenter = transform.position;
        for (int i = 0; i < neighbourhood.Count; i++)
        {
            var neighbour = neighbourhood[i];
            flockCenter += neighbour.transform.position;
        }
        flockCenter /= neighbourhood.Count + 1;
        FlockCenter = flockCenter;
    }

    Vector3 CalculateCohesionForce(List<Agent> neighbourhood)
    {
        Vector3 flockCenter = transform.position;
        float highestLeadership = 0f;
        for (int i = 0; i < neighbourhood.Count; i++)
        {
            Agent neighbour = neighbourhood[i];
            var diff = neighbour.transform.position - transform.position;
            float leadership = neighbour.Leadership / Mathf.Lerp(1f, 5f, diff.magnitude / 15f);
            if (leadership > highestLeadership)
            {
                highestLeadership = leadership;
                flockCenter = neighbour.FlockCenter;
                Leader = neighbour;
            }
        }

        return flockCenter - transform.position;
    }

    Vector3 PlaygroundContainmentForce()
    {
        var playgroundBounds = Playground.Instance.Size;
        var pos = transform.position;
        Vector3 force = Vector3.zero;
        if (pos.x > playgroundBounds.x)
        {
            force += Vector3.left;
        }
        if (pos.x < -playgroundBounds.x)
        {
            force += Vector3.right;
        }
        if (pos.z > playgroundBounds.y)
        {
            force += Vector3.back;
        }
        if (pos.z < -playgroundBounds.y)
        {
            force += Vector3.forward;
        }
        return force;
    }

    public void Highlight(Color color)
    {
        if (_renderer)
            _renderer.color = color;
    }

    public void RemoveHighlight()
    {
        if (_renderer)
            _renderer.color = Color.white;
    }

    void OnDrawGizmos()
    {
        var prevColor = Gizmos.color;

        Gizmos.color = Color.yellow;
        if (Leader)
            Gizmos.DrawLine(transform.position, Leader.transform.position);
        Gizmos.color = prevColor;
    }

    Vector3 _desiredDirection;
    SpriteRenderer _renderer;
}
