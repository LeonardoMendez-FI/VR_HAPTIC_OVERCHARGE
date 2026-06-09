using UnityEngine;
using UnityEngine.UI;

public class EnergyBarView : ViewBase
{
    [Header("References")]
    public Image energyBarImage;
    public EnergyManager energyManager;

    protected override void Subscribe()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.AddListener(UpdateBar);
    }

    protected override void Unsubscribe()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.RemoveListener(UpdateBar);
    }

    private void UpdateBar(float normalized)
    {
        if (energyBarImage != null)
            energyBarImage.fillAmount = normalized;
    }
}