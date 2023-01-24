using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [Header("Settings")]
    public float Mass = 1f;
    public float MaxForce = 10f;
    public float MaxSpeed = 50f;

    [Header("Debug")]
    public Vector3 Velocity;

    void Awake()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {

        // var steeringDirection = transform.forward;
        // //TODO: steering direction should come from somewhere
        // var steeringForce = steeringDirection * MaxForce;
        // var acceleration = steeringForce / Mass;
        // var velocity = Vector3.ClampMagnitude(Velocity + acceleration, MaxSpeed);
        // transform.position += velocity * Time.deltaTime;
    }

    public void Highlight(Color color)
    {
        _renderer.color = color;
    }

    public void RemoveHighlight()
    {
        _renderer.color = Color.white;
    }

    SpriteRenderer _renderer;
}
