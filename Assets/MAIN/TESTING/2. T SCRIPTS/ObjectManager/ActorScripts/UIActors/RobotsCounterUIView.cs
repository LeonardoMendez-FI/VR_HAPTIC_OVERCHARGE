using TMPro;
using UnityEngine;

public class RobotsCounterUIView : MonoBehaviour
{
    public TMP_Text counterText;
    public GameSessionData sessionData;   // asigna el ScriptableObject

    private void OnEnable()
    {
        if (sessionData != null)
            sessionData.OnTotalRobotsDestroyedChanged.AddListener(UpdateCount);
        if (sessionData != null)
            UpdateCount(sessionData.totalRobotsDestroyed);
    }

    private void OnDisable()
    {
        if (sessionData != null)
            sessionData.OnTotalRobotsDestroyedChanged.RemoveListener(UpdateCount);
    }

    private void UpdateCount(int count)
    {
        if (counterText != null)
            counterText.text = $"x{count}";
    }
}