using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LevelService : ServiceScript
{
    [Header("Level Configuration")]
    public int levelNumber = 1;
    public string levelTitle = "LEVEL 1 — SAMPLE LABORATORY";

    [Header("Machines")]
    public Machine[] machines;

    [Header("Spawn Probabilities")]
    public float[] probabilities = { 0.6f, 0.3f, 0.1f, 0f, 0f };

    [Header("Player References (auto-assigned if empty)")]
    public Transform playerTarget;
    public GazeManager playerGazeManager;
    public EnergyManager playerEnergyManager;

    [Header("UI Events")]
    public IntEvent OnMachinesRemainingChanged;
    public StringEvent OnLevelTitleChanged;

    [Header("Next Level")]
    public bool autoLoadNextLevel = false;

    private int remainingMachines;

    void Start()
    {
        // Auto-asignar referencias del jugador si no se arrastraron en el Inspector
        if (playerTarget == null)
        {
            var player = FindFirstObjectByType<PlayerRobot>();
            if (player != null)
            {
                playerTarget = player.transform;
                playerGazeManager = player.GetComponentInChildren<GazeManager>();
                playerEnergyManager = player.GetComponentInChildren<EnergyManager>();
            }
        }

        if (machines == null || machines.Length == 0)
            machines = FindObjectsByType<Machine>(FindObjectsSortMode.None);

        remainingMachines = machines.Length;

        OnMachinesRemainingChanged?.Invoke(remainingMachines);
        OnLevelTitleChanged?.Invoke(levelTitle);

        // Distribuir referencias del jugador a todas las máquinas
        foreach (var machine in machines)
        {
            if (machine != null)
                machine.SetPlayerReferences(playerTarget, playerGazeManager, playerEnergyManager);

            if (machine?.spawnService != null)
                machine.spawnService.SetProbabilities(probabilities);
            if (machine?.structManager != null)
                machine.structManager.OnEntityDestroyed.AddListener(OnMachineDestroyed);
        }

        StatsCollector collector = FindFirstObjectByType<StatsCollector>();
        if (collector != null)
            collector.SetLevel(levelNumber);
    }

    void OnDestroy()
    {
        foreach (var machine in machines)
        {
            if (machine?.structManager != null)
                machine.structManager.OnEntityDestroyed.RemoveListener(OnMachineDestroyed);
        }
    }

    void OnMachineDestroyed()
    {
        remainingMachines--;
        OnMachinesRemainingChanged?.Invoke(remainingMachines);

        if (remainingMachines <= 0)
        {
            if (autoLoadNextLevel && levelNumber < 6)
                SceneManager.LoadScene($"3-LEVEL{levelNumber + 1}");
            else if (autoLoadNextLevel && levelNumber >= 6)
            {
                StatsCollector collector = FindFirstObjectByType<StatsCollector>();
                collector?.EndGame(true);
            }
        }
    }
}