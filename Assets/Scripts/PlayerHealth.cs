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

    // ============================================================
    // REFERENCJA DO HealthBar
    // ============================================================
    private HealthBar healthBar;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        gameManager = FindFirstObjectByType<GameManager>();

        // ============================================================
        // ZNAJDŹ HealthBar
        // ============================================================
        healthBar = FindFirstObjectByType<HealthBar>();
        if (healthBar != null)
        {
            Debug.Log("✅ PlayerHealth znalazł HealthBar!");
        }
        else
        {
            Debug.LogWarning("⚠️ HealthBar nie znaleziony!");
        }
    }

    public void SetBaseHealth(float health, float initialArmor)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;
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

        // ============================================================
        // ODSWIEŻ HealthBar
        // ============================================================
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar();
            Debug.Log($"🔄 HealthBar odświeżony: {currentHealth}/{maxHealth}");
        }
        else
        {
            // Spróbuj znaleźć ponownie
            healthBar = FindFirstObjectByType<HealthBar>();
            if (healthBar != null)
            {
                healthBar.UpdateHealthBar();
                Debug.Log($"🔄 HealthBar znaleziony i odświeżony: {currentHealth}/{maxHealth}");
            }
        }

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
        List<BaseEnemy> enemiesHit = new List<BaseEnemy>();

        foreach (var hit in hitColliders)
        {
            BaseEnemy enemy = hit.GetComponent<BaseEnemy>();
            if (enemy != null) enemiesHit.Add(enemy);

            Bazyliszek bazyliszek = hit.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                Rigidbody rb = bazyliszek.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (bazyliszek.transform.position - transform.position).normalized;
                    dir.y = pushbackUpForce;
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(dir * pushbackForce, ForceMode.Impulse);
                }
                continue;
            }

            Leszy leszy = hit.GetComponent<Leszy>();
            if (leszy != null)
            {
                Rigidbody rb = leszy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (leszy.transform.position - transform.position).normalized;
                    dir.y = pushbackUpForce;
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(dir * pushbackForce, ForceMode.Impulse);
                }
                continue;
            }
        }

        foreach (BaseEnemy enemy in enemiesHit)
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

        foreach (BaseEnemy enemy in enemiesHit)
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

        if (healthBar != null) healthBar.UpdateHealthBar();
        RefreshUI();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        AudioManager.Instance?.PlayHeal();

        if (healthBar != null) healthBar.UpdateHealthBar();
        RefreshUI();
    }

    public void AddArmor(int amount) => armor += amount;
    public void LevelUpHealth() => AddMaxHealth(5);

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

        // Dodatkowe odświeżenie HealthBar
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar();
        }
    }

    public void UpdateUI() => RefreshUI();

    public float GetHealthPercent() => currentHealth / maxHealth;
    public bool IsDead() => isDead;

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Player died!");
        AudioManager.Instance?.PlayDeath();
        Time.timeScale = 0f;

        if (healthBar != null) healthBar.UpdateHealthBar();

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushbackRadius);
    }
}