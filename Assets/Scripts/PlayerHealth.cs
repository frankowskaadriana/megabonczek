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
        UpdateUI();
        Debug.Log($"PlayerHealth initialized: {currentHealth}/{maxHealth}");
    }

    public void SetBaseHealth(float health, float initialArmor = 0)
    {
        maxHealth = health;
        currentHealth = health;
        armor = initialArmor;
        UpdateUI();
        Debug.Log($"SetBaseHealth: {currentHealth}/{maxHealth}, Armor: {armor}");
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        UpdateUI();
        Debug.Log($"Dodano {amount} HP. Nowe max zdrowie: {maxHealth}");
    }

    public void AddArmor(int amount)
    {
        armor += amount;
        UpdateUI();
        Debug.Log($"Dodano {amount} pancerza. Aktualny pancerz: {armor}");
    }

    public void LevelUpHealth()
    {
        maxHealth += 5f;
        currentHealth = maxHealth;
        UpdateUI();
        Debug.Log($"Level up! Nowe zdrowie: {maxHealth}");
    }

    public void TakeDamage(float damage)
    {
        Debug.Log($"TakeDamage wywołane! Obrażenia: {damage}, Aktualne HP: {currentHealth}/{maxHealth}");

        float reducedDamage = damage * (1f - armor / 100f);
        currentHealth -= reducedDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // WAŻNE: Wywołaj UpdateUI natychmiast po zmianie zdrowia
        UpdateUI();

        Debug.Log($"Po obrażeniach: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            Debug.Log("Warunek śmierci spełniony! Wywołuję Die()");
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateUI();
        Debug.Log($"Leczenie: +{amount} HP, Aktualne HP: {currentHealth}/{maxHealth}");
    }

    public void UpdateUI()
    {
        Debug.Log($"UpdateUI - currentHealth: {currentHealth}, maxHealth: {maxHealth}");

        if (healthFill != null)
        {
            healthFill.fillAmount = currentHealth / maxHealth;
            Debug.Log($"healthFill ustawiony na {currentHealth / maxHealth}");
        }
        else
        {
            Debug.LogError("healthFill jest NULL! Przeciągnij Image Fill w Inspektorze!");
        }

        if (healthText != null)
        {
            healthText.text = Mathf.Round(currentHealth) + " / " + Mathf.Round(maxHealth);
            Debug.Log($"healthText ustawiony na {healthText.text}");
        }
        else
        {
            Debug.LogError("healthText jest NULL! Przeciągnij TextMeshPro w Inspektorze!");
        }
    }

    void Die()
    {
        Debug.Log("!!! PLAYER DIED !!!");
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Destroy(gameObject, 0.1f);
    }

    // TEST - zadaj obrażenia klawiszem H
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("========== KLAWISZ H - TEST OBRAŻEŃ ==========");
            TakeDamage(20f);
        }
    }
}