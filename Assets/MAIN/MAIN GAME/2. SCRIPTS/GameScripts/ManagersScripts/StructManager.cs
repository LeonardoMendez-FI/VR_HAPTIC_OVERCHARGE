using UnityEngine;
using UnityEngine.Events;

public class StructManager : ManagerScript
{
    [Header("Structure Settings")]
    [Tooltip("Si es true, la salud máxima se calcula como energía máxima * ENERGY_TO_STRUCT.")]
    public bool useEnergyBasedHealth = true;   // nuevo: por defecto, automático

    [Tooltip("Salud máxima fija (solo si useEnergyBasedHealth = false).")]
    public float maxHealth = 100f;

    [HideInInspector] public float currHealth;

    [Header("Events")]
    public FloatEvent OnStructureChanged;
    public Vector3Event OnDamagedWithDirection;
    public UnityEvent OnEntityDestroyed;

    private bool isDead;
    private float _lastSentNorm = -1f;

    void Start()
    {
        if (useEnergyBasedHealth)
        {
            // Obtener el EnergyManager del mismo ElectronicObject
            EnergyManager energy = electronicObject?.energyManager;
            if (energy != null)
                maxHealth = energy.max_energy * PlayerParameters.ENERGY_TO_STRUCT;
            else
                Debug.LogWarning("[StructManager] No se encontró EnergyManager para calcular maxHealth automático. Usando valor manual.");
        }

        currHealth = maxHealth;
        isDead = false;
        _lastSentNorm = 1f;
        OnStructureChanged?.Invoke(1f);
    }

    public float lastDamageReceived { get; private set; }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        lastDamageReceived = damage;
        currHealth = Mathf.Clamp(currHealth - damage, 0, maxHealth);
        SendHealthNorm();
        CheckDeath();
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        if (isDead) return;
        lastDamageReceived = damage;
        currHealth = Mathf.Clamp(currHealth - damage, 0, maxHealth);
        SendHealthNorm();
        OnDamagedWithDirection?.Invoke(attackerPosition);
        CheckDeath();
    }

    void SendHealthNorm()
    {
        float newNorm = currHealth / maxHealth;
        if (Mathf.Abs(newNorm - _lastSentNorm) > 0.001f)
        {
            _lastSentNorm = newNorm;
            OnStructureChanged?.Invoke(newNorm);
        }
    }

    void CheckDeath()
    {
        if (!isDead && currHealth <= 0f)
        {
            isDead = true;
            OnEntityDestroyed?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currHealth = Mathf.Clamp(currHealth + amount, 0, maxHealth);
        SendHealthNorm();
    }
}