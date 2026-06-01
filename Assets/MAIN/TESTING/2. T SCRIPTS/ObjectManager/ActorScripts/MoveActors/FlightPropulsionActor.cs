using UnityEngine;

public class FlightPropulsionActor : MoveActor
{
    private Vector3 targetVelocity;
    private float targetAngularVel;

    [Header("Debug")]
    public bool showDebug = false;

    public override bool MeetsRequirements()
    {
        return base.MeetsRequirements() && moveManager.isFlying && rb != null;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        targetVelocity = rb.linearVelocity;
        targetAngularVel = rb.angularVelocity.y;
        if (showDebug) Debug.Log("FlightPropulsionActor: StartExecution");
    }

    public override void UpdateExecution()
    {
        if (rb == null) return;

        // Leer entradas procesadas desde el InputManager
        Vector2 moveInput = input.MoveInput;       // x: strafe, y: forward/back
        float rotInput = input.YawInput;           // -1..1
        float ascendInput = input.AscendInput;     // -1..1

        if (showDebug)
            Debug.Log($"Flight: Move=({moveInput.x:F2},{moveInput.y:F2}) Rot={rotInput:F2} Asc={ascendInput:F2}");

        // Vector de empuje local (lateral, vertical, frontal)
        Vector3 rawInput = new Vector3(moveInput.x, ascendInput, moveInput.y);
        float maxSpeed = PlayerParameters.MEDIUM_LINEAR_SPEED * linearSpeedMultiplier;
        Vector3 desiredVelocity = Vector3.ClampMagnitude(rawInput, 1f) * maxSpeed;
        float desiredAngularSpeed = rotInput * PlayerParameters.MEDIUM_ANGULAR_SPEED * angularSpeedMultiplier;

        // Aceleraciones suaves
        float inputMag = rawInput.magnitude;
        float accel = maxSpeed * inputMag;
        targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, accel * Time.deltaTime);
        if (rawInput == Vector3.zero)
            targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, maxSpeed * Time.deltaTime);

        if (Mathf.Abs(desiredAngularSpeed) > 0.01f)
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, desiredAngularSpeed, maxSpeed * Time.deltaTime);
        else
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, 0f, maxSpeed * Time.deltaTime);

        // Aplicar al Rigidbody
        Vector3 globalVel = playerTransform.TransformDirection(targetVelocity);
        rb.linearVelocity = globalVel;
        rb.angularVelocity = new Vector3(0f, targetAngularVel, 0f);

        if (showDebug)
            Debug.Log($"Flight: targetVel={targetVelocity} globalVel={globalVel} angVel={targetAngularVel:F2}");
    }
}