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
    public float animationTimeout = 1f; 
    public Animator animator;
    public ObstaclePool pool;
    [SerializeField] private bool GodMod = false;

    public void HitBy(Projectile projectile, Collider collider)
    {
        //Debug.Log($"Obstacle {name} hit by projectile {projectile.name}");
        if (GodMod) return;
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
        //Debug.Log($"Obstacle {name} triggered explode with radius {sourceRadius}");

        StartCoroutine(ExplodeCoroutine(sourceRadius));
    }

    private IEnumerator ExplodeCoroutine(float sourceRadius)
    {
        if (animator != null) animator.SetTrigger("Explode");
        var col = GetComponent<Collider>();
        
        if (col != null) col.enabled = false;

        // Чекаємо animationTimeout
        yield return new WaitForSeconds(animationTimeout);


        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var effect = fx.GetComponent<ExplosionEffect>();
            if (effect != null)
            {
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
