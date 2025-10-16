using UnityEngine;

public class RadiusInfection : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 1.5f;      // Скільки секунд існує ефект
    public float pulseSpeed = 2f;      // Швидкість пульсації
    public float pulseAmount = 0.1f;   // Наскільки сильно пульсує масштаб

    Vector3 baseScale;
    float timeAlive;
    bool initialized = false;
    float radiusValue = 1f;

    public void Init(float radius)
    {
        if (radius <= 0f) radius = 1f;

        radiusValue = radius;
        baseScale = Vector3.one * radius * 2f; // масштаб під радіус
        transform.localScale = baseScale;

        timeAlive = 0f;
        initialized = true;

        // автоматичне знищення
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!initialized) return;

        timeAlive += Time.deltaTime;

        // Пульсація
        float pulse = 1f + Mathf.Sin(timeAlive * pulseSpeed) * pulseAmount;
        transform.localScale = baseScale * pulse;

        // Згасання кольору (якщо є Renderer)
        var rend = GetComponent<Renderer>();
        if (rend != null && rend.material.HasProperty("_Color"))
        {
            Color c = rend.material.color;
            c.a = Mathf.Lerp(1f, 0f, timeAlive / lifetime);
            rend.material.color = c;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f); // помаранчева прозора сфера
        float drawRadius = radiusValue > 0 ? radiusValue : transform.localScale.x * 0.5f;
        Gizmos.DrawSphere(transform.position, drawRadius);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // контур
        Gizmos.DrawWireSphere(transform.position, drawRadius);
    }
}
