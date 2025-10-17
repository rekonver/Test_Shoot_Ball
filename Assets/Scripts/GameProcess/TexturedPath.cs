using UnityEngine;

public class TexturedPath : MonoBehaviour
{
    [Tooltip("Префаб Plane (опціонально). Якщо null — створиться стандартний Plane (10x10).")]
    public GameObject planePrefab;

    [Tooltip("Початок доріжки (опціонально). Якщо null — використовується цей об'єкт.")]
    public Transform startPoint;

    [Tooltip("Кінець доріжки (обов’язковий).")]
    public Transform endPoint;

    [Tooltip("Оригінальний розмір Plane у префабі (Unity Plane = 10).")]
    public float planeOriginalSize = 10f;

    private GameObject planeInstance;
    private float playerWidth = 0.5f;
    private float lastPlayerWidth = -1f;
    private Vector3 lastStartPos, lastEndPos;

    void Start()
    {
        CreatePlaneIfNeeded();
        UpdatePath(true);
    }

    void Update()
    {
        UpdatePath();
    }

    private void CreatePlaneIfNeeded()
    {
        if (planeInstance != null) return;

        if (planePrefab != null)
            planeInstance = Instantiate(planePrefab, transform);
        else
            planeInstance = GameObject.CreatePrimitive(PrimitiveType.Plane);

        planeInstance.name = "__PathPlane";
        planeInstance.transform.SetParent(transform, false);
    }

    private void UpdatePath(bool force = false)
    {
        if (endPoint == null || planeInstance == null) return;

        // отримуємо гравця та його ширину
        var player = Instances.Instance.GetOrFind<PlayerController>();
        if (player != null)
            playerWidth = player.transform.localScale.x;

        // позиції початку і кінця
        Vector3 start = startPoint ? startPoint.position : transform.position;
        Vector3 end = endPoint.position;
        start.y = end.y = transform.position.y;

        // перевіряємо, чи потрібно оновлювати
        if (!force && 
            Mathf.Approximately(playerWidth, lastPlayerWidth) &&
            start == lastStartPos && end == lastEndPos)
            return;

        lastPlayerWidth = playerWidth;
        lastStartPos = start;
        lastEndPos = end;

        // напрямок і довжина
        Vector3 dir = end - start;
        float length = dir.magnitude;

        if (length < 0.0001f)
        {
            planeInstance.SetActive(false);
            return;
        }

        planeInstance.SetActive(true);
        planeInstance.transform.position = (start + end) * 0.5f;
        planeInstance.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
        planeInstance.transform.localScale = new Vector3(length / planeOriginalSize, 1f, playerWidth / planeOriginalSize);
    }
}
