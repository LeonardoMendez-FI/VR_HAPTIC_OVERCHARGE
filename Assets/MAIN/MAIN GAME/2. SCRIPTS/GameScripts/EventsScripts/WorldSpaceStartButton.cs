using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Se coloca en el botón de inicio en World Space.
/// Requiere un GazeTargetBehaviour en el mismo GameObject.
/// Cuando el jugador hace gaze lock, carga la escena del juego.
/// </summary>
public class WorldSpaceStartButton : MonoBehaviour
{
    [Header("Scene to Load")]
    public string gameSceneName = "GameScene";

    private GazeTargetBehaviour gazeTarget;
    private bool isLocked = false;

    void Awake()
    {
        gazeTarget = GetComponent<GazeTargetBehaviour>();
    }

    public void OnGazeLocked()
    {
        if (isLocked) return;
        isLocked = true;
        Debug.Log("[WorldSpaceStartButton] Gaze locked. Loading game scene...");
        SceneManager.LoadScene(gameSceneName);
    }
}