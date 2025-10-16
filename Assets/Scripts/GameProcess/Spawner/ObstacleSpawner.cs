using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawner settings")]
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);
    public int obstacleCount = 20;
    public float randomOffset = 0.5f;
    public bool useSpawnerAsCenter = true; // якщо true, центр = transform.position

    void Start()
    {
        StartCoroutine(SpawnDelayed());
    }

    private IEnumerator SpawnDelayed()
    {
        // Чекаємо один кадр, щоб Instances встиг зареєструвати пул
        yield return null;
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        var pool = Instances.Instance.GetOrFind<ObstaclePool>();
        if (pool == null)
        {
            Debug.LogError("No ObstaclePool found in Instances!");
            return;
        }

        var config = Instances.Instance.Get<GameplayConfig>();
        if (config == null)
        {
            Debug.LogError("No GameplayConfig found in Instances!");
            return;
        }

        int obstacleCount = pool.initialSize;
        if (obstacleCount == 0)
        {
            Debug.LogWarning("ObstaclePool is empty, nothing to spawn!");
            return;
        }

        int rows = Mathf.CeilToInt(Mathf.Sqrt(obstacleCount));
        int cols = Mathf.CeilToInt((float)obstacleCount / rows);

        Vector3 center = useSpawnerAsCenter ? transform.position : Vector3.zero;
        Vector3 startPos = center - spawnAreaSize * 0.5f;
        Vector3 step = new Vector3(spawnAreaSize.x / cols, 0f, spawnAreaSize.z / rows);

        int spawned = 0;
        for (int i = 0; i < rows && spawned < obstacleCount; i++)
        {
            for (int j = 0; j < cols && spawned < obstacleCount; j++)
            {
                if (spawned >= obstacleCount) break;

                // базова позиція в площині XZ
                Vector3 pos = startPos + new Vector3(step.x * j + step.x * 0.5f, 0f, step.z * i + step.z * 0.5f);

                // випадковий офсет лише по XZ
                pos += new Vector3(
                    Random.Range(-randomOffset, randomOffset),
                    0f,
                    Random.Range(-randomOffset, randomOffset)
                );

                // отримуємо obstacle з пулу
                var obstacle = pool.Get(pos, Quaternion.identity);
                obstacle.gameObject.SetActive(true);

                // випадкове масштабування
                float mutation = config.mutation;
                float randomScaleFactor = 1f + Random.Range(-mutation, mutation);
                obstacle.transform.localScale *= randomScaleFactor;

                // позиціонування по DownSpawnPos
                var obstacleScript = obstacle.GetComponent<Obstacle>();
                if (obstacleScript != null && obstacleScript.DownSpawnPos != null)
                {
                    float targetY = transform.position.y;
                    float offsetY = obstacle.transform.position.y - obstacleScript.DownSpawnPos.position.y;
                    obstacle.transform.position = new Vector3(pos.x, targetY + offsetY, pos.z);
                }
                else
                {
                    obstacle.transform.position = new Vector3(pos.x, transform.position.y, pos.z);
                }

                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} obstacles with mutation ±{config.mutation * 100f:F1}% at height {transform.position.y}");
    }



    void OnDrawGizmosSelected()
    {
        Vector3 center = useSpawnerAsCenter ? transform.position : Vector3.zero;

        // напівпрозорий куб
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 size = new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.z);
        Gizmos.DrawCube(center, size);

        // рамка куба
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        // центр спавну — червоний хрест
        float crossSize = 0.5f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
        Gizmos.DrawLine(center + Vector3.back * crossSize, center + Vector3.forward * crossSize);

        // сітка спавну для візуалізації
        int rows = Mathf.CeilToInt(Mathf.Sqrt(obstacleCount));
        int cols = Mathf.CeilToInt((float)obstacleCount / rows);
        Vector3 startPos = center - spawnAreaSize * 0.5f;
        Vector3 step = new Vector3(spawnAreaSize.x / cols, 0f, spawnAreaSize.z / rows);

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                Vector3 pos = startPos + new Vector3(step.x * j + step.x * 0.5f, 0f, step.z * i + step.z * 0.5f);
                Gizmos.DrawSphere(pos, 0.2f);
            }
        }
    }
}