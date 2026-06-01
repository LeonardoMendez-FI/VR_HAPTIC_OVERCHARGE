using UnityEngine;
using UnityEngine.Events;

public class StructManager : ManagerScript
{
    [Header("Structure Settings")]
    public float maxHealth = 100f;
    public float currHealth;

    [Header("Events")]
    public FloatEvent OnStructureChanged;           // 0..1 normalizado
    public Vector3Event OnDamagedWithDirection;     // posición del atacante
    public UnityEvent OnEntityDestroyed;            // se dispara al llegar a 0

    private bool isDead;

    void Start()
    {
        currHealth = maxHealth;
        isDead = false;
        OnStructureChanged?.Invoke(1f);
    }

    public float lastDamageReceived { get; private set; }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        lastDamageReceived = damage;
        currHealth = Mathf.Clamp(currHealth - damage, 0, maxHealth);
        OnStructureChanged?.Invoke(currHealth / maxHealth);
        CheckDeath();
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        if (isDead) return;
        lastDamageReceived = damage;
        currHealth = Mathf.Clamp(currHealth - damage, 0, maxHealth);
        OnStructureChanged?.Invoke(currHealth / maxHealth);
        OnDamagedWithDirection?.Invoke(attackerPosition);
        CheckDeath();
    }

    void CheckDeath()
    {
        if (!isDead && currHealth <= 0f)
        {
            isDead = true;
            OnEntityDestroyed?.Invoke();
        }
    }

    // Futuro: método de curación
    public void Heal(float amount)
    {
        if (isDead) return;
        currHealth = Mathf.Clamp(currHealth + amount, 0, maxHealth);
        OnStructureChanged?.Invoke(currHealth / maxHealth);
    }
}