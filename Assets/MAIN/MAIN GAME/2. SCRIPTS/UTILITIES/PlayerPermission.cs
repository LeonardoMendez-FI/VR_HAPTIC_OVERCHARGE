using UnityEngine;

public class PlayerPermissions : MonoBehaviour
{
    [Header("Movement Permissions")]
    public bool canMove = true;
    public bool canRotate = true;
    public bool canJump = true;

    [Header("Flight Permissions")]
    public bool canToggleFlight = true;
    public bool canLand = true;
    public bool flightEnergyDrainEnabled = true;

    [Header("Combat Permissions")]
    public bool canGaze = true;
    public bool canAttack = true;

    /// <summary>
    /// Restablece todos los permisos a false (útil para tutorial y menús).
    /// </summary>
    public void ResetAll()
    {
        canMove = false;
        canRotate = false;
        canJump = false;
        canToggleFlight = false;
        canLand = false;
        flightEnergyDrainEnabled = false;
        canGaze = false;
        canAttack = false;
    }
}