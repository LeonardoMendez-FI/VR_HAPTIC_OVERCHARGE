using TMPro;
using UnityEngine;

public class RobotsCounterView : ViewBase
{
    public TMP_Text counterText;
    public GameSessionData sessionData;

    protected override void Subscribe()
    {
        if (sessionData != null)
            sessionData.OnTotalRobotsDestroyedChanged.AddListener(UpdateCount);
    }

    protected override void Unsubscribe()
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