using UnityEngine;

public class StartMenuUIManager : MonoBehaviour
{
    void Start()
    {
        var player = FindFirstObjectByType<PlayerRobot>();
        if (player == null) return;

        var hud = player.GetComponentInChildren<PlayerHUD>();
        if (hud != null)
            hud.HideHUD();
    }
}