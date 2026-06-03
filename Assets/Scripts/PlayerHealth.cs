using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("═══════════════ HEALTH SETTINGS ═══════════════")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float armor = 0f;

    [Header("═══════════════ UI REFERENCES ═══════════════")]
    public Image healthFill;
    public TextMeshProUGUI healthText;

    void Start()
    {
        // Spróbuj znaleźć UI automatycznie jeśli nie podpięte
        if (healthFill == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                healthFill = canvas.transform.Find("HealthBar/Fill")?.GetComponent<Image>();
            }
        }

        if (healthText == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                healthText = canvas.transform.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
            }
        }

        UpdateUI();
        Debug.Log($"PlayerHealth: {currentHealth}/{maxHealth}");
    }

    public void SetBaseHealth(float health, float initialArmor = 0)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;
        UpdateUI();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void AddArmor(int amount)
    {
        armor += amount;
        UpdateUI();
    }

    public void LevelUpHealth()
    {
        maxHealth += 5f;
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        float reducedDamage = damage * (1f - armor / 100f);
        currentHealth -= reducedDamage;
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
        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (healthText != null)
            healthText.text = Mathf.Round(currentHealth) + " / " + Mathf.Round(maxHealth);
    }

    void Die()
    {
        Debug.Log("Player died!");
        Time.timeScale = 0f;
        Destroy(gameObject);
    }
}