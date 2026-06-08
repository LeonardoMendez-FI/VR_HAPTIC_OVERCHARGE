using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTrigger : MonoBehaviour
{
    [Header("Scene to Load")]
    public string sceneName;

    [Header("Scene Fader")]
    public SceneFader sceneFader;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (sceneFader != null)
            {
                sceneFader.FadeOutAndLoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("[SceneLoaderTrigger] SceneFader no asignado. Cargando sin fade.");
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}