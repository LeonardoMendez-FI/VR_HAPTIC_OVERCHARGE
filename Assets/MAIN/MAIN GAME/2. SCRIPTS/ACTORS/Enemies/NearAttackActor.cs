using UnityEngine;
using UnityEngine.AI;

public class NearAttackActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Sphere Reference")]
    public GameObject attackSphere;

    [Header("Attack Range")]
    [Range(0.1f, 5f)] public float timeMultiplier = 1f;

    [Header("Sphere Scale Settings")]
    public float maxSphereScale   = 2f;
    public float expandDuration   = 0.3f;
    public float contractDuration = 0.3f;

    [Header("Cooldown")]
    public float attackRate = 1f;

    [Header("Damage Multiplier")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;

    public float attackRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                              * PlayerParameters.ENEMY_MELEE_TIME
                              * timeMultiplier;

    public float damage => PlayerParameters.ENEMY_BASE_MELEE_DAMAGE * damageMultiplier;

    private enum Phase { Idle, Expanding, Contracting }
    private Phase _phase          = Phase.Idle;
    private float _phaseTimer     = 0f;
    private float _attackCooldown = 0f;
    private bool  _hitDelivered   = false;

    private EnemyEnergyScaledStatsComponent _scaler;
    private SphereCollider _sphereCollider;

    private void Start()
    {
        _scaler = GetComponentInParent<EnemyEnergyScaledStatsComponent>();
        if (attackSphere != null)
        {
            _sphereCollider = attackSphere.GetComponent<SphereCollider>();
            attackSphere.SetActive(false);
        }
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (playerTarget == null) return false;
        if (attackSphere == null) return false;

        // Sigue activo mientras se ejecuta la animación de ataque
        if (_phase != Phase.Idle) return true;

        if (_attackCooldown > 0f) return false;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        return dist <= attackRange;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        StopAgent();
    }

    public override void UpdateExecution()
    {
        _attackCooldown -= Time.deltaTime;

        switch (_phase)
        {
            case Phase.Idle:
                if (_attackCooldown <= 0f)
                    BeginExpand();
                break;

            case Phase.Expanding:
                _phaseTimer += Time.deltaTime;
                float expandT = Mathf.Clamp01(_phaseTimer / expandDuration);
                SetSphereScale(Mathf.Lerp(0f, maxSphereScale, expandT));

                if (!_hitDelivered)
                    TryDamagePlayer();

                if (_phaseTimer >= expandDuration)
                {
                    _phaseTimer = 0f;
                    _phase      = Phase.Contracting;
                }
                break;

            case Phase.Contracting:
                _phaseTimer += Time.deltaTime;
                float contractT = Mathf.Clamp01(_phaseTimer / contractDuration);
                SetSphereScale(Mathf.Lerp(maxSphereScale, 0f, contractT));

                if (_phaseTimer >= contractDuration)
                {
                    attackSphere.SetActive(false);
                    _attackCooldown = 1f / attackRate;
                    _phaseTimer     = 0f;
                    _phase          = Phase.Idle;
                }
                break;
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        ResumeAgent();

        if (_phase != Phase.Idle)
        {
            _phase      = Phase.Idle;
            _phaseTimer = 0f;
            _attackCooldown = 0f;
            if (attackSphere != null) attackSphere.SetActive(false);
        }
    }

    private void BeginExpand()
    {
        _phase        = Phase.Expanding;
        _phaseTimer   = 0f;
        _hitDelivered = false;
        attackSphere.SetActive(true);
        SetSphereScale(0f);
    }

    private void SetSphereScale(float scale)
    {
        if (attackSphere != null)
            attackSphere.transform.localScale = Vector3.one * scale;
    }

    private void TryDamagePlayer()
    {
        if (playerTarget == null || _sphereCollider == null) return;

        Vector3 center = _sphereCollider.bounds.center;
        float   radius = _sphereCollider.bounds.extents.magnitude;

        if (Vector3.Distance(center, playerTarget.position) <= radius)
        {
            float scaledDamage = damage;
            if (_scaler != null) scaledDamage *= _scaler.CurrentDamageScale;

            StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
            playerStruct?.TakeDamage(scaledDamage, transform.position);
            _hitDelivered = true;
        }
    }

    private void StopAgent()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void ResumeAgent()
    {
        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }
}