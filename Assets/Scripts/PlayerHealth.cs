using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("PlayerHealth")]
    public float HeathValue = 100f;
    public float damageAmount = 20f;
    public bool isInvincible = false;

    void Start()
    {
        Debug.Log("PlayerHealth started with " + HeathValue + " health");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage called. Damage: " + damage + ", isInvincible: " + isInvincible);

        if (isInvincible)
        {
            Debug.Log("Berserk active! No damage taken!");
            return;
        }

        HeathValue -= damage;
        Debug.Log("Health now: " + HeathValue);

        if (HeathValue <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit by enemy! Taking " + damageAmount + " damage");
            TakeDamage(damageAmount);
        }
    }
}