using UnityEngine;
using System.Collections.Generic;

// Базовий абстрактний пул
public abstract class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour
{

    [Tooltip("Префаб для пулу")]
    public T prefab;

    [Tooltip("Початковий розмір пулу")]
    public int initialSize = 10;

    protected Queue<T> pool = new Queue<T>();

    protected virtual void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }

        RegisterPool();
    }

    // Реєстрація в Instances
    protected abstract void RegisterPool();

    public virtual T Get(Vector3 pos, Quaternion rot)
    {
        T obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, rot);
        }

        ResetObject(obj);
        return obj;
    }

    public virtual void Return(T obj)
    {
        if (obj == null) return;

        ResetObject(obj);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
    }

    // Метод для скидання стану об’єкта (переопреділяється у дочірніх)
    protected abstract void ResetObject(T obj);
}
