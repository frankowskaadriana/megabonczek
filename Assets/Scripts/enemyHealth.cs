using UnityEngine;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float health = 50f;
    public int expReward = 20;

    [Header("UI References")]
    public TextMeshPro healthText;

    void Start()
    {
        UpdateHealthText();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthText();

        Debug.Log($"{gameObject.name} otrzyma³ {damage} obra¿eñ. Pozosta³e zdrowie: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = Mathf.Round(health).ToString();
        }
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} zgin¹³!");

        // ZnajdŸ LevelMechanic na graczu i dodaj EXP
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            LevelMechanic levelMechanic = player.GetComponent<LevelMechanic>();
            if (levelMechanic != null)
            {
                levelMechanic.AddExp(expReward);
                Debug.Log($"Zdobyto {expReward} EXP za zabicie {gameObject.name}!");
            }
            else
            {
                Debug.LogWarning("Nie znaleziono LevelMechanic na graczu!");
            }
        }

        Destroy(gameObject);
    }
}
