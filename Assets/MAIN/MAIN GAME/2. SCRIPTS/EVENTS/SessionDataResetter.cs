using UnityEngine;

public static class SessionDataResetter
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ResetSessionData()
    {
        GameSessionData data = Resources.Load<GameSessionData>("GameSessionData");
        if (data != null)
        {
            data.ResetData();
            Debug.Log("[SessionDataResetter] GameSessionData reiniciado.");
        }
    }
}