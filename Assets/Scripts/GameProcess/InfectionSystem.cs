using UnityEngine;
using System.Collections.Generic;

public class InfectionSystem : MonoBehaviour
{
    public GameplayConfig config;
    const int MAX_OVERLAP = 256;
    Collider[] overlap = new Collider[MAX_OVERLAP];

    void Awake()
    {
        if (Instances.Instance != null) Instances.Instance.Register<InfectionSystem>(this);
    }

    public void HandleProjectileHit(Projectile p, Collider hitCollider)
    {
        var hitObstacle = hitCollider.GetComponent<Obstacle>();
        if (hitObstacle == null) return;

        float infectionRadius = p.radius * config.infectionMultiplier;
        // Find obstacles in radius
        int found = Physics.OverlapSphereNonAlloc(hitCollider.transform.position, infectionRadius, overlap, config.obstacleLayer);
        List<Obstacle> toExplode = new List<Obstacle>(found);
        for (int i = 0; i < found; i++)
        {
            var o = overlap[i].GetComponent<Obstacle>();
            if (o != null && !o.exploded) toExplode.Add(o);
        }

        // Explode all found (simple simultaneous explosion)
        foreach (var o in toExplode) o.Explode();

        // Optional: chain via proximity — simple BFS
        ChainExplode(toExplode, infectionRadius);
    }

    void ChainExplode(List<Obstacle> initial, float baseRadius)
    {
        Queue<Obstacle> q = new Queue<Obstacle>(initial);
        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int found = Physics.OverlapSphereNonAlloc(cur.transform.position, baseRadius, overlap, config.obstacleLayer);
            for (int i = 0; i < found; i++)
            {
                var o = overlap[i].GetComponent<Obstacle>();
                if (o != null && !o.exploded)
                {
                    o.Explode();
                    q.Enqueue(o);
                }
            }
        }
    }
}
