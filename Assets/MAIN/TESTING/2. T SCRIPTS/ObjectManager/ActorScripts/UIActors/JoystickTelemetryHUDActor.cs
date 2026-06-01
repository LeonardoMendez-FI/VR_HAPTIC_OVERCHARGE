using UnityEngine;

public class JoystickTelemetryHUDActor : UIActor<MoveManager>
{
    public JoystickTelemetryUI telemetryUI;

    protected override void Subscribe() { }
    protected override void Unsubscribe() { }

    void Update()
    {
        if (manager != null && telemetryUI != null)
        {
            Vector2 input = new Vector2(manager.inputManager.MoveInput.x, manager.inputManager.AscendInput);
            telemetryUI.SetInput(input);
        }
    }
}