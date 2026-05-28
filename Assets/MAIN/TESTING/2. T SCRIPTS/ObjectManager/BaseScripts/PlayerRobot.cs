using UnityEngine;
public class PlayerRobot : Robot
{
    public InputManager inputManager;

    private void Awake()
    {
        // El transform de la raíz es el padre de UNIT-7Managers (si este script está en ese hijo)
        Transform root = transform.parent; // UNIT-7 raíz

        if (moveManager != null)
        {
            if (moveManager.inputManager == null)
                moveManager.inputManager = inputManager;
            if (moveManager.playerTransform == null)
                moveManager.playerTransform = root; // la raíz para mover todo
        }
    }
}