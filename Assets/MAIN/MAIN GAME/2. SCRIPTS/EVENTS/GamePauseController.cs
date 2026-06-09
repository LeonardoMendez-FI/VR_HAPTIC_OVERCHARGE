using UnityEngine;

public class GamePauseController : MonoBehaviour
{
    public static GamePauseController Instance { get; private set; }

    [Header("Systems to Pause")]
    public ManagerScript[] managersToPause;
    public InputLogic inputLogic;
    public GazeManager gazeManager;

    private bool isPaused = false;

    void Awake()
    {
        // Si hay otra instancia, destruimos esta (ya no intentamos persistir)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Si se desea que persista entre escenas, asegúrate de que esté en un GameObject raíz
        // y descomenta la línea siguiente:
        // DontDestroyOnLoad(gameObject);
        // En este proyecto, cada escena tiene su propio jugador, así que NO persistimos.
    }

    public void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;

        foreach (var m in managersToPause)
            if (m != null) m.enabled = false;

        if (inputLogic != null) inputLogic.enabled = false;
        if (gazeManager != null) gazeManager.enabled = false;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;

        foreach (var m in managersToPause)
            if (m != null) m.enabled = true;

        if (inputLogic != null) inputLogic.enabled = true;
        if (gazeManager != null) gazeManager.enabled = true;
    }
}