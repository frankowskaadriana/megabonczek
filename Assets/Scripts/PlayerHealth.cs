using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    void Start()
    {
        // Statystyki zostan¹ ustawione przez PlayerStats
        UpdateUI();
    }

    public void SetBaseHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
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
}