using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;
    public float lifetime = 2f;
    // private float timer; // zakomentuj lub usuñ tê liniê

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyHealth enemy = other.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }
}