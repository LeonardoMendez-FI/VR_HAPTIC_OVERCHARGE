using UnityEngine;

public class MovementModeHUDActor : UIActor<MoveManager>
{
    public MovementModeUI movementModeUI;

    protected override void Subscribe() => manager.OnFlightModeChanged.AddListener(OnFlightModeChanged);
    protected override void Unsubscribe() => manager.OnFlightModeChanged.RemoveListener(OnFlightModeChanged);

    void OnFlightModeChanged(bool flying) => movementModeUI?.SetMode(flying);
}