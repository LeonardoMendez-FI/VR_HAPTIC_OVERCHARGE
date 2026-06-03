using UnityEngine;

public class SurvivalGameManager : MonoBehaviour
{
    [Header("References")]
    public StatsCollector statsCollector;

    private bool gameStarted = false;

    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        // Desactivar LevelService si existe
        LevelService levelService = FindFirstObjectByType<LevelService>();
        if (levelService != null)
            levelService.enabled = false;

        // Asegurar que el spawner está activo
        SurvivalSpawner spawner = FindFirstObjectByType<SurvivalSpawner>();
        if (spawner != null)
            spawner.enabled = true;

        // Desactivar máquinas antiguas (si las hay)
        Machine[] machines = FindObjectsByType<Machine>(FindObjectsSortMode.None);
        foreach (var m in machines)
            m.gameObject.SetActive(false);
    }
}