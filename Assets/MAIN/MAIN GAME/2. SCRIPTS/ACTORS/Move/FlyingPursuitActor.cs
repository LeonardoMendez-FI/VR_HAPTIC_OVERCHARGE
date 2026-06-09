using UnityEngine;

public class FlyingPursuitActor : ActorScript<EnemyManager>
{
    [Header("Player Reference (injected by EnemyReferences)")]
    public Transform playerTarget;

    [Header("Flight Settings")]
    public float flyHeight         = 5f;
    public float preferredDistance = 8f;

    [Header("Detection")]
    [Range(0.1f, 3f)] public float detectionTimeMultiplier = 1f;
    [Range(0.1f, 3f)] public float loseTimeMultiplier      = 1f;

    [Header("Speed Multipliers")]
    [Range(0.1f, 3f)]  public float linearSpeedMultiplier  = 0.5f;
    [Range(0.1f, 10f)] public float angularSpeedMultiplier = 1f;

    [HideInInspector] public float speed;
    [HideInInspector] public float rotationSpeed;

    private float detectionRange;
    private float loseRange;
    private bool  hasDetectedPlayer;

    private void Awake()
    {
        speed         = PlayerParameters.MEDIUM_LINEAR_SPEED
                      * PlayerParameters.ENEMY_BASE_SPEED_MULTIPLIER
                      * linearSpeedMultiplier;
        rotationSpeed = PlayerParameters.ENEMY_BASE_ANGULAR_SPEED_DEG
                      * angularSpeedMultiplier
                      * Mathf.Deg2Rad;
        detectionRange = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.ENEMY_DETECTION_TIME
                       * detectionTimeMultiplier;
        loseRange      = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.ENEMY_LOSE_TIME
                       * loseTimeMultiplier;
    }

    public override bool MeetsRequirements()
    {
        if (managerScript == null)
        {
            Debug.LogWarning("[FlyingPursuit] managerScript es null");
            return false;
        }
        if (playerTarget == null)
        {
            Debug.LogWarning("[FlyingPursuit] playerTarget es null");
            return false;
        }
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (!hasDetectedPlayer)
        {
            if (dist <= detectionRange)
            {
                hasDetectedPlayer = true;
                Debug.Log($"[FlyingPursuit] Jugador detectado a {dist:F1}");
                return true;
            }
            Debug.Log($"[FlyingPursuit] Fuera de rango detección: dist={dist:F1} rango={detectionRange:F1}");
            return false;
        }
        if (dist > loseRange)
        {
            hasDetectedPlayer = false;
            Debug.Log($"[FlyingPursuit] Jugador perdido a {dist:F1}");
            return false;
        }
        return true;
    }

    public override void UpdateExecution()
    {
        if (playerTarget == null) return;

        Vector3 toPlayer = playerTarget.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (toPlayer != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        Vector3 moveDir = Vector3.zero;
        if (dist > preferredDistance + 1f)       moveDir =  toPlayer.normalized;
        else if (dist < preferredDistance - 1f)  moveDir = -toPlayer.normalized;

        float   heightError = flyHeight - transform.position.y;
        Vector3 vertMove    = Vector3.up * heightError * 0.5f;

        transform.position += (moveDir * speed + vertMove) * Time.deltaTime;
    }

    public override void StopExecution() { }
}