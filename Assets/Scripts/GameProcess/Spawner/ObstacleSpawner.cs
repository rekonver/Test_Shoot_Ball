using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawner settings")]
    public GameObject spawnTarget;
    public Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);
    public int obstacleCount = 20;
    public float safeRadius = 1f; // Мінімальна відстань між obstacle
    public bool useSpawnerAsCenter = true;

    [Header("Cluster settings")]
    public Transform[] clusterPoints; // Тепер це Transform-и
    public float clusterRadius = 3f; // Радіус навколо точок скупчень, де можуть з'являтися obstacle

    void Start()
    {
        StartCoroutine(SpawnDelayed());
    }

    private IEnumerator SpawnDelayed()
    {
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

        // Якщо не задано жодного Transform-а, використовуємо центр спавнера
        List<Vector3> clusterPositions = new List<Vector3>();
        if (clusterPoints != null && clusterPoints.Length > 0)
        {
            foreach (var t in clusterPoints)
                if (t != null) clusterPositions.Add(t.position);
        }

        if (clusterPositions.Count == 0)
            clusterPositions.Add(useSpawnerAsCenter ? transform.position : Vector3.zero);

        Vector3 center = useSpawnerAsCenter ? transform.position : Vector3.zero;
        List<Vector3> occupiedPositions = new List<Vector3>();
        int spawned = 0;
        int maxAttempts = obstacleCount * 10; // Максимальна кількість спроб

        for (int i = 0; i < obstacleCount && spawned < obstacleCount && maxAttempts > 0;)
        {
            maxAttempts--;

            // Вибираємо випадкову точку скупчення
            Vector3 clusterCenter = clusterPositions[Random.Range(0, clusterPositions.Count)];
            if (!useSpawnerAsCenter)
            {
                clusterCenter += center;
            }

            // Генеруємо випадкову позицію в межах clusterRadius
            Vector2 randomCircle = Random.insideUnitCircle * clusterRadius;
            Vector3 potentialPos = clusterCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Перевіряємо, чи позиція в межах spawnArea
            if (!IsPositionInSpawnArea(potentialPos, center))
                continue;

            // Перевіряємо колізії з іншими obstacle
            if (IsPositionValid(potentialPos, occupiedPositions, safeRadius))
            {
                var obstacle = pool.Get(potentialPos, Quaternion.identity);
                obstacle.gameObject.SetActive(true);

                // 🔹 якщо є об'єкт, до якого треба прикріпити obstacle
                if (spawnTarget != null)
                    obstacle.transform.SetParent(spawnTarget.transform, true);

                // випадкове масштабування
                float mutation = config.mutation;
                float randomScaleFactor = 1f + Random.Range(-mutation, mutation);
                obstacle.transform.localScale *= randomScaleFactor;

                // позиціонування по DownSpawnPos
                var obstacleScript = obstacle.GetComponent<Obstacle>();
                if (obstacleScript != null && obstacleScript.DownSpawnPos != null)
                {
                    float targetY = (spawnTarget != null ? spawnTarget.transform.position.y : transform.position.y);
                    float offsetY = obstacle.transform.position.y - obstacleScript.DownSpawnPos.position.y;
                    obstacle.transform.position = new Vector3(potentialPos.x, targetY + offsetY, potentialPos.z);
                }
                else
                {
                    float y = (spawnTarget != null ? spawnTarget.transform.position.y : transform.position.y);
                    obstacle.transform.position = new Vector3(potentialPos.x, y, potentialPos.z);
                }

                occupiedPositions.Add(potentialPos);
                spawned++;
                i++;
            }

        }

        Debug.Log($"Spawned {spawned} obstacles around {clusterPositions.Count} cluster(s) with safe radius {safeRadius}");
    }

    bool IsPositionInSpawnArea(Vector3 position, Vector3 center)
    {
        Vector3 areaMin = center - spawnAreaSize * 0.5f;
        Vector3 areaMax = center + spawnAreaSize * 0.5f;

        return position.x >= areaMin.x && position.x <= areaMax.x &&
               position.z >= areaMin.z && position.z <= areaMax.z;
    }

    bool IsPositionValid(Vector3 position, List<Vector3> occupiedPositions, float minDistance)
    {
        foreach (var occupiedPos in occupiedPositions)
        {
            if (Vector3.Distance(new Vector3(position.x, 0, position.z),
                                new Vector3(occupiedPos.x, 0, occupiedPos.z)) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = useSpawnerAsCenter ? transform.position : Vector3.zero;

        // Область спавну
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Vector3 size = new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.z);
        Gizmos.DrawCube(center, size);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        // Центр спавну
        Gizmos.color = Color.red;
        float crossSize = 0.5f;
        Gizmos.DrawLine(center + Vector3.left * crossSize, center + Vector3.right * crossSize);
        Gizmos.DrawLine(center + Vector3.back * crossSize, center + Vector3.forward * crossSize);

        // Візуалізація кластерів
        if (clusterPoints != null && clusterPoints.Length > 0)
        {
            foreach (var t in clusterPoints)
            {
                if (t == null) continue;

                Vector3 pos = t.position;

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(pos, 0.3f);

                Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                Gizmos.DrawWireSphere(pos, clusterRadius);

                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(pos, safeRadius);
            }
        }
        else
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(center, 0.3f);
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center, clusterRadius);
        }
    }
}
