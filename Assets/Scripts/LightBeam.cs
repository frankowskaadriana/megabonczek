using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public float damage = 30f;
    public float speed = 20f;
    public float lifetime = 2f;

    private Vector3 direction;

    void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyHealth enemy = other.GetComponent<enemyHealth>();
            if (enemy != null) enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}