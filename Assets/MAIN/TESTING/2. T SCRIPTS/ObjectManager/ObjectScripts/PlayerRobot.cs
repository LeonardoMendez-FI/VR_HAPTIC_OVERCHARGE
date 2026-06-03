using UnityEngine;

public class PlayerRobot : Robot
{
    public InputLogic inputLogic;

    private void Awake()
    {
        Transform root = transform.parent;

        if (moveManager != null)
        {
            if (moveManager.inputLogic == null)
                moveManager.inputLogic = inputLogic;
            if (moveManager.playerTransform == null)
                moveManager.playerTransform = root;
        }
    }
}