using UnityEngine;

public class ElectronicObject : MonoBehaviour
{

    [Header("Electronic Managers")]
    public EnergyManager energyManager;
    public StructManager structManager;

    public float LocalNormalizedEnergy()
    {
        return energyManager.normalized_local;
    }

    public float MaxEnergy()
    {

        return energyManager.max_energy;

    }

    public bool EnergyIsEmpty()
    {
        return energyManager.is_empty;
    }

    public void TakeDamage(float energy_damage){

        float struct_damage = energy_damage * PlayerParameters.ENERGY_TO_STRUCT;
        structManager.TakeDamage(struct_damage);

    }

}
