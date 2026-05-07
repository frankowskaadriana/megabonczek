using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    private float baseHealth = 100f;
    private float healthPerLevel = 5f;
    private int currentLevel = 1;

    void Start()
    {
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null) currentLevel = levelSystem.currentLevel;

        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void SetBaseHealth(float health, int level)
    {
        baseHealth = health;
        currentLevel = level;
        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void LevelUpHealth()
    {
        currentLevel++;
        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (healthFill != null) healthFill.fillAmount = currentHealth / maxHealth;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth) + " / " + maxHealth;
    }

    void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy")) TakeDamage(20f);
    }
}