using UnityEngine;

public class HapticFlightActor : UIActor<MoveManager>
{
    [Header("References")]
    public HapticService hapticService;
    public EnergyManager energyManager;

    private bool wasFlying;

    protected override void Subscribe()
    {
        manager.OnFlightModeChanged.AddListener(OnFlightModeChanged);
    }

    protected override void Unsubscribe()
    {
        manager.OnFlightModeChanged.RemoveListener(OnFlightModeChanged);
    }

    void OnFlightModeChanged(bool isFlying)
    {
        if (isFlying && !wasFlying)
        {
            hapticService?.PlayFlightActivationEffect();
        }
        else if (!isFlying && wasFlying)
        {
            if (energyManager != null && energyManager.is_empty)
            {
                hapticService?.PlayEnergyDepletedEffect();
            }
        }
        wasFlying = isFlying;
    }
}