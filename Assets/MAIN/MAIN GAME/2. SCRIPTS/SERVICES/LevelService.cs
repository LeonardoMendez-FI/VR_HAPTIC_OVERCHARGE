using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class LevelService : ServiceScript
{
    [Header("Level Configuration")]
    public int levelNumber = 1;
    public string levelTitle = "LEVEL 1 — SAMPLE LABORATORY";

    [Header("Machines (asignar manualmente o se buscan solas)")]
    public Machine[] machines;

    [Header("Spawn Probabilities")]
    public float[] probabilities = { 0.6f, 0.3f, 0.1f, 0f, 0f };

    [Header("Player References (auto si vacío)")]
    public Transform playerTarget;
    public GazeManager playerGazeManager;
    public EnergyManager playerEnergyManager;
    public AttackSequenceActor playerAttackSequenceActor;

    [Header("UI Events")]
    public IntEvent OnMachinesRemainingChanged;
    public StringEvent OnLevelTitleChanged;

    [Header("Level Completion")]
    public UnityEvent OnAllMachinesDestroyed;

    private int remainingMachines;
    private bool _levelStarted = false;
    private bool _allDestroyedInvoked = false;  // para no invocar el evento dos veces

    void Start()
    {
        // Buscar jugador si falta
        if (playerTarget == null)
        {
            var player = FindFirstObjectByType<PlayerRobot>();
            if (player != null)
            {
                playerTarget = player.transform;
                playerGazeManager = player.GetComponentInChildren<GazeManager>();
                playerEnergyManager = player.GetComponentInChildren<EnergyManager>();
                playerAttackSequenceActor = player.GetComponentInChildren<AttackSequenceActor>();
            }
        }

        // Buscar máquinas si no están asignadas
        if (machines == null || machines.Length == 0)
            machines = FindObjectsByType<Machine>(FindObjectsSortMode.None);

        remainingMachines = machines.Length;
        _allDestroyedInvoked = false;

        OnMachinesRemainingChanged?.Invoke(remainingMachines);
        OnLevelTitleChanged?.Invoke(levelTitle);

        // Suscribir eventos y asegurar referencias
        foreach (var machine in machines)
        {
            if (machine == null) continue;

            // Asignar referencias del jugador
            machine.SetPlayerReferences(playerTarget, playerGazeManager,
                                        playerEnergyManager, playerAttackSequenceActor);

            if (machine.spawnService != null)
                machine.spawnService.SetProbabilities(probabilities);

            // Asegurar que StructManager esté asignado
            if (machine.structManager == null)
            {
                machine.structManager = machine.GetComponent<StructManager>()
                                     ?? machine.GetComponentInChildren<StructManager>();
                if (machine.structManager == null)
                    Debug.LogError($"[LevelService] La máquina {machine.name} no tiene StructManager. No se contará su destrucción.", machine);
            }

            // Suscribirse al evento de destrucción
            if (machine.structManager != null)
                machine.structManager.OnEntityDestroyed.AddListener(OnMachineDestroyed);
        }
    }

    void Update()
    {
        // Respaldo: si alguna máquina fue destruida sin disparar el evento, la detectamos como null
        if (machines == null || _allDestroyedInvoked) return;

        int aliveCount = 0;
        foreach (var machine in machines)
        {
            if (machine != null && machine.gameObject != null) // referencia válida
                aliveCount++;
        }

        if (aliveCount != remainingMachines)
        {
            // Alguien murió sin avisar
            int diff = remainingMachines - aliveCount;
            Debug.LogWarning($"[LevelService] Detectada pérdida de {diff} máquina(s) sin evento. Ajustando contador.");
            remainingMachines = aliveCount;
            OnMachinesRemainingChanged?.Invoke(remainingMachines);
            CheckAllDestroyed();
        }
    }

    void OnDestroy()
    {
        // Limpiar suscripciones
        if (machines != null)
        {
            foreach (var machine in machines)
            {
                if (machine != null && machine.structManager != null)
                    machine.structManager.OnEntityDestroyed.RemoveListener(OnMachineDestroyed);
            }
        }
    }

    public void StartLevel()
    {
        if (_levelStarted) return;
        _levelStarted = true;

        Debug.Log($"[LevelService] Nivel {levelNumber} iniciado. Máquinas: {machines.Length}");

        foreach (var machine in machines)
        {
            if (machine?.spawnService != null)
                machine.spawnService.StartSpawning();
        }
    }

    public void LoadNextLevel()
    {
        if (levelNumber < 6)
            SceneManager.LoadScene($"3-LEVEL{levelNumber + 1}");
        else
        {
            StatsCollector collector = FindFirstObjectByType<StatsCollector>();
            collector?.EndGame(true);
        }
    }

    void OnMachineDestroyed()
    {
        remainingMachines--;
        Debug.Log($"[LevelService] Máquina destruida. Quedan {remainingMachines}");
        OnMachinesRemainingChanged?.Invoke(remainingMachines);
        CheckAllDestroyed();
    }

    void CheckAllDestroyed()
    {
        if (remainingMachines <= 0 && !_allDestroyedInvoked)
        {
            _allDestroyedInvoked = true;
            Debug.Log("[LevelService] ¡Todas las máquinas destruidas!");
            OnAllMachinesDestroyed?.Invoke();
        }
    }
}