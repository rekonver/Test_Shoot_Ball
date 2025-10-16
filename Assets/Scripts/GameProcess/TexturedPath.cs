using UnityEngine;

public class TexturedPath : MonoBehaviour
{
    [Tooltip("Префаб Plane (опціонально). Якщо null — створиться Primitive Plane (10x10).")]
    public GameObject planePrefab;

    [Tooltip("Початок доріжки (опціонально). Якщо null — використовується цей об'єкт.")]
    public Transform startPoint;

    [Tooltip("Кінець доріжки")]
    public Transform endPoint;

    float playerWidth = 0.5f;

    [Tooltip("Оригінальний розмір Plane у префабі (наприклад Unity Plane = 10)")]
    public float planeOriginalSize = 10f;

    GameObject planeInstance;

    void Start()
    {
        if (Application.isPlaying)
            UpdatePlane();
    }

    float lastPlayerWidth = 1f; // зберігаємо останню ширину гравця

    void Update()
    {
        UpdatePlane();
    }

    void UpdatePlane()
    {
        if (endPoint == null) return;

        // Отримуємо PlayerController через Instances
        var player = Instances.Instance.Get<PlayerController>();
        if (player != null)
        {
            float currentWidth = player.transform.localScale.x;

            // Перевіряємо, чи змінився масштаб
            if (!Mathf.Approximately(currentWidth, lastPlayerWidth))
            {
                playerWidth = currentWidth;
                lastPlayerWidth = currentWidth;

                // Оновлюємо масштаб Plane
                if (planeInstance != null)
                {
                    UpdatePlaneScale();
                }
            }
        }

        if (planeInstance == null)
        {
            if (planePrefab != null)
            {
                planeInstance = Instantiate(planePrefab, transform);
                planeInstance.name = "__PathPlane_Prefab";
                planeInstance.transform.SetParent(transform, false);
            }
            else
            {
                Debug.LogWarning($"[TexturedPath] Не задано Plane Prefab для '{name}'. Доріжка не буде створена.");
                return;
            }
        }

        // позиції
        float planeY = transform.position.y;
        Vector3 start = (startPoint != null ? startPoint.position : transform.position);
        Vector3 end = endPoint.position;
        start.y = planeY;
        end.y = planeY;

        Vector3 dir = end - start;
        float length = dir.magnitude;
        if (length < 0.0001f)
        {
            planeInstance.SetActive(false);
            return;
        }

        planeInstance.SetActive(true);
        Vector3 forward = dir.normalized;
        planeInstance.transform.position = start + dir * 0.5f;
        planeInstance.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        planeInstance.transform.rotation *= Quaternion.Euler(0f, -90f, 0f);

        // оновлюємо масштаб
        UpdatePlaneScale();
    }

    void UpdatePlaneScale()
    {
        if (planeInstance == null) return;

        float length = (endPoint.position - (startPoint != null ? startPoint.position : transform.position)).magnitude;
        planeInstance.transform.localScale = new Vector3(length / planeOriginalSize, 1f, playerWidth / planeOriginalSize);
    }
}
