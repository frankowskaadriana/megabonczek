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

    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance;
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
        if (audioManager != null) audioManager.PlayHeal();
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
        if (audioManager != null) audioManager.PlayHeal();
    }

    public void TakeDamage(float damage)
    {
        float reducedDamage = damage * (1f - armor / 100f);
        currentHealth -= reducedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();

        if (audioManager != null) audioManager.PlayDamage();

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
        if (audioManager != null) audioManager.PlayHeal();
    }

    public void UpdateUI()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.Round(currentHealth) + " / " + Mathf.Round(maxHealth);
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        if (audioManager != null) audioManager.PlayDeath();
        Time.timeScale = 0f;
        Destroy(gameObject);
    }
}