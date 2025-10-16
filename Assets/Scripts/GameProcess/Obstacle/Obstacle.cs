using System.Collections;
using UnityEngine;



public class Obstacle : MonoBehaviour, IHitble
{
    public Transform DownSpawnPos; 
    public float radius = 0.5f;
    public bool exploded = false;

    [Header("Explosion")]
    public GameObject explosionPrefab;
    public float explosionEffectLifetime = 1.5f;

    public ObstaclePool pool;

    // Метод, який викликається при ударі снарядом
    public void HitBy(Projectile projectile, Collider collider)
    {
        // Тут можна додати логіку пошкодження/ефекту
        Debug.Log($"Obstacle {name} hit by projectile {projectile.name}");

        // Наприклад, одразу вибух
        Explode(projectile.radius);
    }

    public void Explode()
    {
        Explode(radius);
    }

    public void Explode(float sourceRadius)
    {
        if (exploded) return;
        exploded = true;
        Debug.Log($"Obstacle {name} exploded with radius {sourceRadius}");

        // Spawn explosion effect
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var effect = fx.GetComponent<ExplosionEffect>();
            if (effect != null)
            {
                effect.lifetime = explosionEffectLifetime;
                effect.Init(sourceRadius);
            }
            else
            {
                fx.transform.localScale = Vector3.one * sourceRadius * 2f;
                Destroy(fx, explosionEffectLifetime);
            }
        }

        // Повернення в пул
        if (pool != null)
        {
            pool.Return(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // Скидання стану для повторного використання в пулі
    public void ResetState()
    {
        exploded = false;
    }
}
