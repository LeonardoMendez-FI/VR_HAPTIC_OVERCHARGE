using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldSpaceStartButton : MonoBehaviour
{
    [Header("Scene to Load")]
    public string gameSceneName = "Level1";

    [Header("Auto-hide")]
    [Tooltip("Si es true, el botón se oculta al iniciar la escena (útil para el tutorial).")]
    public bool hideOnStart = false;

    private bool triggered = false;

    void Start()
    {
        if (hideOnStart)
            gameObject.SetActive(false);
    }

    /// <summary>
    /// Llamado por GazeTargetBehaviour.OnGazeFocusedInternal() cuando se hace lock sobre este objeto.
    /// </summary>
    public void OnGazeLocked()
    {
        if (triggered) return;
        triggered = true;
        Debug.Log("[WorldSpaceStartButton] Cargando escena: " + gameSceneName);
        SceneManager.LoadScene(gameSceneName);
    }
}