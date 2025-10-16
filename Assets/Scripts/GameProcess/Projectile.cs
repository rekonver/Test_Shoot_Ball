using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float radius { get; private set; } = 0.1f;
    public float speed = 12f;
    public LayerMask hitLayers;
    public float maxLifeTime = 4f;

    Rigidbody rb;
    public event Action<Projectile, Collider> OnHit;

    bool fired = false;
    Coroutine lifeCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Init(float r)
    {
        radius = r;
        transform.localScale = Vector3.one * radius * 2f;
        fired = false;
        rb.isKinematic = true;
    }

    public void ResetState()
    {
        OnHit = null;
        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
        fired = false;
        // safety: ensure collider enabled
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    public void Fire(Vector3 dir)
    {
        fired = true;
        rb.isKinematic = false;
        rb.linearVelocity = dir.normalized * speed;
        Debug.Log($"{name} fired. dir={dir} speed={speed}");
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(AutoReturnCoroutine(maxLifeTime));
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{name} OnTriggerEnter with {other.name} fired={fired}");
        if (!fired) return;
        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            Debug.Log($"{name} hit wrong layer {LayerMask.LayerToName(other.gameObject.layer)}");
            return;
        }

        if (other.TryGetComponent<IHitble>(out var hittable))
        {
            hittable.HitBy(this, other);
        }

        OnHit?.Invoke(this, other);

        fired = false;
        if (lifeCoroutine != null) { StopCoroutine(lifeCoroutine); lifeCoroutine = null; }
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // disable collider so it won't trigger again while pooled
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var pool = Instances.Instance != null ? Instances.Instance.GetOrFind<SimpleObjectPool>() : null;
        if (pool != null) pool.Return(gameObject);
        else Destroy(gameObject);
    }

    IEnumerator AutoReturnCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        if (!gameObject.activeInHierarchy) yield break;

        Debug.Log($"{name} AutoReturn after {t}s");
        fired = false;
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        var pool = Instances.Instance != null ? Instances.Instance.GetOrFind<SimpleObjectPool>() : null;
        if (pool != null) pool.Return(gameObject);
        else Destroy(gameObject);
        lifeCoroutine = null;
    }
}
