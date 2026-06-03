using UnityEngine;

public class HapticDamageActor : UIActor<StructManager>
{
    [Header("References")]
    public HapticService hapticService;   // renombrado
    public Transform playerTransform;

    protected override void Subscribe()
    {
        manager.OnDamagedWithDirection.AddListener(OnPlayerDamaged);
    }

    protected override void Unsubscribe()
    {
        manager.OnDamagedWithDirection.RemoveListener(OnPlayerDamaged);
    }

    void OnPlayerDamaged(Vector3 attackerPosition)
    {
        if (hapticService == null || playerTransform == null) return;
        Vector3 direction = attackerPosition - playerTransform.position;
        hapticService.TriggerDamageFeedback(direction);
    }
}