using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float HeathValue = 100f;
    public Image healthFill;
    public TextMeshProUGUI healthText;
    public Text healthTextLegacy;

    private int currentLevel = 1;
    private float baseHealth = 100f;
    private float healthPerLevel = 5f;

    void Start()
    {
        LevelSystem levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            currentLevel = levelSystem.currentLevel;
        }

        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        HeathValue = maxHealth;

        UpdateHealthUI();
        Debug.Log("PlayerHealth started with " + HeathValue + " health (Level " + currentLevel + ")");
    }

    public void SetBaseHealth(float health, int level)
    {
        baseHealth = health;
        currentLevel = level;
        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        HeathValue = maxHealth;
        UpdateHealthUI();
    }

    public void LevelUpHealth()
    {
        currentLevel++;
        maxHealth = baseHealth + (currentLevel - 1) * healthPerLevel;
        HeathValue = maxHealth;
        UpdateHealthUI();
        Debug.Log("Health increased! New max: " + maxHealth);
    }

    public void TakeDamage(float damage)
    {
        HeathValue -= damage;
        HeathValue = Mathf.Clamp(HeathValue, 0f, maxHealth);
        UpdateHealthUI();

        if (HeathValue <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        HeathValue += amount;
        HeathValue = Mathf.Clamp(HeathValue, 0f, maxHealth);
        UpdateHealthUI();
        Debug.Log("Healed for " + amount + "! Health: " + HeathValue);
    }

    public void UpdateHealthUI()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = HeathValue / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.Round(HeathValue) + " / " + maxHealth;
        }

        if (healthTextLegacy != null)
        {
            healthTextLegacy.text = Mathf.Round(HeathValue) + " / " + maxHealth;
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
            TakeDamage(20f);
        }
    }
}