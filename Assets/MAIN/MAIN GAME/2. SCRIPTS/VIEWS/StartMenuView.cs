using UnityEngine;

public class StartMenuView : ViewBase
{
    [Header("Player References")]
    public MoveManager moveManager;
    public GazeManager gazeManager;
    public PlayerHUD playerHUD;
    public PlayerPermissions playerPermissions;
    public Rigidbody playerRigidbody;

    void Start()
    {
        if (moveManager == null || gazeManager == null || playerHUD == null || playerPermissions == null || playerRigidbody == null)
        {
            Debug.LogError("[StartMenuView] Faltan referencias. Asigna todos los campos en el Inspector.");
            return;
        }

        // Restringir permisos (solo mirada activa)
        playerPermissions.ResetAll();
        playerPermissions.canGaze = true;

        // Desactivar movimiento por script
        moveManager.enabled = false;

        // Anular gravedad y congelar posición
        playerRigidbody.useGravity = false;
        playerRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        // Asegurar que el gaze funcione
        gazeManager.enabled = true;

        // Ocultar HUD del jugador
        playerHUD.HideHUD();
    }

    protected override void Subscribe() { }
    protected override void Unsubscribe() { }
}