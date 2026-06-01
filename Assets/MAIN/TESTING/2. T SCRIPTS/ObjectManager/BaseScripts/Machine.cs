using UnityEngine;

public class Machine : ElectronicObject
{
    [Header("Machine Settings")]
    public SpawnManager spawnManager;

    void OnValidate()
    {
        if (spawnManager == null)
            spawnManager = GetComponentInChildren<SpawnManager>();
    }
}