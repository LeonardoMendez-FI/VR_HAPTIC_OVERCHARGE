using UnityEngine;
using UnityEngine.Events;

public class LevelService : ServiceScript
{
    [Header("Level Configuration")]
    public int currentLevel = 1;
    public string levelTitle = "LEVEL 1 — SAMPLE LABORATORY";

    [Header("Machines")]
    public Machine[] machines;

    [Header("Spawn Probabilities per Level")]
    public float[] level1Probabilities = { 0.6f, 0.3f, 0.1f, 0f, 0f };
    public float[] level2Probabilities = { 0.4f, 0.4f, 0.15f, 0.05f, 0f };
    public float[] level3Probabilities = { 0.2f, 0.3f, 0.3f, 0.15f, 0.05f };
    public float[] level4Probabilities = { 0.1f, 0.2f, 0.3f, 0.3f, 0.1f };
    public float[] level5Probabilities = { 0.05f, 0.1f, 0.2f, 0.45f, 0.2f };
    public float[] level6Probabilities = { 0f, 0.05f, 0.1f, 0.35f, 0.5f };

    [Header("References")]
    public AttackManager attackManager;
    public StatsCollector statsCollector;

    [Header("UI Events")]
    public IntEvent OnMachinesRemainingChanged;
    public StringEvent OnLevelTitleChanged;

    private int remainingMachines;
    private float[] currentProbabilities;

    void Start()
    {
        if (machines == null || machines.Length == 0)
            machines = FindObjectsByType<Machine>(FindObjectsSortMode.None);

        remainingMachines = machines.Length;
        attackManager?.SetObjectives(remainingMachines);

        OnMachinesRemainingChanged?.Invoke(remainingMachines);
        OnLevelTitleChanged?.Invoke(levelTitle);

        currentProbabilities = GetProbabilitiesForLevel(currentLevel);

        foreach (var machine in machines)
        {
            if (machine != null && machine.spawnService != null)
                machine.spawnService.SetProbabilities(currentProbabilities);

            if (machine != null && machine.structManager != null)
                machine.structManager.OnEntityDestroyed.AddListener(OnMachineDestroyed);
        }
    }

    void OnDestroy()
    {
        if (machines != null)
        {
            foreach (var machine in machines)
            {
                if (machine != null && machine.structManager != null)
                    machine.structManager.OnEntityDestroyed.RemoveListener(OnMachineDestroyed);
            }
        }
    }

    float[] GetProbabilitiesForLevel(int level)
    {
        return level switch
        {
            1 => level1Probabilities,
            2 => level2Probabilities,
            3 => level3Probabilities,
            4 => level4Probabilities,
            5 => level5Probabilities,
            6 => level6Probabilities,
            _ => level1Probabilities
        };
    }

    void OnMachineDestroyed()
    {
        remainingMachines--;
        OnMachinesRemainingChanged?.Invoke(remainingMachines);
        attackManager?.SetObjectives(remainingMachines);

        if (remainingMachines <= 0)
            statsCollector?.EndGame(true);
    }
}