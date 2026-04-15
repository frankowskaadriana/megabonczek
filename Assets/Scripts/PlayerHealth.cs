using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("PlayerHealth")]
    public float maxHealth = 100f;
    public float HeathValue = 100f;
    public float damageAmount = 20f;
    public bool isInvincible = false;

    [Header("UI References")]
    public Image healthFill;
    public TextMeshProUGUI healthText;
    public Text healthTextLegacy;

    void Start()
    {
        maxHealth = HeathValue;
        UpdateHealthUI();
        Debug.Log("PlayerHealth started with " + HeathValue + " health");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamage(10f);
        }
    }

    public void TakeDamage(float damage)
    {
        Debug.Log("TakeDamage called. Damage: " + damage + ", isInvincible: " + isInvincible);

        if (isInvincible)
        {
            Debug.Log("Berserk active! No damage taken!");
            return;
        }

        HeathValue -= damage;
        HeathValue = Mathf.Clamp(HeathValue, 0f, maxHealth);
        Debug.Log("Health now: " + HeathValue);

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
        Debug.Log("Healed for " + amount + "! Health now: " + HeathValue);
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
            Debug.Log("Hit by enemy! Taking " + damageAmount + " damage");
            TakeDamage(damageAmount);
        }
    }
}