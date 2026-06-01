using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : ManagerScript
{
    [Header("Global multiplier")]
    [Range(0, 2)] public float energy_multiplier = 1f;

    [Header("Debug")]
    public bool showDebug = false;

    public FloatEvent OnEnergyChanged;

    [HideInInspector] public float normalized_local => max_energy > 0f ? curr_energy / max_energy : 0f;
    [HideInInspector] public bool is_empty { get; private set; } = true;
    [HideInInspector] public bool is_full { get; private set; } = false;

    public float curr_energy;
    public float max_energy;

    void Start()
    {
        max_energy = PlayerParameters.MAX_ENERGY * energy_multiplier;
        curr_energy = max_energy;
        is_empty = false;
        is_full = true;
        OnEnergyChanged?.Invoke(normalized_local);
    }

    public void modify_energy(float delta_energy)
    {
        curr_energy = Mathf.Clamp(curr_energy + delta_energy, 0f, max_energy);
        is_empty = normalized_local <= 0f;
        is_full = normalized_local >= 1f;
        OnEnergyChanged?.Invoke(normalized_local);
    }
}