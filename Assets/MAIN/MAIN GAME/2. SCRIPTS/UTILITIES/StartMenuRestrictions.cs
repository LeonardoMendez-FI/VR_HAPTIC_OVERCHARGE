using UnityEngine;

public class StartMenuRestrictions : MonoBehaviour
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
            Debug.LogError("[StartMenuRestrictions] Faltan referencias.");
            return;
        }

        // Solo rotación y mirada
        playerPermissions.ResetAll();
        playerPermissions.canRotate = true;
        playerPermissions.canGaze = true;

        // El Rigidbody debe estar activo pero no moverse
        if (moveManager.playerRigidbody != null)
        {
            moveManager.playerRigidbody.linearVelocity = Vector3.zero;
            moveManager.playerRigidbody.angularVelocity = Vector3.zero;
            moveManager.playerRigidbody.useGravity = false;
            moveManager.playerRigidbody.constraints = RigidbodyConstraints.FreezePosition;
        }

        moveManager.enabled = true;
        gazeManager.enabled = true;
        playerHUD.HideHUD();
    }
}