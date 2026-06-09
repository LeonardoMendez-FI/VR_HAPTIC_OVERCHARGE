using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [Header("Scene to Load")]
    public string sceneName;

    private bool triggered = false;

    public void OnGazeFocused()
    {
        if (triggered) return;
        triggered = true;
        Debug.Log($"[MenuButton] Cargando escena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}