using UnityEngine;

public class EnergyActor: ActorScript<EnergyManager>
{
    private float max_energy;

    private void Start()
    {
        max_energy = this.managerScript.max_energy;
    }

}
