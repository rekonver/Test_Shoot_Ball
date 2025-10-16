using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    public Transform target; // goal position
    public ProjectilePool projectilePool; // новий пул
    public Door door;
    public Transform projectileSpawnPoint;
    public InfectionSystem infectionSystem; // optional, буде через Instances якщо null

    private GameplayConfig config;
    private Coroutine doorCheckCoroutine;
    private bool hasStarted = false;
    private float playerRadius;

    GameObject currentPreviewProjectile;
    Projectile previewProjectileComp;
    bool isCharging = false;
    float chargeTime = 0f;
    bool advancing = false;

    // --- Gizmo variables ---
    private Vector3 openPointXZ;
    private bool hasOpenPoint = false;

    void Start()
    {
        if (Instances.Instance != null)
        {
            if (projectilePool == null)
                projectilePool = Instances.Instance.GetOrFind<ProjectilePool>();

            if (infectionSystem == null)
                infectionSystem = Instances.Instance.GetOrFind<InfectionSystem>();

            if (door == null)
                door = Instances.Instance.GetOrFind<Door>();

            Instances.Instance.Register<PlayerController>(this);

            if (config == null)
                config = Instances.Instance.Get<GameplayConfig>();
        }

        if (config == null)
        {
            Debug.LogError("PlayerController: GameplayConfig not found!");
            return;
        }

        playerRadius = config.initialPlayerRadius * (1f + config.initialReservePercent);
        UpdateVisualScale(playerRadius);
    }

    public void StartDoorCheck()
    {
        if (doorCheckCoroutine != null)
            StopCoroutine(doorCheckCoroutine);

        doorCheckCoroutine = StartCoroutine(CheckDoorDistanceRoutine());
    }

    private IEnumerator CheckDoorDistanceRoutine()
    {
        if (config == null || door == null)
            yield break;

        Transform doorTransform = door.transform;
        float openDistance = config.OpenDistance;

        Vector3 doorXZ = new Vector3(doorTransform.position.x, 0, doorTransform.position.z);
        Vector3 playerXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 dir = (doorXZ - playerXZ).normalized;

        openPointXZ = doorXZ - dir * openDistance;
        hasOpenPoint = true;

        float distance = Vector3.Distance(playerXZ, doorXZ);
        if (distance > openDistance)
        {
            float timeToReach = (distance - openDistance) / config.PlayerMoveSpeed;
            yield return new WaitForSeconds(timeToReach);
        }

        door.Open();
    }

    void Update()
    {
        HandleInput();

        if (hasStarted)
        {
            transform.Translate(Vector3.forward * config.advanceSpeed * Time.deltaTime);
        }
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
            StartCharge();
        if (Input.GetMouseButton(0) && isCharging)
            UpdateCharge(Time.deltaTime);
        if (Input.GetMouseButtonUp(0) && isCharging)
            ReleaseCharge();
    }

    void StartCharge()
    {
        if (advancing) return;
        if (playerRadius <= config.minCriticalRadius + 0.0001f) return;

        isCharging = true;
        chargeTime = 0f;

        if (projectilePool == null && Instances.Instance != null)
            projectilePool = Instances.Instance.GetOrFind<ProjectilePool>();

        if (projectilePool == null)
        {
            Debug.LogError("PlayerController: No ProjectilePool available");
            isCharging = false;
            return;
        }

        // Беремо Projectile з пулу (очікуємо, що Get повертає Projectile)
        Projectile proj = projectilePool.Get(projectileSpawnPoint.position, Quaternion.identity);
        if (proj == null)
        {
            Debug.LogError("PlayerController: ProjectilePool.Get returned null");
            isCharging = false;
            return;
        }

        currentPreviewProjectile = proj.gameObject;
        previewProjectileComp = proj;

        // Важливо: передаємо посилання на пул у Init
        previewProjectileComp.Init(config.minProjectileRadius, projectilePool);
    }

    void UpdateCharge(float dt)
    {
        if (previewProjectileComp == null) return;

        chargeTime += dt;
        float desiredProjRadius = Mathf.Clamp(
            config.minProjectileRadius + config.chargeRate * chargeTime,
            config.minProjectileRadius,
            config.maxProjectileRadius
        );

        float maxDeltaAllowed = playerRadius - config.minCriticalRadius;
        if (maxDeltaAllowed <= 0f)
            desiredProjRadius = config.minProjectileRadius;
        else
        {
            float maxProjRadiusAllowed =
                config.minProjectileRadius +
                (maxDeltaAllowed / Mathf.Max(0.00001f, config.transferK));
            desiredProjRadius = Mathf.Min(desiredProjRadius, maxProjRadiusAllowed, config.maxProjectileRadius);
        }

        // Повторно ініціалізуємо пресв'ю з пулом, щоб previewProjectileComp.projectilePool не став null
        previewProjectileComp.Init(desiredProjRadius, projectilePool);

        float tempDelta = (desiredProjRadius - config.minProjectileRadius) * config.transferK;
        float previewPlayerRadius = Mathf.Max(config.minCriticalRadius, playerRadius - tempDelta);
        UpdateVisualScale(previewPlayerRadius);
    }

    void ReleaseCharge()
    {
        if (!isCharging) return;
        isCharging = false;
        if (currentPreviewProjectile == null || previewProjectileComp == null) return;

        float projRadius = previewProjectileComp.radius;
        float delta = (projRadius - config.minProjectileRadius) * config.transferK;

        playerRadius = Mathf.Max(config.minCriticalRadius, playerRadius - delta);
        UpdateVisualScale(playerRadius);

        // запускаємо снаряд. previewProjectileComp вже має посилання на projectilePool
        previewProjectileComp.Fire(transform.forward);

        // очистка локальних preview-змінних — сам projectile повернеться в пул, коли закінчить життя або вдариться
        currentPreviewProjectile = null;
        previewProjectileComp = null;

        if (!hasStarted)
        {
            hasStarted = true;
            StartDoorCheck();
        }
    }

    void UpdateVisualScale(float radiusToShow)
    {
        transform.localScale = Vector3.one * radiusToShow * 2f;
    }

    void OnDrawGizmos()
    {
        if (!hasOpenPoint) return;

        float planeY = transform.position.y;
        Vector3 openPointOnPlane = new Vector3(openPointXZ.x, planeY, openPointXZ.z);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(openPointOnPlane, 0.1f);

        if (door != null)
        {
            Vector3 doorOnPlane = new Vector3(door.transform.position.x, planeY, door.transform.position.z);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(openPointOnPlane, doorOnPlane);
        }

        Gizmos.color = Color.cyan;
        Vector3 fromOnPlane = new Vector3(transform.position.x, planeY, transform.position.z);
        Gizmos.DrawLine(fromOnPlane, openPointOnPlane);
    }
}
