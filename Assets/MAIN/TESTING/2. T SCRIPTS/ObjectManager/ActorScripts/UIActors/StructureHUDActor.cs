using UnityEngine;

public class StructureHUDActor : UIActor<StructManager>
{
    public StructureBarUI structureBar;

    protected override void Subscribe() => manager.OnStructureChanged.AddListener(OnStructureChanged);
    protected override void Unsubscribe() => manager.OnStructureChanged.RemoveListener(OnStructureChanged);

    void OnStructureChanged(float val) => structureBar?.SetValue(val);
}