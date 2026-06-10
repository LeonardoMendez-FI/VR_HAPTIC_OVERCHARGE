using System.Collections;
using UnityEngine;

public class KamikazeAttackActor : ActorScript<EnemyManager>
{
    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Energy Reference (injected by EnemyReferences)")]
    public EnergyManager playerEnergyManager;

    [Header("Detonation Settings")]
    [Range(0.1f, 5f)] public float detonationTimeMultiplier = 1f;
    public float detonationDelay = 1.5f;

    [Header("Damage Multiplier")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;

    public GameObject explosionPrefab;

    public float detonationRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                                 * PlayerParameters.ENEMY_MELEE_TIME
                                 * detonationTimeMultiplier * 0.5f;
    public float damage => PlayerParameters.ENEMY_BASE_MELEE_DAMAGE * 3f * damageMultiplier;

    private bool      detonating        = false;
    private Coroutine _detonateCoroutine;
    private EnemyEnergyScaledStatsComponent _scaler;

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
    }

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        return dist <= detonationRange || detonating;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        if (!detonating)
        {
            detonating         = true;
            _detonateCoroutine = StartCoroutine(Detonate());
        }
    }

    private IEnumerator Detonate()
    {
        yield return new WaitForSeconds(detonationDelay);

        float scaledDamage = damage;
        if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

        if (playerTarget != null)
        {
            StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
            playerStruct?.TakeDamage(scaledDamage, transform.position);
        }

        if (playerEnergyManager != null)
            playerEnergyManager.modify_energy(-playerEnergyManager.curr_energy);

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_detonateCoroutine != null)
            StopCoroutine(_detonateCoroutine);
    }

    public override void UpdateExecution() { }
    public override void StopExecution()   { }
}