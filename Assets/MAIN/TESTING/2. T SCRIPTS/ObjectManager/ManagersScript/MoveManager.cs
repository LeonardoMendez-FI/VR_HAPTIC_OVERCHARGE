using UnityEngine;
using System.Collections.Generic;

public class MoveManager : ManagerScript
{
    [Header("Global multipliers")]
    [Range(0, 2)] public float linear_multiplier = 0.7f;
    [Range(0, 2)] public float angular_multiplier = 0.7f;

    [HideInInspector] public float max_linear_speed;
    [HideInInspector] public float max_angular_speed;

    [Header("Input & Attributes")]
    public InputManager inputManager;

    [Header("Physics")]
    public Rigidbody playerRigidbody;
    public Transform playerTransform; // ← necesario para los actores

    [Header("State")]
    public bool isFlying = false;
    public bool isAttacking = false;

    private void Awake()
    {
        max_linear_speed = PlayerParameters.MEDIUM_LINEAR_SPEED * linear_multiplier;
        max_angular_speed = PlayerParameters.MEDIUM_ANGULAR_SPEED * angular_multiplier;

        if (actors.Count == 0)
        {
            var childActors = GetComponentsInChildren<IActorScript>(true);
            actors = new List<IActorScript>(childActors);
            actors.RemoveAll(a => a == this);
        }

        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();
        if (playerTransform == null)
        {
            var robot = GetComponentInParent<Robot>();
            if (robot != null)
                playerTransform = robot.transform;
            else if (electronicObject != null)
                playerTransform = electronicObject.transform;
        }

        if (inputManager == null)
            inputManager = FindFirstObjectByType<InputManager>();

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

    private void SetMode(bool flying)
    {
        isFlying = flying;
        if (playerRigidbody != null)
        {
            if (flying)
            {
                playerRigidbody.useGravity = false;
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                playerRigidbody.useGravity = true;
                playerRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }

    public void Move(Vector3 direction, float speed)
    {
        if (playerTransform != null)
            playerTransform.position += direction * speed * Time.deltaTime;
    }
}