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
    public float explosionMultiplier = 3f;

    [Header("Explosion Visualization")]
    public GameObject radiusInfectionPrefab; // Префаб ефекту
    public float infectionLifetime = 1.5f;   // Час життя ефекту


    Rigidbody rb;
    public event Action<Projectile, Collider> OnHit;

    bool fired = false;
    Coroutine lifeCoroutine;

    // Для Gizmos
    Vector3 lastExplosionPos;
    float lastExplosionRadius;

    // Ссилка на пул
    ProjectilePool projectilePool;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Init(float r, ProjectilePool pool = null)
    {
        radius = r;
        transform.localScale = Vector3.one * radius * 2f;
        fired = false;
        rb.isKinematic = true;
        projectilePool = pool;
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
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }

    public void Fire(Vector3 dir)
    {
        fired = true;
        rb.isKinematic = false;
        rb.linearVelocity = dir.normalized * speed;
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
            // викликаємо DoExplosionEffect без передачі obstacle.radius
            DoExplosionEffect(obstacle.transform.position);
        }
        else if (other.TryGetComponent<IHitble>(out var hittable))
        {
            hittable.HitBy(this, other);
        }

        OnHit?.Invoke(this, other);

        ReturnToPool();
    }



    void DoExplosionEffect(Vector3 center)
    {
        // беремо найбільшу компоненту масштабу (щоб врахувати non-uniform scale / lossy scale)
        Vector3 lossy = transform.lossyScale;
        float maxScale = Mathf.Max(lossy.x, Mathf.Max(lossy.y, lossy.z));

        // діаметр = maxScale (бо ти ставиш localScale = Vector3.one * radius * 2f)
        // тому радіус снаряду = maxScale * 0.5f
        float projectileRadius = maxScale * 0.5f;

        // фінальний радіус вибуху — від снаряду, помножений на множник
        float explosionRadius = projectileRadius * explosionMultiplier;

        lastExplosionPos = center;
        lastExplosionRadius = explosionRadius;

        // Створюємо візуальний ефект
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

        // Пошук об'єктів у зоні вибуху
        Collider[] hits = Physics.OverlapSphere(center, explosionRadius, hitLayers);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Obstacle>(out var nearbyObstacle))
            {
                if (Vector3.Distance(center, nearbyObstacle.transform.position) < 0.01f) continue;
                nearbyObstacle.HitBy(this, hit);
            }
        }
    }




    void ReturnToPool()
    {
        fired = false;
        if (lifeCoroutine != null) { StopCoroutine(lifeCoroutine); lifeCoroutine = null; }
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (projectilePool != null)
            projectilePool.Return(this); // повертаємо в свій пул
        else
            gameObject.SetActive(false); // fallback, просто вимикаємо
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
}
