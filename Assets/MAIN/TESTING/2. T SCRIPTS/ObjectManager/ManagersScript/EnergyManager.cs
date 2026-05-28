using UnityEngine;
using System;

public class EnergyManager : ManagerScript
{
    [Header("Global multiplier")]
    [Range(0,2)]
    public float energy_multiplier = 1f;

    [Header("Energy Parameters")]
    public float normalized_local => max_energy > 0f ? curr_energy / max_energy : 0f;
    public bool is_empty { get; private set; } = true;
    public bool is_full { get; private set; } = false;

    public float curr_energy;
    public float max_energy;

    public void Start(){
        is_empty = false;
        max_energy = PlayerParameters.MAX_ENERGY * energy_multiplier;
    }

    // Función para modificar la variable de energía. Si el valor recibido es positivo se suma, si es negativo se resta.
    public void modify_energy(float delta_energy){

        // Mantener el valor en un rango
        curr_energy = Mathf.Clamp(curr_energy + delta_energy, 0f, max_energy);

        is_empty = normalized_local <= 0f;
        is_full = normalized_local >= 1f;

    }

}