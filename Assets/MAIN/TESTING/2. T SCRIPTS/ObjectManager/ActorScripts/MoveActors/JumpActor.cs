using UnityEngine;

public class JumpActor : MoveActor
{
    [Header("Jump Multiplier")]
    [Range(0, 2)] public float jumpForceMultiplier = 1f;

    private float jumpForce;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayers = ~0;

    private bool isGrounded;

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

        Vector3 rayOrigin = playerTransform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundLayers);

        if (input.JumpPressed && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
            isGrounded = false;
        }
    }
}