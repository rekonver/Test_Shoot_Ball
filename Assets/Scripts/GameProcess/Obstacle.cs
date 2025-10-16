using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : MonoBehaviour, IHitble
{
    public Transform DownSpawnPos;
    public bool exploded = false;
    public float radius = 0.25f; // physical size for overlap checks

    private ObstaclePool pool;

    public void SetPool<T>(ObjectPool<T> pool) where T : MonoBehaviour
    {
        this.pool = pool as ObstaclePool;
    }

    public void Explode()
    {
        if (exploded) return;
        exploded = true;
        Debug.Log($"Obstacle {name} exploded");

        // TODO: spawn particle, sound

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

    public void HitBy(Projectile proj, Collider hitCollider)
    {
        Debug.Log($"{name} was hit by projectile (r={proj.radius})");
        Explode();
    }
}
