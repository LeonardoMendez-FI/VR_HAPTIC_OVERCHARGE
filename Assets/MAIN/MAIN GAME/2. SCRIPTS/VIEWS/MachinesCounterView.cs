using TMPro;
using UnityEngine;

public class MachinesCounterView : ViewBase
{
    public TMP_Text counterText;
    public LevelService levelService;   // renombrado

    protected override void Subscribe()
    {
        if (levelService != null)
            levelService.OnMachinesRemainingChanged.AddListener(UpdateCount);
    }

    protected override void Unsubscribe()
    {
        if (levelService != null)
            levelService.OnMachinesRemainingChanged.RemoveListener(UpdateCount);
    }

    private void UpdateCount(int remaining)
    {
        if (counterText != null)
            counterText.text = $"x{remaining}";
    }
}