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

    /// <summary>
    /// Llamado por LevelService para inyectar las referencias del jugador.
    /// </summary>
    public void SetPlayerReferences(Transform playerTarget, GazeManager playerGaze, EnergyManager playerEnergy)
    {
        if (spawnService != null)
            spawnService.SetPlayerReferences(playerTarget, playerGaze, playerEnergy);
    }
}