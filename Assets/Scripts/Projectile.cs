using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damage = 30f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Pocisk trafi³ gracza! Obra¿enia: {damage}");
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}