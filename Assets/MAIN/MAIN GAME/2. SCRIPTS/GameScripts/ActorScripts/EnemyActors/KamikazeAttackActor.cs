using System.Collections;
using UnityEngine;

public class KamikazeAttackActor : EnemyActor
{
    [Header("Detonation Settings")]
    public float detonationRange = 2f;      // distancia para iniciar cuenta atrás
    public float detonationDelay = 1.5f;    // segundos hasta explotar
    public float damage = 30f;
    public GameObject explosionPrefab;      // efecto visual (opcional)

    [Header("References")]
    public EnergyManager playerEnergyManager; // para drenar energía del jugador

    private bool detonating = false;

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
            detonating = true;
            StartCoroutine(Detonate());
        }
    }

    IEnumerator Detonate()
    {
        yield return new WaitForSeconds(detonationDelay);

        // Dañar al jugador
        StructManager playerStruct = playerTarget.GetComponentInParent<StructManager>();
        if (playerStruct != null)
        {
            playerStruct.TakeDamage(damage, transform.position);
        }

        // Drenar toda la energía del jugador
        if (playerEnergyManager != null)
        {
            playerEnergyManager.modify_energy(-playerEnergyManager.curr_energy);
        }

        // Efecto de explosión
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Destruir este enemigo
        Destroy(gameObject);
    }

    public override void UpdateExecution() { }

    public override void StopExecution() { }
}