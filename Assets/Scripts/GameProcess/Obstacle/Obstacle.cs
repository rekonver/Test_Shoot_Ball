using System.Collections;
using UnityEngine;

public class Obstacle : MonoBehaviour, IHitble
{
    public Transform DownSpawnPos;
    public ObstaclePool pool;
    public bool exploded = false;
    [SerializeField] private float radius = 0.5f;

    [Header("Explosion")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionEffectLifetime = 1.5f;
    [SerializeField] private float animationTimeout = 1f; 
    [SerializeField] private Animator animator;

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

        pool.Return(this);
    }
}
