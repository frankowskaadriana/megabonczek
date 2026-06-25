using UnityEngine;

public class LightBeam : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI POCISKU ═══════════════")]
    public float damage = 30f;
    public float speed = 20f;
    public float lifetime = 3f;

    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;

        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
        }
        else
        {
            collider.isTrigger = true;
        }

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Trafienie w wroga
        if (other.CompareTag("Enemy"))
        {
            enemyHealth enemy = other.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"💥 LightBeam trafił wroga! Obrażenia: {damage}");
                hasHit = true;
            }
            Destroy(gameObject);
        }
        // Trafienie w ścianę lub przeszkodę
        else if (!other.CompareTag("Player") && !other.CompareTag("Sheep"))
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
}