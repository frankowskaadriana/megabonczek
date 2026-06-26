using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Zdrowie")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float armor = 0f;

    [Header("UI")]
    public Image healthFill;
    public TextMeshProUGUI healthText;

    private LevelSystem levelSystem;
    private float targetFill = 1f;
    private float currentFill = 1f;
    private const float SMOOTH_SPEED = 5f;

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

    // ===== PUBLICZNA METODA DLA PLAYERSTATS =====
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

        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        targetFill = currentHealth / maxHealth;
        UpdateUI();
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        targetFill = 1f;
        UpdateUI();
    }

    public void AddArmor(int amount) => armor += amount;
    public void LevelUpHealth() => AddMaxHealth(5);

    // ===== PUBLICZNA METODA DLA UI =====
    public void UpdateUI()
    {
        if (healthFill != null) targetFill = currentHealth / maxHealth;
        if (healthText != null) healthText.text = $"{Mathf.Round(currentHealth)} / {Mathf.Round(maxHealth)}";
        if (levelSystem != null) levelSystem.UpdateUI();
    }

    void Die()
    {
        Debug.Log("Player died!");
        Time.timeScale = 0f;
        Destroy(gameObject);
    }
}