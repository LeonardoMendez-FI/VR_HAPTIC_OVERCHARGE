using UnityEngine;
using UnityEngine.UI;

public class EnergyBarUIView: MonoBehaviour
{
    [Header("References")]
    public Image energyBarImage;
    public EnergyManager energyManager;

    private void OnEnable()
    {
        if (energyManager != null)
            energyManager.OnEnergyChanged.AddListener(UpdateBar);
    }

    private void OnDisable()
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