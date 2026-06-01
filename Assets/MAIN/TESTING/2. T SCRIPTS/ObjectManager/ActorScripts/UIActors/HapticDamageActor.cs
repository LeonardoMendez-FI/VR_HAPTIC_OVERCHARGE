using UnityEngine;

public class HapticDamageActor : UIActor<StructManager>
{
    [Header("References")]
    public HapticManager hapticManager;
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
        if (hapticManager == null || playerTransform == null) return;
        Vector3 direction = attackerPosition - playerTransform.position;
        hapticManager.TriggerDamageFeedback(direction);
    }
}