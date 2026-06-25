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
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;
    private float smoothSpeed = 5f;

    void Start()
    {
        audioManager = AudioManager.Instance;
        currentHealth = maxHealth;
        targetFillAmount = 1f;
        currentFillAmount = 1f;
        UpdateUI();
        Debug.Log($"PlayerHealth: {currentHealth}/{maxHealth}");
    }

    void Update()
    {
        if (healthFill != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
            healthFill.fillAmount = currentFillAmount;
        }
    }

    public void SetBaseHealth(float health, float initialArmor = 0)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;
        targetFillAmount = 1f;
        currentFillAmount = 1f;
        UpdateUI();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        targetFillAmount = 1f;
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
        targetFillAmount = 1f;
        UpdateUI();
        if (audioManager != null) audioManager.PlayHeal();
    }

    public void TakeDamage(float damage)
    {
        float reducedDamage = damage * (1f - armor / 100f);
        currentHealth -= reducedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        targetFillAmount = currentHealth / maxHealth;
        UpdateUI();

        if (audioManager != null) audioManager.PlayDamage();

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        targetFillAmount = currentHealth / maxHealth;
        UpdateUI();
        if (audioManager != null) audioManager.PlayHeal();
    }

    public void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(currentHealth)} / {Mathf.Round(maxHealth)}";
        }

        if (healthFill != null)
        {
            targetFillAmount = currentHealth / maxHealth;
        }

        if (healthFill != null)
        {
            float healthPercent = currentHealth / maxHealth;
            if (healthPercent > 0.6f)
                healthFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthFill.color = Color.yellow;
            else
                healthFill.color = Color.red;
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