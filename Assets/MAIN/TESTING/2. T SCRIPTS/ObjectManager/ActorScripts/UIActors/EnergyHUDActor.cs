using UnityEngine;

public class EnergyHUDActor : UIActor<EnergyManager>
{
    public EnergyCellsUI energyCells;

    protected override void Subscribe() => manager.OnEnergyChanged.AddListener(OnEnergyChanged);
    protected override void Unsubscribe() => manager.OnEnergyChanged.RemoveListener(OnEnergyChanged);

    void OnEnergyChanged(float val) => energyCells?.SetValue(val);
}