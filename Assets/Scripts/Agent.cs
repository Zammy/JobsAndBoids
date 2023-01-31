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
        public float SeparationWeight = 1f;
        public float CohesionWeight = 1f;
        public float AlignmentWeight = 1f;
        public float ContainmentWeight = 10f;
    }

    public float Leadership;

    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        Highlight(Color.Lerp(Color.blue, Color.green, Leadership));
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
    
    SpriteRenderer _renderer;
}
