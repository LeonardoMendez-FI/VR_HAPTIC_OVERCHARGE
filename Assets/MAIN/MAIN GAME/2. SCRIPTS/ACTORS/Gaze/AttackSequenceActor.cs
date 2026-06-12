using UnityEngine;
using UnityEngine.Events;

public class AttackSequenceActor : GazeActor
{
    [Header("Attack Settings")]
    public float attackDuration = 2f;

    [Header("Events (wire in Inspector)")]
    public UnityEvent OnAttackStarted;
    public UnityEvent OnAttackEnded;
    public UnityEvent OnEnemyDestroyed;

    [Header("References")]
    public GazeEnergyDrainActor drainActor;
    public EnergyManager playerEnergyManager;   // arrastra el EnergyManager del jugador

    private bool isAttacking;
    private float attackTimer;
    private IGazeTarget currentTarget;
    private ElectronicObject targetElectronics;
    private GazeVisualController targetVisual;
    private GameObject rootToDestroy;
    private float requiredEnergy;              // energía necesaria para el ataque
    private float energySpent;                 // energía ya gastada en este ataque

    public bool IsDraining => isAttacking;

    // Propiedad pública para que AttackEventBridge pueda inspeccionar el objetivo actual
    public IGazeTarget CurrentTarget => currentTarget;

    void Start()
    {
        if (drainActor != null)
            drainActor.onTargetFullyDrained.AddListener(StartAttack);
    }

    void OnDestroy()
    {
        if (drainActor != null)
            drainActor.onTargetFullyDrained.RemoveListener(StartAttack);
    }

    void StartAttack(IGazeTarget target)
    {
        if (isAttacking) return;

        // Obtener referencias desde el GazeTargetBehaviour
        var gazeTarget = target as GazeTargetBehaviour;
        if (gazeTarget != null)
        {
            targetElectronics = gazeTarget.targetElectronicObject;
            targetVisual = gazeTarget.GetComponentInChildren<GazeVisualController>();
            rootToDestroy = gazeTarget.rootToDestroy;
        }

        if (targetElectronics == null)
        {
            Debug.LogError("[AttackSequence] targetElectronicObject no asignado.");
            return;
        }

        // Calcular la energía necesaria para destruir al enemigo
        requiredEnergy = targetElectronics.MaxEnergy();

        // Verificar si tenemos suficiente energía
        if (playerEnergyManager != null && playerEnergyManager.curr_energy < requiredEnergy)
        {
            Debug.Log("[AttackSequence] Energía insuficiente para atacar.");
            // Aquí no se inicia el ataque; el EnemyDepletedActor se encargará del enemigo
            return;
        }

        // Consumir la energía necesaria (se irá restando durante el ataque)
        // Podemos consumir toda al inicio o durante. La consumiré progresivamente.
        isAttacking = true;
        attackTimer = 0f;
        energySpent = 0f;
        currentTarget = target;

        OnAttackStarted?.Invoke();
        StartExecution();
    }

    public override bool MeetsRequirements() => isAttacking;

    public override void UpdateExecution()
    {
        if (targetElectronics == null)
        {
            StopExecution();
            return;
        }

        attackTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(attackTimer / attackDuration);

        // Calcular daño a infligir este frame
        float damage = (requiredEnergy / attackDuration) * Time.deltaTime;

        // Asegurarse de no gastar más de la energía restante
        if (energySpent + damage > requiredEnergy)
            damage = requiredEnergy - energySpent;

        // Quitar energía al jugador
        if (playerEnergyManager != null)
            playerEnergyManager.modify_energy(-damage);

        // Infligir daño al enemigo
        targetElectronics.TakeDamage(damage);

        energySpent += damage;

        if (targetVisual != null)
            targetVisual.SetAttackProgress(progress);

        if (progress >= 1f || energySpent >= requiredEnergy)
        {
            DestroyEnemy();
        }
    }

    void DestroyEnemy()
    {
        if (rootToDestroy != null)
            Destroy(rootToDestroy);
        else if (targetElectronics != null)
            Destroy(targetElectronics.gameObject);

        OnEnemyDestroyed?.Invoke();
        GazeManager?.ForceReleaseTarget();
        StopExecution();
    }

    public override void StopExecution()
    {
        base.StopExecution();
        if (isAttacking)
        {
            isAttacking = false;
            OnAttackEnded?.Invoke();
        }
    }
}