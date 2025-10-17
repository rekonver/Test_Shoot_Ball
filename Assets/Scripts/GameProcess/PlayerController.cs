using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    public Transform projectileSpawnPoint;
    public Animator animator;
    public float playerRadius;

    private ProjectilePool projectilePool;
    private Door door;
    private GameplayConfig config;
    private Projectile previewProjectile;
    private Vector3 aimDirection = Vector3.forward;
    private bool isCharging;
    private float chargeTime;
    private bool hasStarted;

    private bool isDead = false;
    public event Action OnDeath;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        var inst = Instances.Instance;
        projectilePool = inst.GetOrFind<ProjectilePool>();
        door = inst.GetOrFind<Door>();
        config = inst.Get<GameplayConfig>();

        playerRadius = config.initialPlayerRadius * (1f + config.initialReservePercent);
        UpdateVisualScale(playerRadius);
    }

    void Update()
    {
        if (isDead) return;

        HandleInput();

        if (hasStarted)
            transform.position += transform.forward * config.advanceSpeed * Time.deltaTime;
    }

    void HandleInput()
    {
        if (isDead) return;

        if (Input.GetMouseButtonDown(0)) StartCharge();
        if (Input.GetMouseButton(0) && isCharging) UpdateCharge(Time.deltaTime);
        if (Input.GetMouseButtonUp(0) && isCharging) ReleaseCharge();
    }

    void StartCharge()
    {
        if (playerRadius <= config.minCriticalRadius) return;

        isCharging = true;
        chargeTime = 0f;

        previewProjectile = projectilePool.Get(projectileSpawnPoint.position, Quaternion.identity);
        previewProjectile.Init(config.minProjectileRadius, projectilePool);

        aimDirection = ComputeAimDirection();
        previewProjectile.transform.forward = aimDirection;
    }

    void UpdateCharge(float dt)
    {
        if (previewProjectile == null) return;

        aimDirection = ComputeAimDirection();
        previewProjectile.transform.forward = aimDirection;

        chargeTime += dt;
        float projRadius = Mathf.Clamp(
            config.minProjectileRadius + config.chargeRate * chargeTime,
            config.minProjectileRadius,
            Mathf.Min(config.maxProjectileRadius, playerRadius)
        );

        previewProjectile.Init(projRadius, projectilePool);
        UpdateVisualScale(Mathf.Max(config.minCriticalRadius, playerRadius - (projRadius - config.minProjectileRadius) * config.transferK));
    }

    void ReleaseCharge()
    {
        if (previewProjectile == null || isDead) return;

        isCharging = false;

        Vector3 finalAim = ComputeAimDirection();
        aimDirection = finalAim;
        previewProjectile.transform.forward = finalAim;

        float projRadius = previewProjectile.radius;
        playerRadius = Mathf.Max(config.minCriticalRadius, playerRadius - (projRadius - config.minProjectileRadius) * config.transferK);
        UpdateVisualScale(playerRadius);

        previewProjectile.Fire(finalAim);
        previewProjectile = null;

        if (playerRadius <= config.minCriticalRadius + 0.0001f)
        {
            Death();
            return;
        }

        if (!hasStarted)
        {
            hasStarted = true;
            animator?.SetTrigger("StartWalk");
            StartCoroutine(OpenDoorRoutine());
        }
    }

    IEnumerator OpenDoorRoutine()
    {
        float distance = Vector3.Distance(transform.position, door.transform.position);
        float openDist = config.OpenDistance;
        if (distance > openDist)
            yield return new WaitForSeconds((distance - openDist) / config.PlayerMoveSpeed);
        door.Open();
    }

    void UpdateVisualScale(float r)
    {
        if (!isDead)
            transform.localScale = Vector3.one * r * 2f;
    }

    private Vector3 ComputeAimDirection()
    {
        if (cam == null) return transform.forward;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 hitPoint = hit.point;
            hitPoint.y = projectileSpawnPoint.position.y; // фіксуємо Y
            return (hitPoint - projectileSpawnPoint.position).normalized;
        }

        Vector3 dir = cam.transform.forward;
        dir.y = 0f;
        return dir.normalized;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (IsLayerInMask(collision.gameObject.layer, config.obstacleLayer))
            Death();
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    public void Death()
    {
        if (isDead) return;
        isDead = true;
        isCharging = false;
        hasStarted = false;

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        StopAllCoroutines();
        animator?.SetTrigger("Death");

        StartCoroutine(ShrinkAndDie());
    }

    private IEnumerator ShrinkAndDie()
    {
        float duration = 0.6f;
        float t = 0f;
        Vector3 startScale = transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = 1f - Mathf.SmoothStep(0f, 1f, t / duration);
            transform.localScale = startScale * k;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        OnDeath?.Invoke();

        Debug.Log($"{name}: Player smoothly shrunk to zero (death).");
    }
}
