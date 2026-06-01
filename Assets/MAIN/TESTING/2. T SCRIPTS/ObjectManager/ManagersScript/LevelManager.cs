using UnityEngine;

public class LevelManager : ServiceScript
{
    [Header("Level Configuration")]
    public int currentLevel = 1;                          // nivel actual (1–6)

    [Header("Machines")]
    public Machine[] machines;                            // todas las máquinas del nivel (arrastrar o auto-buscar)

    [Header("Spawn Probabilities per Level")]
    [Tooltip("Cada fila corresponde a un nivel, cada columna a un tipo de enemigo (0-4)")]
    public float[][] levelProbabilities = new float[][]
    {
        new float[] {0.6f, 0.3f, 0.1f, 0f, 0f},   // nivel 1: mayoría suelo
        new float[] {0.4f, 0.4f, 0.15f, 0.05f, 0f}, // nivel 2
        new float[] {0.2f, 0.3f, 0.3f, 0.15f, 0.05f}, // nivel 3
        new float[] {0.1f, 0.2f, 0.3f, 0.3f, 0.1f},   // nivel 4
        new float[] {0.05f, 0.1f, 0.2f, 0.45f, 0.2f}, // nivel 5
        new float[] {0f, 0.05f, 0.1f, 0.35f, 0.5f}     // nivel 6: mayoría voladores
    };

    [Header("References")]
    public AttackManager attackManager;
    public StatsCollector statsCollector;

    private int remainingMachines;

    void Start()
    {
        // Buscar máquinas automáticamente si no se asignaron
        if (machines == null || machines.Length == 0)
            machines = FindObjectsByType<Machine>(FindObjectsSortMode.None);

        remainingMachines = machines.Length;
        attackManager?.SetObjectives(remainingMachines);

        // Asignar probabilidades a cada máquina según nivel
        float[] probs = levelProbabilities[Mathf.Clamp(currentLevel - 1, 0, levelProbabilities.Length - 1)];
        foreach (var machine in machines)
        {
            if (machine.spawnManager != null)
                machine.spawnManager.SetProbabilities(probs);

            // Escuchar destrucción de la máquina
            if (machine.structManager != null)
                machine.structManager.OnEntityDestroyed.AddListener(OnMachineDestroyed);
        }
    }

    void OnMachineDestroyed()
    {
        remainingMachines--;
        attackManager?.SetObjectives(remainingMachines);

        if (remainingMachines <= 0)
        {
            statsCollector?.EndGame(true); // victoria
        }
    }
}