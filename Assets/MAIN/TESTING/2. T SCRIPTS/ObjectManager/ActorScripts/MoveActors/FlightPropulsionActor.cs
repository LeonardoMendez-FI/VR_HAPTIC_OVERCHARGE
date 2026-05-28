using UnityEngine;

public class FlightPropulsionActor : MoveActor
{
    [Header("Ascend Multiplier")]
    [Range(0, 2)] public float ascendForceMultiplier = 1f;

    private float ascendForce;

    private Vector3 targetVelocity;
    private float targetAngularVel;

    [Header("Debug")]
    public bool showDebug = false;

    protected override void Start()
    {
        base.Start();
        ascendForce = PlayerParameters.MEDIUM_LINEAR_SPEED * PlayerParameters.FLIGHT_ASCEND_FORCE_RATIO * ascendForceMultiplier;
    }

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

        bool w = input.W, up = input.UpArrow;
        bool s = input.S, down = input.DownArrow;
        bool a = input.A, left = input.LeftArrow;
        bool d = input.D, right = input.RightArrow;
        float ascendInput = input.AscendInput; // -1..1

        if (showDebug)
            Debug.Log($"Flight: Keys W={w} Up={up} S={s} Down={down} A={a} Left={left} D={d} Right={right} Asc={ascendInput:F2}");

        float moveZ = 0f, moveX = 0f, torque = 0f;

        // Lógica de combinación (idéntica a antes)
        if (w && !up && !s && !down) { moveZ = 1f; torque = 1f; }
        else if (!w && up && !s && !down) { moveZ = 1f; torque = -1f; }
        else if (!w && !up && s && !down) { moveZ = -1f; torque = -1f; }
        else if (!w && !up && !s && down) { moveZ = -1f; torque = 1f; }
        else if (w && up && !s && !down) { moveZ = 1f; torque = 0f; }
        else if (!w && !up && s && down) { moveZ = -1f; torque = 0f; }
        else if (w && !up && !s && down) { moveZ = 0f; torque = 1f; }
        else if (!w && up && s && !down) { moveZ = 0f; torque = -1f; }
        else if (w && s) { moveZ = 0f; torque = 0f; }

        if (a && !d && !right) moveX = -1f;
        else if (!a && d && !left) moveX = 1f;
        else if (left && !a && !d && !right) moveX = -1f;
        else if (right && !d && !a && !left) moveX = 1f;
        if (a && left) moveX = -2f;
        if (d && right) moveX = 2f;
        if ((a && right) || (d && left)) moveX = 0f;

        // Vector de empuje local (lateral, vertical, frontal)
        Vector3 rawInput = new Vector3(moveX, ascendInput, moveZ);
        Vector3 desiredVelocity = Vector3.ClampMagnitude(rawInput, 1f) * maxLinearSpeed;
        float desiredAngularSpeed = torque * maxAngularSpeed;

        if (showDebug)
            Debug.Log($"Flight: rawInput={rawInput} desiredVel={desiredVelocity} torque={torque}");

        // Aceleraciones suaves
        float inputMag = rawInput.magnitude;
        float accel = maxLinearSpeed * inputMag; // aceleración proporcional a la velocidad máxima
        targetVelocity = Vector3.MoveTowards(targetVelocity, desiredVelocity, accel * Time.deltaTime);
        if (rawInput == Vector3.zero)
            targetVelocity = Vector3.MoveTowards(targetVelocity, Vector3.zero, maxLinearSpeed * Time.deltaTime);

        if (Mathf.Abs(desiredAngularSpeed) > 0.01f)
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, desiredAngularSpeed, maxAngularSpeed * Time.deltaTime);
        else
            targetAngularVel = Mathf.MoveTowards(targetAngularVel, 0f, maxAngularSpeed * Time.deltaTime);

        Vector3 globalVel = playerTransform.TransformDirection(targetVelocity);
        rb.linearVelocity = globalVel;
        rb.angularVelocity = new Vector3(0f, targetAngularVel, 0f);

        if (showDebug)
            Debug.Log($"Flight: targetVel={targetVelocity} globalVel={globalVel} angVel={targetAngularVel:F2}");
    }
}