using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public float damage = 30f;
    public float speed = 20f;
    public float lifetime = 3f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearVelocity = transform.forward * speed;

        // Dodaj kolider jako trigger
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;  // TRIGGER - nie odpycha!
            collider.radius = 0.3f;
        }
        else
        {
            collider.isTrigger = true;  // Upewnij siê ¿e to trigger
        }

        // Zniszcz po czasie
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        // TYLKO wróg - zniszcz pocisk po trafieniu
        if (other.CompareTag("Enemy"))
        {
            enemyHealth enemy = other.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"LightBeam trafi³ wroga! Obra¿enia: {damage}");
            }
            Destroy(gameObject);
        }
        // Zniszcz przy kolizji z czymkolwiek innym (oprócz gracza i owiec)
        else if (!other.CompareTag("Player") && !other.CompareTag("Sheep"))
        {
            Destroy(gameObject);
        }
    }
}