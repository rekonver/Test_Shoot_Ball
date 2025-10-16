using UnityEngine;
using System.Collections.Generic;

public class SimpleObjectPool : MonoBehaviour
{
    public GameObject prefab;
    public int initialSize = 10;

    Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }

        if (Instances.Instance != null)
            Instances.Instance.Register<SimpleObjectPool>(this);
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
            Debug.Log($"Pool.Get -> dequeued {go.name}. poolSize(after)={pool.Count}");
            go.transform.SetPositionAndRotation(pos, rot);
            go.transform.SetParent(null);
            go.SetActive(true);
        }
        else
        {
            go = Instantiate(prefab, pos, rot);
            Debug.Log($"Pool.Get -> instantiated new {go.name}");
        }

        // safety: ensure projectile state reset on take
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.ResetState(); // ensure fired=false, no coroutines, etc.
        }

        return go;
    }

    public void Return(GameObject go)
    {
        if (go == null) return;

        // Reset projectile-specific state if available
        var proj = go.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.ResetState();
        }

        Debug.Log($"Pool.Return -> returning {go.name}. poolSize(before)={pool.Count}");
        go.SetActive(false);
        go.transform.SetParent(transform);
        pool.Enqueue(go);
        Debug.Log($"Pool.Return -> poolSize(after)={pool.Count}");
    }
}
