using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    public float radius { get; private set; } = 0.1f;
    public float maxLifeTime = 4f;


    [Header("Explosion Visualization")]
    public GameObject radiusInfectionPrefab;
    public float infectionLifetime = 1.5f;
    private float speed = 12f;
    private float explosionMultiplier = 3f;
    private LayerMask hitLayers;

    Rigidbody rb;
    Collider col;
    public event Action<Projectile, Collider> OnHit;

    bool fired = false;
    Coroutine lifeCoroutine;


    Vector3 lastExplosionPos;
    float lastExplosionRadius;

    ProjectilePool projectilePool;
    GameplayConfig config;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;


        col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void SetVelues()
    {
        config = Instances.Instance.Get<GameplayConfig>();
        speed = config.projectileSpeed;
        explosionMultiplier = config.explosionMultiplier;
        hitLayers = config.obstacleLayer;
    }

    public void Init(float r, ProjectilePool pool = null)
    {
        SetVelues();
        radius = r;
        transform.localScale = Vector3.one * radius * 2f;
        fired = false;
        projectilePool = pool;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
        if (col != null)
        {
            col.enabled = false;
        }

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
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

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
        if (col != null)
        {
            col.enabled = false;
        }
    }


    public void Fire(Vector3 dir)
    {
        if (fired) return;
        fired = true;

        if (col != null) col.enabled = true;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = dir.normalized * speed;
        }

        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(AutoReturnCoroutine(maxLifeTime));
    }


    void OnTriggerEnter(Collider other)
    {
        if (!fired) return;

        if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent<Obstacle>(out var obstacle))
        {
            obstacle.HitBy(this, other);
            DoExplosionEffect(obstacle.transform.position);
        }
        else if (other.TryGetComponent<IHitble>(out var hittable))
        {
            hittable.HitBy(this, other);
            DoExplosionEffect(other.ClosestPoint(transform.position));
        }
        else
        {
            DoExplosionEffect(other.ClosestPoint(transform.position));
        }

        OnHit?.Invoke(this, other);

        ReturnToPool();
    }

    void DoExplosionEffect(Vector3 center)
    {
        Vector3 lossy = transform.lossyScale;
        float maxScale = Mathf.Max(lossy.x, Mathf.Max(lossy.y, lossy.z));
        float projectileRadius = maxScale * 0.5f;
        float explosionRadius = projectileRadius * explosionMultiplier;

        lastExplosionPos = center;
        lastExplosionRadius = explosionRadius;

        if (radiusInfectionPrefab != null)
        {
            GameObject effect = Instantiate(radiusInfectionPrefab, center, Quaternion.identity);
            var infection = effect.GetComponent<RadiusInfection>();
            if (infection != null)
            {
                infection.Init(explosionRadius);
                infection.lifetime = infectionLifetime;
            }
            else
            {
                effect.transform.localScale = Vector3.one * explosionRadius * 2f;
                Destroy(effect, infectionLifetime);
            }
        }

        Collider[] hits = Physics.OverlapSphere(center, explosionRadius, hitLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Obstacle>(out var nearbyObstacle))
            {
                nearbyObstacle.HitBy(this, hit);
            }
        }
    }


    void ReturnToPool()
    {
        fired = false;
        if (lifeCoroutine != null) { StopCoroutine(lifeCoroutine); lifeCoroutine = null; }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (col != null)
            col.enabled = false;


        if (projectilePool != null)
            projectilePool.Return(this);
        else
            gameObject.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (lastExplosionRadius > 0)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(lastExplosionPos, lastExplosionRadius);
        }
    }

    IEnumerator AutoReturnCoroutine(float t)
    {
        yield return new WaitForSeconds(t);
        if (!gameObject.activeInHierarchy) yield break;
        ReturnToPool();
    }

    public void SetPool(ProjectilePool pool) => projectilePool = pool;
}
