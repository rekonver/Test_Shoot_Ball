using UnityEngine;
public class ProjectilePool : ObjectPool<Projectile>
{
    protected override void RegisterPool()
    {
        if (Instances.Instance != null)
            Instances.Instance.Register<ProjectilePool>(this);
    }

    protected override void ResetObject(Projectile obj)
    {
        obj.ResetState();
    }
}