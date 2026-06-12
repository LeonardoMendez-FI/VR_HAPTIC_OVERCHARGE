using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Kamikaze attack. When the enemy enters detonation range:
///   1. Stops moving.
///   2. Activates an electric particle effect for the charge duration.
///   3. Explodes — drains all player energy and applies damage if the player
///      is still inside the explosion radius.
///   4. Destroys the enemy.
///
/// The charge coroutine runs to completion regardless of whether the player
/// leaves range, so the enemy always destroys itself once triggered.
/// </summary>
public class KamikazeAttackActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Energy Reference (injected by EnemyReferences)")]
    public EnergyManager playerEnergyManager;

    [Header("Detonation Settings")]
    [Range(0.1f, 5f)] public float detonationTimeMultiplier = 1f;
    public float detonationDelay = 1.5f;

    [Header("Explosion Radius")]
    [Tooltip("Multiplier applied to the detonation range for the final damage check.")]
    [Range(0.5f, 3f)] public float explosionRadiusMultiplier = 1.5f;

    [Header("Damage Multiplier")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;

    [Header("VFX")]
    [Tooltip("Electric particle system activated during the charge phase.")]
    public ParticleSystem chargeParticles;

    [Tooltip("Explosion prefab instantiated on detonation.")]
    public GameObject explosionPrefab;

    /// <summary>Range at which the kamikaze begins its charge.</summary>
    public float detonationRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                                  * PlayerParameters.ENEMY_MELEE_TIME
                                  * detonationTimeMultiplier * 0.5f;

    public float damage => PlayerParameters.ENEMY_BASE_MELEE_DAMAGE * 3f * damageMultiplier;

    private bool      _detonating        = false;
    private Coroutine _detonateCoroutine;
    private EnemyEnergyScaledStatsComponent _scaler;

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (playerTarget == null) return false;

        // Once detonating, hold true so EnemyManager doesn't interrupt us.
        if (_detonating) return true;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        return dist <= detonationRange;
    }

    public override void StartExecution()
    {
        base.StartExecution();

        if (_detonating) return;

        _detonating = true;

        // Stop movement.
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Activate charge particles.
        if (chargeParticles != null)
            chargeParticles.Play();

        _detonateCoroutine = StartCoroutine(DetonateSequence());
    }

    private IEnumerator DetonateSequence()
    {
        yield return new WaitForSeconds(detonationDelay);

        // Stop particles.
        if (chargeParticles != null)
            chargeParticles.Stop();

        float explosionRadius = detonationRange * explosionRadiusMultiplier;

        if (playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= explosionRadius)
            {
                // Drain all player energy.
                if (playerEnergyManager != null)
                    playerEnergyManager.modify_energy(-playerEnergyManager.curr_energy);

                // Apply structural damage.
                float scaledDamage = damage;
                if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

                StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
                playerStruct?.TakeDamage(scaledDamage, transform.position);
            }
        }

        // Spawn explosion VFX.
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_detonateCoroutine != null)
            StopCoroutine(_detonateCoroutine);
    }

    // Agent is stopped on StartExecution and the enemy is destroyed on detonation,
    // so StopExecution only needs to handle the edge case where EnemyManager
    // somehow calls it before detonation completes (e.g. EnemyManager disabled).
    public override void StopExecution()
    {
        base.StopExecution();

        if (!_detonating && agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    public override void UpdateExecution() { }
}
