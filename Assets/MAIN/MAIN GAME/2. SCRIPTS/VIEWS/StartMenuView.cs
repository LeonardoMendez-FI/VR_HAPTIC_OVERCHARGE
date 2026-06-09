using UnityEngine;

public class StartMenuView : ViewBase
{
    [Header("Player References")]
    public MoveManager moveManager;
    public GazeManager gazeManager;
    public PlayerHUD playerHUD;
    public PlayerPermissions playerPermissions;

    void Start()
    {
        if (moveManager == null || gazeManager == null || playerHUD == null || playerPermissions == null)
        {
            Debug.LogError("[StartMenuView] Faltan referencias. Asigna todos los campos en el Inspector.");
            return;
        }

        playerPermissions.ResetAll();
        playerPermissions.canGaze = true;

        moveManager.enabled = false;
        gazeManager.enabled = true;
        playerHUD.HideHUD();
    }

    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
}