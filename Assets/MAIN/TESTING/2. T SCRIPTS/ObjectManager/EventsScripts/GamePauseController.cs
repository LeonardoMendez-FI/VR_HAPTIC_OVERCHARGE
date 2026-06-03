using UnityEngine;

/// <summary>
/// Controla la pausa global del juego. Cuando se activa, detiene todos los managers
/// y el input, evitando que el juego siga procesando durante pantallas finales o pausas.
/// Se activa desde EndGameUI u otros sistemas que necesiten congelar la partida.
/// </summary>
public class GamePauseController : MonoBehaviour
{
    public static GamePauseController Instance { get; private set; }

    [Header("Systems to Pause")]
    public ManagerScript[] managersToPause;   // llenar en inspector con MoveManager, EnergyManager, etc.
    public InputLogic inputLogic;
    public GazeManager gazeManager;

    private bool isPaused = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        // Desactivar managers
        foreach (var m in managersToPause)
            if (m != null) m.enabled = false;

        // Desactivar input
        if (inputLogic != null) inputLogic.enabled = false;

        // Desactivar gaze (para que no siga enfocando)
        if (gazeManager != null) gazeManager.enabled = false;

        Debug.Log("[GamePause] Juego pausado.");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        // Reactivar managers
        foreach (var m in managersToPause)
            if (m != null) m.enabled = true;

        if (inputLogic != null) inputLogic.enabled = true;
        if (gazeManager != null) gazeManager.enabled = true;

        Debug.Log("[GamePause] Juego reanudado.");
    }
}