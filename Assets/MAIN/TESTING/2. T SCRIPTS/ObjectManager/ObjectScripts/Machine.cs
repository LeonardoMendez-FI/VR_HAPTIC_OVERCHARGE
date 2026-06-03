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
}