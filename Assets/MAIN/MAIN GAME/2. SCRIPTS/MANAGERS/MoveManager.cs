using UnityEngine;
using UnityEngine.Events;

public class MoveManager : ManagerScript
{
    [System.Serializable] public class BoolEvent : UnityEvent<bool> { }
    public BoolEvent OnFlightModeChanged;

    [Header("Global multipliers")]
    [Range(0, 2)] public float linear_multiplier = 0.7f;
    [Range(0, 2)] public float angular_multiplier = 0.7f;

    [HideInInspector] public float max_linear_speed;
    [HideInInspector] public float max_angular_speed;

    [Header("Input & Attributes")]
    public InputLogic inputLogic;

    [Header("Physics")]
    public Rigidbody playerRigidbody;
    public Transform playerTransform;

    [Header("Ground Check")]
    public LayerMask groundLayers = ~0;
    public bool showDebug = false;

    [Header("State")]
    public bool isFlying = false;
    public bool isAttacking = false;

    private bool _isGrounded;
    private bool _wasAttacking;

    private void Awake()
    {
        max_linear_speed = PlayerParameters.MEDIUM_LINEAR_SPEED * linear_multiplier;
        max_angular_speed = PlayerParameters.MEDIUM_ANGULAR_SPEED * angular_multiplier;

        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();

        if (inputLogic != null)
        {
            inputLogic.OnFlightRequested.AddListener(ActivateFlight);
            inputLogic.OnLandRequested.AddListener(ActivateGround);
        }

        SetMode(false);
    }

    private void OnDestroy()
    {
        if (inputLogic != null)
        {
            inputLogic.OnFlightRequested.RemoveListener(ActivateFlight);
            inputLogic.OnLandRequested.RemoveListener(ActivateGround);
        }
    }

    public override void Update()
    {
        // Ground check
        if (playerRigidbody != null)
        {
            Vector3 feetPosition = GetFeetPosition();
            LayerMask mask = groundLayers & ~(1 << playerRigidbody.gameObject.layer);
            _isGrounded = Physics.CheckSphere(feetPosition, 0.3f, mask);
        }
        else
        {
            _isGrounded = false;
        }

        // Manejar cambio de estado de ataque
        if (isAttacking != _wasAttacking)
        {
            if (isAttacking)
                foreach (var actor in actors) actor.StopExecution();
            _wasAttacking = isAttacking;
        }
        if (isAttacking) return;

        // Ejecutar actores
        foreach (var actor in actors)
        {
            if (actor.MeetsRequirements()) actor.Solve();
            else actor.StopExecution();
        }
    }

    private Vector3 GetFeetPosition()
    {
        Collider col = playerRigidbody?.GetComponent<Collider>();
        if (col != null)
            return new Vector3(col.bounds.center.x, col.bounds.min.y + 0.1f, col.bounds.center.z);
        if (playerTransform != null)
            return playerTransform.position + Vector3.down * 0.9f;
        return playerRigidbody.position;
    }

    public bool IsGrounded() => _isGrounded;

    public void SetAttacking(bool attacking) => isAttacking = attacking;

    private void ActivateFlight()
    {
        if (!isAttacking && !isFlying) SetMode(true);
    }

    private void ActivateGround()
    {
        if (!isAttacking && isFlying) SetMode(false);
    }

    public void ForceLand()
    {
        if (isFlying) SetMode(false);
    }

    private void SetMode(bool flying)
    {
        isFlying = flying;
        OnFlightModeChanged?.Invoke(flying);

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = !flying;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    // Métodos agregados para compatibilidad con FlightEnergyDrainActor
    public float GetSpeedFraction()
    {
        if (playerRigidbody == null) return 0f;
        return Mathf.Clamp01(playerRigidbody.linearVelocity.magnitude / max_linear_speed);
    }

    public float GetAngularFraction()
    {
        if (playerRigidbody == null) return 0f;
        return Mathf.Clamp01(Mathf.Abs(playerRigidbody.angularVelocity.y) / max_angular_speed);
    }
}