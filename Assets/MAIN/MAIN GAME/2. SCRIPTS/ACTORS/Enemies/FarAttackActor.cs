using UnityEngine;
using UnityEngine.AI;

public class FarAttackActor : ActorScript<EnemyManager>
{
    [Header("Agent Reference")]
    public NavMeshAgent agent;

    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Attack Origin")]
    public Transform attackOrigin;

    [Header("Sphere Reference")]
    public GameObject attackSphere;

    [Header("Attack Range")]
    [Range(0.1f, 5f)] public float timeMultiplier = 1f;

    [Header("Sphere Travel Settings")]
    public float travelSpeed    = 12f;
    public float lingerTime     = 0.4f;
    public float returnSpeed    = 16f;
    public float minSphereScale = 0.1f;
    public float maxSphereScale = 1f;

    [Header("Cooldown")]
    public float attackRate = 0.5f;

    [Header("Damage Multiplier")]
    [Range(0.1f, 5f)] public float damageMultiplier = 1f;

    public float attackRange => PlayerParameters.MEDIUM_LINEAR_SPEED
                              * PlayerParameters.ENEMY_RANGED_TIME
                              * timeMultiplier;

    public float damage => PlayerParameters.ENEMY_BASE_RANGED_DAMAGE * damageMultiplier;

    private enum Phase { Idle, Traveling, Lingering, Returning }
    private Phase _phase          = Phase.Idle;
    private float _lingerTimer    = 0f;
    private float _attackCooldown = 0f;
    private Vector3 _capturedTarget;
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
        if (attackSphere == null || attackOrigin == null) return false;

        if (_phase != Phase.Idle) return true;

        if (_attackCooldown > 0f) return false;

        float dist = Vector3.Distance(attackOrigin.position, playerTarget.position);
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
                    BeginTravel();
                break;

            case Phase.Traveling:
                attackSphere.transform.position = Vector3.MoveTowards(
                    attackSphere.transform.position, _capturedTarget, travelSpeed * Time.deltaTime);

                if (!_hitDelivered) TryDamagePlayer();

                if (Vector3.Distance(attackSphere.transform.position, _capturedTarget) < 0.05f)
                {
                    _phase       = Phase.Lingering;
                    _lingerTimer = lingerTime;
                }
                break;

            case Phase.Lingering:
                if (!_hitDelivered) TryDamagePlayer();
                _lingerTimer -= Time.deltaTime;
                if (_lingerTimer <= 0f)
                    _phase = Phase.Returning;
                break;

            case Phase.Returning:
                Vector3 returnTarget = attackOrigin.position;
                attackSphere.transform.position = Vector3.MoveTowards(
                    attackSphere.transform.position, returnTarget, returnSpeed * Time.deltaTime);

                float totalDist  = Vector3.Distance(_capturedTarget, returnTarget);
                float remaining  = Vector3.Distance(attackSphere.transform.position, returnTarget);
                float t = totalDist > 0f ? 1f - (remaining / totalDist) : 1f;
                SetSphereScale(Mathf.Lerp(maxSphereScale, minSphereScale, t));

                if (remaining < 0.05f)
                    EndAttack();
                break;
        }
    }

    public override void StopExecution()
    {
        base.StopExecution();
        ResumeAgent();

        if (_phase != Phase.Idle)
        {
            _phase = Phase.Idle;
            ResetSphere();
        }
    }

    private void BeginTravel()
    {
        _capturedTarget = playerTarget.position;
        _hitDelivered   = false;

        attackSphere.transform.position = attackOrigin.position;
        SetSphereScale(maxSphereScale);
        attackSphere.SetActive(true);

        _phase = Phase.Traveling;
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

    private void EndAttack()
    {
        ResetSphere();
        _attackCooldown = 1f / attackRate;
        _phase          = Phase.Idle;
    }

    private void ResetSphere()
    {
        if (attackSphere == null) return;
        attackSphere.SetActive(false);
        SetSphereScale(maxSphereScale);
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