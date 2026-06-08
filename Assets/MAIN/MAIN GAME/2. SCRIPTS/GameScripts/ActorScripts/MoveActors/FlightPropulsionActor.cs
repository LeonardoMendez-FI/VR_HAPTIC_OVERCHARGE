using UnityEngine;

public class FlightPropulsionActor : MoveActor
{
    private Vector3 targetVelocity;
    private float targetAngularVel;

    [Header("Permissions")]
    public PlayerPermissions permissions;

    [Header("Debug")]
    public bool showDebug = false;

    public override bool MeetsRequirements()
    {
        return base.MeetsRequirements() && moveManager.isFlying && rb != null &&
               (permissions.canMove || permissions.canRotate);
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

        bool w = input.W, up = input.UpArrow;
        bool s = input.S, down = input.DownArrow;
        bool a = input.A, left = input.LeftArrow;
        bool d = input.D, right = input.RightArrow;

        float moveX = 0f, moveZ = 0f, torque = 0f;

        if (w && !up && !s && !down)          { moveZ = 1f; torque = 1f; }
        else if (!w && up && !s && !down)      { moveZ = 1f; torque = -1f; }
        else if (!w && !up && s && !down)      { moveZ = -1f; torque = -1f; }
        else if (!w && !up && !s && down)      { moveZ = -1f; torque = 1f; }
        else if (w && up && !s && !down)       { moveZ = 1f; torque = 0f; }
        else if (!w && !up && s && down)       { moveZ = -1f; torque = 0f; }
        else if (w && !up && !s && down)       { moveZ = 0f; torque = 1f; }
        else if (!w && up && s && !down)       { moveZ = 0f; torque = -1f; }
        else if (w && s)                       { moveZ = 0f; torque = 0f; }

        if (a && !d && !right) moveX = -1f;
        else if (!a && d && !left) moveX = 1f;
        else if (left && !a && !d && !right) moveX = -1f;
        else if (right && !d && !a && !left) moveX = 1f;
        if (a && left) moveX = -2f;
        if (d && right) moveX = 2f;
        if ((a && right) || (d && left)) moveX = 0f;

        float moveY = 0f;
        if (input.RightButtonHeld) moveY += 1f;
        if (input.LeftButtonHeld)  moveY -= 1f;

        Vector3 rawInput = new Vector3(moveX, moveY, moveZ);
        float maxSpeed = PlayerParameters.MEDIUM_LINEAR_SPEED * linearSpeedMultiplier;
        Vector3 desiredVelocity = Vector3.ClampMagnitude(rawInput, 1f) * maxSpeed;
        float desiredAngularSpeed = torque * PlayerParameters.MEDIUM_ANGULAR_SPEED * angularSpeedMultiplier;

        float inputMag = rawInput.magnitude;
        float accel = maxSpeed * inputMag;
        targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, accel * Time.deltaTime);
        if (rawInput == Vector3.zero)
            targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, maxSpeed * Time.deltaTime);

        if (Mathf.Abs(desiredAngularSpeed) > 0.01f)
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, desiredAngularSpeed, maxSpeed * Time.deltaTime);
        else
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, 0f, maxSpeed * Time.deltaTime);

        Vector3 globalVel = playerTransform.TransformDirection(targetVelocity);
        rb.linearVelocity = globalVel;
        rb.angularVelocity = new Vector3(0f, targetAngularVel, 0f);
    }
}