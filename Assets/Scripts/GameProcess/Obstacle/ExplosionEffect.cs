using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ExplosionEffect : MonoBehaviour
{
    [Header("Timing")]
    public float lifetime = 1.5f;
    public float expandDuration = 0.15f; // час наростання до повного розміру

    [Header("Visuals")]
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // форма розширення
    public bool fadeRenderer = true; // згасання матеріалу за час життя

    [Header("Audio")]
    public AudioClip playOnSpawn;
    [Range(0f,1f)] public float volume = 1f;

    float targetRadius = 1f;
    Vector3 targetScale = Vector3.one;
    float timeAlive = 0f;

    /// <summary>
    /// Ініціалізація ефекту — викликається одразу після Instantiate.
    /// radius — радіус у одиницях світу (не діаметр).
    /// </summary>
    public void Init(float radius)
    {
        if (radius <= 0f) radius = 1f;
        targetRadius = radius;
        targetScale = Vector3.one * targetRadius * 2f; // діаметр
        transform.localScale = Vector3.zero;
        timeAlive = 0f;

        // AudioSource (можна використовувати PlayClipAtPoint замість локального джерела)
        if (playOnSpawn != null)
        {
            var src = GetComponent<AudioSource>();
            if (src != null)
            {
                src.clip = playOnSpawn;
                src.spatialBlend = 1f;
                src.volume = volume;
                src.Play();
            }
            else
            {
                AudioSource.PlayClipAtPoint(playOnSpawn, transform.position, volume);
            }
        }

        // Якщо є ParticleSystem на префабі — запустити його
        var ps = GetComponentInChildren<ParticleSystem>();
        if (ps != null) ps.Play();

        // Запускаємо coroutine, який керує розширенням і хованням
        StartCoroutine(LifeCoroutine());
    }

    IEnumerator LifeCoroutine()
    {
        // expand
        float t = 0f;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float k = expandCurve.Evaluate(Mathf.Clamp01(t / expandDuration));
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, k);
            yield return null;
        }
        transform.localScale = targetScale;

        // тримаємо на повному розмірі і одночасно згасаємо (за need)
        while (timeAlive < lifetime)
        {
            timeAlive += Time.deltaTime;

            if (fadeRenderer)
            {
                var rend = GetComponent<Renderer>();
                if (rend != null && rend.material.HasProperty("_Color"))
                {
                    Color c = rend.material.color;
                    c.a = Mathf.Lerp(1f, 0f, timeAlive / lifetime);
                    rend.material.color = c;
                }
                else
                {
                    // пошук в дочірніх рендерах
                    var rends = GetComponentsInChildren<Renderer>();
                    foreach (var r in rends)
                    {
                        if (r.material.HasProperty("_Color"))
                        {
                            Color c = r.material.color;
                            c.a = Mathf.Lerp(1f, 0f, timeAlive / lifetime);
                            r.material.color = c;
                        }
                    }
                }
            }

            yield return null;
        }

        // Очищення: знищуємо префаб (або можна повернути в пул якщо треба)
        Destroy(gameObject);
    }

    // Для редактора — показати радіус дії
    void OnDrawGizmosSelected()
    {
        float drawRadius = targetRadius > 0f ? targetRadius : transform.localScale.x * 0.5f;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.18f);
        Gizmos.DrawSphere(transform.position, drawRadius);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, drawRadius);
    }
}
