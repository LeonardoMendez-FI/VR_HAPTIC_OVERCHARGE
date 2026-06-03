using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "GameSessionData", menuName = "Game/Session Data")]
public class GameSessionData : ScriptableObject
{
    public int totalRobotsDestroyed;
    public UnityEvent<int> OnTotalRobotsDestroyedChanged;

    public void AddRobotDestroyed()
    {
        totalRobotsDestroyed++;
        OnTotalRobotsDestroyedChanged?.Invoke(totalRobotsDestroyed);
    }

    public void ResetData()
    {
        totalRobotsDestroyed = 0;
        OnTotalRobotsDestroyedChanged?.Invoke(0);
    }
}