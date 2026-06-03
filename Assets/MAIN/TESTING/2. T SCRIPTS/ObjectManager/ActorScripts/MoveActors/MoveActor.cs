using UnityEngine;

public class MoveActor : ActorScript<MoveManager>
{
    [Header("Speed Multipliers")]
    [Range(0, 2)] public float linearSpeedMultiplier = 1f;
    [Range(0, 2)] public float angularSpeedMultiplier = 1f;

    protected float maxLinearSpeed;
    protected float maxAngularSpeed;

    protected MoveManager moveManager => managerScript;
    protected InputLogic input => moveManager.inputLogic;
    protected Rigidbody rb => moveManager.playerRigidbody;
    protected Transform playerTransform => rb?.transform;

    protected virtual void Awake()
    {
        if (managerScript == null)
            managerScript = GetComponentInParent<MoveManager>();
    }

    protected virtual void Start()
    {
        maxLinearSpeed = PlayerParameters.MEDIUM_LINEAR_SPEED * linearSpeedMultiplier;
        maxAngularSpeed = PlayerParameters.MEDIUM_ANGULAR_SPEED * angularSpeedMultiplier;
    }
}