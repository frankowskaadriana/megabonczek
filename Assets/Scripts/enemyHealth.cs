using UnityEngine;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    public float health = 50f;
    public LevelSystem levelSystem;
    public TextMeshPro healthText; // 3D Text nad wrogiem
    public GameObject bloodEffect; // Efekt krwi (opcjonalny)

    private float maxHealth;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        maxHealth = health;
        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
            originalColor = meshRenderer.material.color;

        if (levelSystem != null)
        {
            health = 50f + (levelSystem.currentLevel - 1) * 10f;
            maxHealth = health;
        }

        UpdateHealthText();
    }

    void Update()
    {
        // Tekst zawsze patrzy na kamerê
        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthText();

        // Efekt wizualny - miganie na czerwono
        if (meshRenderer != null)
            StartCoroutine(FlashRed());

        // Efekt krwi (opcjonalny)
        if (bloodEffect != null)
            Instantiate(bloodEffect, transform.position, Quaternion.identity);

        if (health <= 0)
        {
            Die();
        }
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(health)}/{Mathf.Round(maxHealth)}";

            // Zmiana koloru tekstu w zale¿noœci od zdrowia
            if (health > maxHealth * 0.6f)
                healthText.color = Color.green;
            else if (health > maxHealth * 0.3f)
                healthText.color = Color.yellow;
            else
                healthText.color = Color.red;
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        if (levelSystem != null)
            levelSystem.EnemyDied();

        Destroy(gameObject);
    }
}