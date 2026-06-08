using UnityEngine;

public class StartMenuRestrictions : MonoBehaviour
{
    [Header("Player References")]
    public MoveManager moveManager;
    public GazeManager gazeManager;
    public PlayerHUD playerHUD;
    public PlayerPermissions playerPermissions;   // Añadido

    void Start()
    {
        if (moveManager == null || gazeManager == null || playerHUD == null || playerPermissions == null)
        {
            Debug.LogError("[StartMenuRestrictions] Faltan referencias.");
            return;
        }

        playerPermissions.ResetAll();
        playerPermissions.canGaze = true;

        moveManager.enabled = false;
        gazeManager.enabled = true;
        playerHUD.HideHUD();
    }
}