using UnityEngine;

public class Machine : ElectronicObject
{
    [Header("Machine Settings")]
    public SpawnService spawnService;

    void OnValidate()
    {
        if (spawnService == null)
            spawnService = GetComponentInChildren<SpawnService>();
    }

    public void SetPlayerReferences(Transform playerTarget, GazeManager playerGaze,
                                    EnergyManager playerEnergy, AttackSequenceActor attackSeq)
    {
        if (spawnService != null)
            spawnService.SetPlayerReferences(playerTarget, playerGaze, playerEnergy, attackSeq);
    }
}