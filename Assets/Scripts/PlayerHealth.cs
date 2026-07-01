using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("═══════════════ ZDROWIE ═══════════════")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float armor = 0f;

    [Header("═══════════════ ODEPCHNIĘCIE PRZECIWNIKÓW ═══════════════")]
    public float pushbackRadius = 3f;
    public float pushbackForce = 8f;
    public float pushbackUpForce = 1.5f;
    public float pushbackDuration = 0.5f;

    private LevelSystem levelSystem;
    private bool isPushingBack = false;
    private bool isDead = false;
    private GameManager gameManager;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void SetBaseHealth(float health, float initialArmor)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;

        // Odśwież UI przez GameManager
        RefreshUI();
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float reduced = damage * (1f - armor / 100f);
        currentHealth -= reduced;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        AudioManager.Instance?.PlayDamage();
        PushbackEnemies();

        RefreshUI();

        if (currentHealth <= 0f) Die();
    }

    void PushbackEnemies()
    {
        if (isPushingBack) return;
        StartCoroutine(PushbackCoroutine());
    }

    IEnumerator PushbackCoroutine()
    {
        isPushingBack = true;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushbackRadius);
        List<EnemyHealth> enemiesHit = new List<EnemyHealth>();

        foreach (var hit in hitColliders)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null) enemiesHit.Add(enemy);
        }

        foreach (EnemyHealth enemy in enemiesHit)
        {
            if (enemy != null)
            {
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 direction = (enemy.transform.position - transform.position).normalized;
                    direction.y = pushbackUpForce;
                    enemyRb.isKinematic = false;
                    enemyRb.useGravity = true;
                    enemyRb.AddForce(direction * pushbackForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(pushbackDuration);

        foreach (EnemyHealth enemy in enemiesHit)
        {
            if (enemy != null)
            {
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.isKinematic = true;
                    enemyRb.useGravity = false;
                    enemyRb.linearVelocity = Vector3.zero;
                }
            }
        }

        isPushingBack = false;
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        AudioManager.Instance?.PlayHeal();
        RefreshUI();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        AudioManager.Instance?.PlayHeal();
        RefreshUI();
    }

    public void AddArmor(int amount) => armor += amount;
    public void LevelUpHealth() => AddMaxHealth(5);

    // ============================================
    // ODRZEŚWIEŻ UI - TYLKO PRZEZ GAME MANAGER
    // ============================================

    private void RefreshUI()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    public void UpdateUI()
    {
        RefreshUI();
    }

    // ============================================
    // METODY POMOCNICZE
    // ============================================

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");
        AudioManager.Instance?.PlayDeath();
        Time.timeScale = 0f;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushbackRadius);
    }
}