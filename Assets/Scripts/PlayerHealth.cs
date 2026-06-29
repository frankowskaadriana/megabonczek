using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    [Header("Zdrowie")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float armor = 0f;

    [Header("Odepchnięcie przeciwników")]
    public float pushbackRadius = 3f;
    public float pushbackForce = 8f;
    public float pushbackUpForce = 1.5f;
    public float pushbackDuration = 0.5f;

    [Header("UI")]
    public Image healthFill;
    public TextMeshProUGUI healthText;

    private LevelSystem levelSystem;
    private float targetFill = 1f;
    private float currentFill = 1f;
    private const float SMOOTH_SPEED = 5f;
    private bool isPushingBack = false;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        UpdateUI();
    }

    void Update()
    {
        if (healthFill != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * SMOOTH_SPEED);
            healthFill.fillAmount = currentFill;
        }
    }

    public void SetBaseHealth(float health, float initialArmor)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;
        targetFill = 1f;
        currentFill = 1f;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        float reduced = damage * (1f - armor / 100f);
        currentHealth -= reduced;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        targetFill = currentHealth / maxHealth;
        UpdateUI();

        AudioManager.Instance?.PlayDamage(); // DŹWIĘK OBRAŻEŃ

        PushbackEnemies();

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
            if (enemy != null)
            {
                enemiesHit.Add(enemy);
            }
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
        targetFill = currentHealth / maxHealth;
        UpdateUI();

        AudioManager.Instance?.PlayHeal(); // DŹWIĘK LECZENIA
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        targetFill = 1f;
        UpdateUI();

        AudioManager.Instance?.PlayHeal(); // DŹWIĘK LECZENIA
    }

    public void AddArmor(int amount) => armor += amount;
    public void LevelUpHealth() => AddMaxHealth(5);

    public void UpdateUI()
    {
        if (healthFill != null) targetFill = currentHealth / maxHealth;
        if (healthText != null) healthText.text = $"{Mathf.Round(currentHealth)} / {Mathf.Round(maxHealth)}";
        if (levelSystem != null) levelSystem.UpdateUI();
    }

    void Die()
    {
        Debug.Log("Player died!");
        AudioManager.Instance?.PlayDeath(); // DŹWIĘK ŚMIERCI
        Time.timeScale = 0f;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushbackRadius);
    }
}