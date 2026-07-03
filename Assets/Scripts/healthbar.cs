using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("═══════════════ REFERENCJE ═══════════════")]
    public Image healthFillImage;
    public TextMeshProUGUI healthText;

    [Header("═══════════════ KOLORY ═══════════════")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;

    [Header("═══════════════ USTAWIENIA ═══════════════")]
    public float smoothSpeed = 10f;

    private PlayerHealth playerHealth;
    private float currentFill = 1f;
    private float targetFill = 1f;
    private float searchTimer = 0f;
    private float searchInterval = 0.2f; // Szybciej szuka

    void Start()
    {
        FindPlayerHealth();

        if (healthFillImage != null)
        {
            currentFill = 1f;
            targetFill = 1f;
            healthFillImage.fillAmount = 1f;
        }

        UpdateHealthBar();
    }

    void FindPlayerHealth()
    {
        // ============================================================
        // SZUKAJ PlayerHealth NA SCENIE
        // ============================================================
        playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            // Spróbuj znaleźć na graczu
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (playerHealth != null)
        {
            Debug.Log("✅ HealthBar znalazł PlayerHealth!");
        }
    }

    void Update()
    {
        // ============================================================
        // CIĄGLE SZUKAJ PlayerHealth (JEŚLI NIE MA)
        // ============================================================
        if (playerHealth == null)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchInterval)
            {
                searchTimer = 0f;
                FindPlayerHealth();
            }
            return;
        }

        // ============================================================
        // ZAWSZE AKTUALIZUJ TARGET (CO KLATKĘ)
        // ============================================================
        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        targetFill = Mathf.Clamp01(healthPercent);

        // ============================================================
        // PŁYNNA ANIMACJA PASKA
        // ============================================================
        if (healthFillImage != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
            healthFillImage.fillAmount = currentFill;

            // KOLOR
            if (targetFill > 0.6f)
                healthFillImage.color = healthyColor;
            else if (targetFill > 0.3f)
                healthFillImage.color = warningColor;
            else
                healthFillImage.color = dangerColor;
        }

        // ============================================================
        // ZAWSZE AKTUALIZUJ TEKST (CO KLATKĘ)
        // ============================================================
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(playerHealth.currentHealth)} / {Mathf.Round(playerHealth.maxHealth)}";
        }
    }

    public void UpdateHealthBar()
    {
        if (playerHealth == null)
        {
            FindPlayerHealth();
            if (playerHealth == null) return;
        }

        float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
        targetFill = Mathf.Clamp01(healthPercent);

        if (healthFillImage != null)
        {
            currentFill = targetFill;
            healthFillImage.fillAmount = currentFill;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(playerHealth.currentHealth)} / {Mathf.Round(playerHealth.maxHealth)}";
        }

        Debug.Log($"❤️ HealthBar odświeżony: {playerHealth.currentHealth}/{playerHealth.maxHealth}");
    }

    public void Refresh()
    {
        UpdateHealthBar();
    }
}