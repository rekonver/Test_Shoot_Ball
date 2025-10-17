using UnityEngine;

public class RadiusInfection : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 1.5f;   

    float radiusValue = 1f;

    public void Init(float radius)
    {
        if (radius <= 0f) radius = 1f;

        radiusValue = radius;

        Destroy(gameObject, lifetime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        float drawRadius = radiusValue > 0 ? radiusValue : transform.localScale.x * 0.5f;
        Gizmos.DrawSphere(transform.position, drawRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, drawRadius);
    }
}
