using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    void Awake()
    {
        if (FindFirstObjectByType<PlayerRobot>() == null)
        {
            GameObject player = Instantiate(playerPrefab);
            DontDestroyOnLoad(player);
        }

        SceneManager.LoadScene("1-START MENU");
    }
}