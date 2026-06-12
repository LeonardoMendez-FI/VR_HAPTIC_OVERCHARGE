using UnityEngine;

public class MovementActor : MoveActor
{
    private Vector3 targetVelocity;
    private float targetAngularVel;

    [Header("Permissions")]
    public PlayerPermissions permissions;

    public override bool MeetsRequirements()
    {
        if (!base.MeetsRequirements()) return false;
        if (rb == null) return false;
        if (permissions == null) return false;
        if (!permissions.canMove && !permissions.canRotate) return false;
        return true;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        targetVelocity = rb.linearVelocity;
        targetAngularVel = rb.angularVelocity.y;
    }

    public override void UpdateExecution()
    {
        if (rb == null || input == null) return;

        float moveX = 0f, moveZ = 0f, rotInput = 0f;

        if (permissions.canMove)
        {
            if (input.W) moveZ += 1f;
            if (input.S) moveZ -= 1f;
            if (input.A) moveX -= 1f;
            if (input.D) moveX += 1f;
        }

        if (permissions.canRotate)
        {
            if (input.LeftArrow)  rotInput -= 1f;
            if (input.RightArrow) rotInput += 1f;
        }

        float desiredAngularSpeed = rotInput * maxAngularSpeed;
        if (Mathf.Abs(rotInput) > 0.01f)
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, desiredAngularSpeed, maxAngularSpeed * Time.deltaTime);
        else
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, 0f, maxAngularSpeed * Time.deltaTime);

        Vector3 desiredDir = Vector3.zero;
        if (moveZ > 0) desiredDir += playerTransform.forward;
        if (moveZ < 0) desiredDir -= playerTransform.forward;
        if (moveX > 0) desiredDir += playerTransform.right;
        if (moveX < 0) desiredDir -= playerTransform.right;
        if (desiredDir.magnitude > 0.01f) desiredDir.Normalize();

        Vector3 desiredVelocity = desiredDir * maxLinearSpeed;
        if (desiredDir != Vector3.zero)
            targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, maxLinearSpeed * Time.deltaTime);
        else
            targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, maxLinearSpeed * Time.deltaTime);

        Vector3 vel = targetVelocity;
        vel.y = rb.linearVelocity.y; // preservar velocidad vertical
        rb.linearVelocity = vel;
        rb.angularVelocity = new Vector3(0f, targetAngularVel, 0f);
    }
}