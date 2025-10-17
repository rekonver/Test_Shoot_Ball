using UnityEngine;


[CreateAssetMenu(menuName = "BallShooter/GameplayConfig")]
public class GameplayConfig : ScriptableObject
{
    [Header("Door")]
    public float OpenDistance = 5f;
    [Header("Player")]
    public float initialPlayerRadius = 0.6f;
    public float minCriticalRadius = 0.09f;
    [Tooltip("How much extra reserve the initial radius has (0.2 = 20%)")]
    public float initialReservePercent = 0.2f;
    public float PlayerMoveSpeed = 2f;


    [Header("Charging")]
    public float chargeRate = 0.6f;
    public float maxProjectileRadius = 1.2f;
    public float minProjectileRadius = 0.08f;
    [Tooltip("How strongly player radius decreases relative to projectile growth (0..1)")]
    public float transferK = 1.0f;


    [Header("Infection")]
    [Tooltip("Multiplier from projectile radius to infection radius")]
    public float infectionMultiplier = 1.1f;
    public LayerMask obstacleLayer;


    [Header("Advance")]
    public float advanceSpeed = 2f;
    public float doorOpenDistance = 5f;
    [Header("Obstacle")]
    public float mutation = 0.05f;
    [Header("Projectile")]
    public float projectileSpeed = 12f;
    public float explosionMultiplier = 2f;
}