using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float HeathValue = 100f;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    void Start()
    {
        maxHealth = HeathValue;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        HeathValue -= damage;
        HeathValue = Mathf.Clamp(HeathValue, 0f, maxHealth);
        UpdateHealthUI();

        if (HeathValue <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void UpdateHealthUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = HeathValue / maxHealth;
        if (healthText != null)
            healthText.text = Mathf.Round(HeathValue) + " / " + maxHealth;
    }
}