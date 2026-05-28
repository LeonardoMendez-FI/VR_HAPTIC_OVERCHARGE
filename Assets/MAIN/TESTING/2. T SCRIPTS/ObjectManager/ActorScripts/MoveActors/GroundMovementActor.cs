using UnityEngine;

public class GroundMovementActor : MoveActor
{
    private Vector3 targetVelocity;
    private float targetAngularVel;

    [Header("Debug")]
    public bool showDebug = false;

    public override bool MeetsRequirements()
    {
        return base.MeetsRequirements() && !moveManager.isFlying && rb != null;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        targetVelocity = rb.linearVelocity;
        targetAngularVel = 0f;
    }

    public override void UpdateExecution()
    {
        if (rb == null) return;

        float rotInput = input.GroundRotInput; // A/D
        Vector2 moveInput = input.MoveInput;   // flechas

        if (showDebug)
            Debug.Log($"Ground: Move=({moveInput.x:F2},{moveInput.y:F2}) Rot={rotInput:F2}");

        // Rotación
        float desiredAngularSpeed = rotInput * maxAngularSpeed;
        if (Mathf.Abs(rotInput) > 0.01f)
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, desiredAngularSpeed, maxAngularSpeed * Time.deltaTime); // aceleración simple
        else
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, 0f, maxAngularSpeed * Time.deltaTime);

        // Movimiento horizontal
        Vector3 desiredDir = Vector3.zero;
        if (moveInput.y > 0) desiredDir += playerTransform.forward;
        if (moveInput.y < 0) desiredDir -= playerTransform.forward;
        if (moveInput.x > 0) desiredDir += playerTransform.right;
        if (moveInput.x < 0) desiredDir -= playerTransform.right;
        if (desiredDir.magnitude > 0.01f) desiredDir.Normalize();

        Vector3 desiredVelocity = desiredDir * maxLinearSpeed;
        if (desiredDir != Vector3.zero)
            targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, maxLinearSpeed * Time.deltaTime);
        else
            targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, maxLinearSpeed * Time.deltaTime);

        Vector3 vel = targetVelocity;
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;
        rb.angularVelocity = new Vector3(0f, targetAngularVel, 0f);
    }
}