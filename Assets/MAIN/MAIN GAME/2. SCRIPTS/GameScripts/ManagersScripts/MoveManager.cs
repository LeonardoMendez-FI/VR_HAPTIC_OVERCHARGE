using System.Collections.Generic;
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
    public InputLogic inputLogic;   // renombrado

    [Header("Physics")]
    public Rigidbody playerRigidbody;
    public Transform playerTransform;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayers = ~0;

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
        // Ground check solo si no vuela
        if (!isFlying && playerRigidbody != null)
        {
            Vector3 rayOrigin = playerRigidbody.position + Vector3.up * 0.1f;
            _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundLayers);
        }
        else if (isFlying)
            _isGrounded = false;

        // StopExecution solo una vez por transición de ataque
        if (isAttacking != _wasAttacking)
        {
            if (isAttacking)
                foreach (var actor in actors)
                    actor.StopExecution();
            _wasAttacking = isAttacking;
        }

        if (isAttacking)
            return;

        foreach (var actor in actors)
        {
            if (actor.MeetsRequirements())
                actor.Solve();
            else
                actor.StopExecution();
        }
    }

    public bool IsGrounded() => _isGrounded;

    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    private void ActivateFlight()
    {
        if (!isAttacking && !isFlying)
            SetMode(true);
    }

    private void ActivateGround()
    {
        if (!isAttacking && isFlying)
            SetMode(false);
    }

    public void ForceLand()
    {
        if (isFlying)
            SetMode(false);
    }

    private void SetMode(bool flying)
    {
        isFlying = flying;
        OnFlightModeChanged?.Invoke(flying);

        if (playerRigidbody != null)
        {
            playerRigidbody.useGravity = false;
            playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        if (playerTransform != null)
            playerTransform.position += direction * speed * Time.deltaTime;
    }
}