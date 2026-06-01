using UnityEngine;

public class HapticFlightActor : UIActor<MoveManager>
{
    [Header("References")]
    public HapticManager hapticManager;
    public EnergyManager energyManager;

    private bool wasFlying;

    protected override void Subscribe()
    {
        manager.OnFlightModeChanged.AddListener(OnFlightModeChanged);
        // Podríamos escuchar también un evento de aterrizaje forzoso, pero por ahora
        // detectamos la pérdida de energía en OnFlightModeChanged indirectamente.
        // Si en el futuro se añade un evento OnForcedLand, se puede migrar aquí.
    }

    protected override void Unsubscribe()
    {
        manager.OnFlightModeChanged.RemoveListener(OnFlightModeChanged);
    }

    void OnFlightModeChanged(bool isFlying)
    {
        if (isFlying && !wasFlying)
        {
            // Acabamos de activar el vuelo
            hapticManager?.PlayFlightActivationEffect();
        }
        else if (!isFlying && wasFlying)
        {
            // Acabamos de aterrizar (puede ser forzoso por energía vacía o manual)
            if (energyManager != null && energyManager.is_empty)
            {
                hapticManager?.PlayEnergyDepletedEffect();
            }
        }
        wasFlying = isFlying;
    }
}