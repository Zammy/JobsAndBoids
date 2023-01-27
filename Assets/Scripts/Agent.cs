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
        ReflectMovementOnPlaygroundBounds();

        //TODO: steering direction should come from somewhere
        var steeringDirection = transform.forward;
        var steeringForce = steeringDirection * MaxForce;
        var acceleration = steeringForce / Mass;
        var velocity = Vector3.ClampMagnitude(Velocity + acceleration, MaxSpeed);
        transform.position += velocity * Time.deltaTime;
    }

    void ReflectMovementOnPlaygroundBounds()
    {
        var playgroundBounds = Playground.Instance.Size;
        var pos = this.GetPos();
        if (pos.x > playgroundBounds.x && Vector3.Dot(transform.forward, Vector3.left) < 0)
        {
            transform.forward = Vector3.Reflect(transform.forward, Vector3.left);
        }
        if (pos.x < -playgroundBounds.x && Vector3.Dot(transform.forward, Vector3.right) < 0)
        {
            transform.forward = Vector3.Reflect(transform.forward, Vector3.right);
        }
        if (pos.y > playgroundBounds.y && Vector3.Dot(transform.forward, Vector3.back) < 0)
        {
            transform.forward = Vector3.Reflect(transform.forward, Vector3.back);
        }
        if (pos.y < -playgroundBounds.y && Vector3.Dot(transform.forward, Vector3.forward) < 0)
        {
            transform.forward = Vector3.Reflect(transform.forward, Vector3.forward);
        }
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
