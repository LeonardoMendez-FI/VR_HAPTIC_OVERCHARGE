using UnityEngine;

public class JumpActor : MoveActor
{
    [Header("Jump Multiplier")]
    [Range(0, 2)] public float jumpForceMultiplier = 1f;

    [Header("Permissions")]
    public PlayerPermissions permissions;

    private float jumpForce;

    protected override void Start()
    {
        base.Start();
        jumpForce = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.JUMP_FORCE_RATIO * jumpForceMultiplier;
    }

    public override bool MeetsRequirements()
    {
        return base.MeetsRequirements() && !moveManager.isFlying && rb != null &&
               permissions != null && permissions.canJump;
    }

    public override void UpdateExecution()
    {
        if (rb == null || input == null) return;

        bool isGrounded = moveManager.IsGrounded();

        if (permissions.canJump && input.JumpPressed && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
    }
}