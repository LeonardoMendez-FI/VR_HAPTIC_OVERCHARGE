using System.Collections;
using UnityEngine;

public class KamikazeAttackActor : EnemyActor
{
    [Header("Detonation Settings")]
    public float      detonationRange  = 2f;
    public float      detonationDelay  = 1.5f;
    public float      damage           = 30f;
    public GameObject explosionPrefab;

    [Header("References")]
    public EnergyManager playerEnergyManager;

    private bool      detonating = false;
    private Coroutine _detonateCoroutine;

    // Must be a protected override so EnemyActor.Start() runs and populates
    // playerTarget from the injected EnemyReferences. The original void Start()
    // (non-override) shadowed the base method, leaving playerTarget always null
    // and causing a NullReferenceException in MeetsRequirements().
    protected override void Start()
    {
        base.Start(); // sets playerTarget from EnemyReferences

        if (playerEnergyManager == null)
        {
            var refs = GetComponentInParent<EnemyReferences>();
            if (refs != null)
                playerEnergyManager = refs.playerEnergyManager;
        }
    }

    public override bool MeetsRequirements()
    {
        if (playerTarget == null) return false;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        return dist <= detonationRange || detonating;
    }

    public override void StartExecution()
    {
        base.StartExecution();
        if (!detonating)
        {
            detonating         = true;
            _detonateCoroutine = StartCoroutine(Detonate());
        }
    }

    private IEnumerator Detonate()
    {
        yield return new WaitForSeconds(detonationDelay);

        // playerTarget is an external reference — guard against it being destroyed
        // during the delay (e.g. player died before impact).
        if (playerTarget != null)
        {
            StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
            if (playerStruct != null)
                playerStruct.TakeDamage(damage, transform.position);
        }

        if (playerEnergyManager != null)
            playerEnergyManager.modify_energy(-playerEnergyManager.curr_energy);

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Stop the coroutine explicitly in case this enemy is killed (e.g. by the
        // player's gaze drain) before the detonation delay elapses. Unity stops
        // coroutines automatically on destroy, but being explicit avoids any edge
        // cases with coroutine runners on other objects.
        if (_detonateCoroutine != null)
            StopCoroutine(_detonateCoroutine);
    }

    public override void UpdateExecution() { }

    // StopExecution intentionally does not cancel the coroutine — once armed the
    // detonation sequence should complete unless the object is destroyed.
    public override void StopExecution() { }
}
