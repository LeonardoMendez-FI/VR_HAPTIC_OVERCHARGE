using UnityEngine;

public class JumpActor : MoveActor
{
    [Header("Jump Multiplier")]
    [Range(0, 2)] public float jumpForceMultiplier = 1f;

    private float jumpForce;
    private float verticalVelocity;

    protected override void Start()
    {
        base.Start();
        jumpForce = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.JUMP_FORCE_RATIO * jumpForceMultiplier;
    }

    public override bool MeetsRequirements()
    {
        return base.MeetsRequirements() && !moveManager.isFlying && rb != null;
    }

    public override void UpdateExecution()
    {
        if (rb == null) return;

        // Usar el ground check del MoveManager (única fuente de verdad)
        bool isGrounded = moveManager.IsGrounded();

        if (isGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += PlayerParameters.GRAVITY * Time.deltaTime;
        }

        if (input.JumpPressed && isGrounded)
        {
            verticalVelocity = jumpForce;
        }

        Vector3 vel = rb.linearVelocity;
        vel.y = verticalVelocity;
        rb.linearVelocity = vel;
    }
}