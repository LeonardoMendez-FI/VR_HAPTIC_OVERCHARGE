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
        // Ahora incluye la verificación del permiso de salto
        return base.MeetsRequirements() && !moveManager.isFlying && rb != null &&
               permissions != null && permissions.canJump;
    }

    public override void UpdateExecution()
    {
        if (rb == null || input == null) return;

        bool isGrounded = moveManager.IsGrounded();

        // Solo aplicar impulso de salto si estamos en el suelo y se pulsa el botón
        if (permissions.canJump && input.JumpPressed && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
        // La gravedad la gestiona el Rigidbody (useGravity = true en tierra)
        // No aplicamos gravedad manual para evitar conflictos
    }
}