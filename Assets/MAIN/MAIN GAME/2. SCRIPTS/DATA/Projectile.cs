using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public Transform target;

    public float speed = 10f;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null)
        {
            // Perseguir al jugador (misil simple)
            Vector3 dir = (target.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(dir);
        }
        else
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Dañar al jugador si impacta
        if (other.CompareTag("Player"))
        {
            StructManager playerStruct = other.GetComponentInParent<StructManager>();
            if (playerStruct != null)
            {
                playerStruct.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
    }
}