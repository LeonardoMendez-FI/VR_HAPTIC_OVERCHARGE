using UnityEngine;

public class FlightVerticalActor : MoveActor
{
    [Header("Permissions")]
    public PlayerPermissions permissions;

    [Header("Velocidad vertical")]
    [Range(0.1f, 2f)] public float verticalSpeedMultiplier = 1f;

    private float _verticalSpeed;

    protected override void Start()
    {
        base.Start();
        _verticalSpeed = PlayerParameters.MEDIUM_LINEAR_SPEED
                       * PlayerParameters.FLIGHT_ASCEND_FORCE_RATIO
                       * verticalSpeedMultiplier;
    }

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (!moveManager.isFlying) return false;
        if (permissions == null) return false;
        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
    }

    public override void UpdateExecution()
    {
        if (input == null || rb == null) return;

        float v = 0f;
        if (input.RightButtonHeld) v += _verticalSpeed;
        if (input.LeftButtonHeld)  v -= _verticalSpeed;

        Vector3 vel = rb.linearVelocity;
        vel.y = v;
        rb.linearVelocity = vel;
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;
        }
    }
}