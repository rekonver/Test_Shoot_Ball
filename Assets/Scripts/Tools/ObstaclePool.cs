using UnityEngine;

public class ObstaclePool : ObjectPool<Obstacle>
{
    protected override void RegisterPool()
    {
        if (Instances.Instance != null)
        {
            Instances.Instance.Register<ObstaclePool>(this);
        }
    }

    protected override void ResetObject(Obstacle obj)
    {
        obj.exploded = false;
        obj.gameObject.SetActive(false);
        // при потребі можна скинути Transform або інші властивості
    }
}
