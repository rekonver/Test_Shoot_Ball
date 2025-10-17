using System;
using System.Collections.Generic;
using UnityEngine;

public class Instances : MonoBehaviour
{
    public static Instances Instance { get; private set; }

    [Header("Optional scene instances")]
    public LevelManager levelManager;
    public PlayerController playerController;
    public Door door;
    public GameplayConfig config;

    [Header("Pool container")]
    public GameObject poolsContainer;

    private Dictionary<Type, object> map = new Dictionary<Type, object>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        RegisterDefaults();

        if (poolsContainer != null)
            RegisterPoolsFromContainer(poolsContainer);
    }

    void RegisterDefaults()
    {
        if (levelManager != null) Register<LevelManager>(levelManager);
        if (playerController != null) Register<PlayerController>(playerController);
        if (door != null) Register<Door>(door);
        if (config != null) Register<GameplayConfig>(config);
    }

    private void RegisterPoolsFromContainer(GameObject container)
    {
        var poolComponents = container.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in poolComponents)
        {
            var type = mb.GetType();
            if (IsObjectPool(type))
            {
                RegisterPoolByType(mb, type);
            }
        }
    }

    private bool IsObjectPool(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(ObjectPool<>))
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }

    private void RegisterPoolByType(MonoBehaviour mb, Type type)
    {
        if (!map.ContainsKey(type))
        {
            map[type] = mb;
            Debug.Log($"Instances: auto-registered pool {type.Name}");
        }
    }

    public void Register<T>(T instance) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        map[typeof(T)] = instance;
    }

    public bool TryGet<T>(out T instance) where T : class
    {
        if (map.TryGetValue(typeof(T), out var o))
        {
            instance = o as T;
            return instance != null;
        }
        instance = null;
        return false;
    }

    public T Get<T>() where T : class
    {
        if (TryGet<T>(out var inst)) return inst;
        Debug.LogWarning($"Instances: no registered instance for {typeof(T).Name}");
        return null;
    }

    public void Unregister<T>() where T : class
    {
        map.Remove(typeof(T));
    }

    public void ClearAll()
    {
        map.Clear();
    }
}
