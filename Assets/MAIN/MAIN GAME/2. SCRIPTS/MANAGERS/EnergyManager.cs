using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : ManagerScript
{
    [Header("Configuration")]
    [Range(0, 2)] public float energy_multiplier = 1f;   // Solo se usa si no usamos drainTime

    [Header("Enemy drain time (optional)")]
    [Tooltip("Si es true, la energía máxima se calcula como DrainRate * drainTime.")]
    public bool useDrainTime = false;
    [Tooltip("Tiempo en segundos que tarda en drenarse este enemigo.")]
    public float drainTime = 2f;

    [Header("Debug")]
    public bool showDebug = false;

    public FloatEvent OnEnergyChanged;

    [HideInInspector] public float normalized_local => max_energy > 0f ? curr_energy / max_energy : 0f;
    [HideInInspector] public bool is_empty { get; private set; } = false;
    [HideInInspector] public bool is_full { get; private set; } = false;

    public float curr_energy;
    public float max_energy;

    private float _lastSentNorm = -1f;

    void Awake()
    {
        if (useDrainTime)
        {
            max_energy = PlayerParameters.DrainRate * drainTime;
        }
        else
        {
            max_energy = PlayerParameters.MAX_ENERGY * energy_multiplier;
        }
        curr_energy = max_energy;
        is_empty = false;
        is_full = true;
        _lastSentNorm = normalized_local;
        OnEnergyChanged?.Invoke(_lastSentNorm);
    }

    public void modify_energy(float delta_energy)
    {
        curr_energy = Mathf.Clamp(curr_energy + delta_energy, 0f, max_energy);
        is_empty = normalized_local <= 0f;
        is_full = normalized_local >= 1f;

        float newNorm = normalized_local;
        if (Mathf.Abs(newNorm - _lastSentNorm) > 0.001f)
        {
            _lastSentNorm = newNorm;
            OnEnergyChanged?.Invoke(newNorm);
        }
    }
}