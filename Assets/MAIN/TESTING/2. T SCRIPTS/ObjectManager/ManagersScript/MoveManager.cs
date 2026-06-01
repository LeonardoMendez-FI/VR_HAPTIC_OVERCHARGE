using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoveManager : ManagerScript
{

    [System.Serializable] public class BoolEvent : UnityEvent<bool> { }
    // dentro de MoveManager:
    public BoolEvent OnFlightModeChanged;

    private void SetMode(bool flying)
    {
        isFlying = flying;
        OnFlightModeChanged?.Invoke(flying);

        if (playerRigidbody != null)
        {
            if (flying)
            {
                playerRigidbody.useGravity = false;
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                // En modo suelo, también desactivamos useGravity porque JumpActor la maneja
                playerRigidbody.useGravity = false;
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }

    [Header("Global multipliers")]
    [Range(0, 2)] public float linear_multiplier = 0.7f;
    [Range(0, 2)] public float angular_multiplier = 0.7f;

    [HideInInspector] public float max_linear_speed;
    [HideInInspector] public float max_angular_speed;

    [Header("Input & Attributes")]
    public InputManager inputManager;
    // MoveAttributes ya no se usa

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

    private void Awake()
    {
        max_linear_speed = PlayerParameters.MEDIUM_LINEAR_SPEED * linear_multiplier;
        max_angular_speed = PlayerParameters.MEDIUM_ANGULAR_SPEED * angular_multiplier;

        if (actors.Count == 0)
        {
            var childActors = GetComponentsInChildren<ActorBase>(true);
            actors = new List<ActorBase>(childActors);
            actors.RemoveAll(a => a == this);
        }

        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();

        if (inputManager != null)
        {
            inputManager.OnFlightRequested.AddListener(ActivateFlight);
            inputManager.OnLandRequested.AddListener(ActivateGround);
        }

        SetMode(false);
    }

    private void OnDestroy()
    {
        if (inputManager != null)
        {
            inputManager.OnFlightRequested.RemoveListener(ActivateFlight);
            inputManager.OnLandRequested.RemoveListener(ActivateGround);
        }
    }

    public override void Update()
    {
        // Ground check (siempre actualizado)
        if (playerRigidbody != null)
        {
            Vector3 rayOrigin = playerRigidbody.position + Vector3.up * 0.1f;
            _isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance + 0.1f, groundLayers);
        }
        else
        {
            _isGrounded = false;
        }

        if (actors.Count == 0) return;

        if (isAttacking)
        {
            foreach (var actor in actors)
                actor.StopExecution();
            return;
        }

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

    // Método básico de movimiento (casi no usado, mantenido por compatibilidad)
    public void Move(Vector3 direction, float speed)
    {
        if (playerTransform != null)
            playerTransform.position += direction * speed * Time.deltaTime;
    }
}