using UnityEngine;
using System.Collections;

public class enemyHealth : MonoBehaviour
{
    public GameObject Player;
    public float health = 100f;
    public TMPro.TextMeshPro healthText;

    public LevelSystem levelSystem; // DODANE: referencja do LevelSystem

    private bool canTakeSpinDamage = true;
    private float maxHealth; // DODANE: do pasków zdrowia

    void Start()
    {
        maxHealth = health;

        // DODANE: automatyczne znalezienie LevelSystem jeœli nie podpiête
        if (levelSystem == null)
        {
            levelSystem = FindFirstObjectByType<LevelSystem>();
        }

        // DODANE: zwiêkszanie zdrowia z poziomem
        if (levelSystem != null)
        {
            health = 50f + (levelSystem.currentLevel - 1) * 10f;
            maxHealth = health;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.Round(health).ToString();
        }

        // DODANE: automatyczne znalezienie Player jeœli nie podpiêty
        if (Player == null)
        {
            Player = GameObject.FindWithTag("Player");
        }
    }

    void Update()
    {
        if (Player != null && healthText != null)
        {
            TextFacePlayer();
        }
    }

    void TextFacePlayer()
    {
        Vector3 direction = Player.transform.position - healthText.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        healthText.transform.rotation = rotation * Quaternion.Euler(0, 180, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Obs³uga trafienia pociskiem
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(10f);
            Destroy(collision.gameObject);
        }

        // Obs³uga trafienia atakiem wiruj¹cym
        if (collision.gameObject.CompareTag("SpinHitBox") && canTakeSpinDamage)
        {
            Debug.Log("Spin hitbox collision detected");
            TakeDamage(20f);
            StartCoroutine(SpinDamageCooldown());
        }
    }

    // DODANE: wspólna metoda do zadawania obra¿eñ
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (healthText != null)
        {
            healthText.text = Mathf.Round(health).ToString();
        }

        Debug.Log($"Enemy took {damage} damage! Health: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    // DODANE: metoda œmierci z powiadomieniem LevelSystem
    void Die()
    {
        Debug.Log("Enemy died!");

        if (levelSystem != null)
        {
            levelSystem.EnemyDied();
        }

        Destroy(gameObject);
    }

    IEnumerator SpinDamageCooldown()
    {
        canTakeSpinDamage = false;
        yield return new WaitForSeconds(0.5f);
        canTakeSpinDamage = true;
    }
}