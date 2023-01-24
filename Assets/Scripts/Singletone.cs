using UnityEngine;

public class Singleton<T> where T : class, new()
{
    static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }
    }
}

public class Monotone<T> : MonoBehaviour
    where T : Component
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = GetComponent<T>();
    }
}