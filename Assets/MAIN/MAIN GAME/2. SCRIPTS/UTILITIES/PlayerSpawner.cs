using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        var player = FindFirstObjectByType<PlayerRobot>();
        if (player == null) return;

        var spawn = GameObject.FindGameObjectWithTag("Respawn");
        if (spawn != null)
        {
            player.transform.position = spawn.transform.position;
            player.transform.rotation = spawn.transform.rotation;
        }

        // Mostrar el HUD del jugador en esta escena
        var hud = player.GetComponentInChildren<PlayerHUD>();
        if (hud != null)
            hud.ShowHUD();
    }
}