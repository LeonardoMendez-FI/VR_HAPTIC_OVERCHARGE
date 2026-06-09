using UnityEngine;

public class EnemyReferences : MonoBehaviour
{
    public Transform playerTarget;
    public GazeManager playerGazeManager;
    public EnergyManager playerEnergyManager;

    public void SetReferences(Transform target, GazeManager gaze, EnergyManager energy)
    {
        playerTarget = target;
        playerGazeManager = gaze;
        playerEnergyManager = energy;
    }
}