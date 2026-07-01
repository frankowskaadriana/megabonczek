using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("??????????????? REFERENCJE ???????????????")]
    public LevelSystem levelSystem;
    public PlayerHealth playerHealth;
    public WaveSpawner waveSpawner;

    [Header("??????????????? PASKI ???????????????")]
    public Image healthFill;
    public Image xpFill;

    [Header("??????????????? TEKSTY ???????????????")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;

    [Header("??????????????? KOLORY PASKA ZDROWIA ???????????????")]
    public Color healthColorGreen = Color.green;
    public Color healthColorYellow = Color.yellow;
    public Color healthColorRed = Color.red;

    [Header("??????????????? KOLORY PASKA XP ???????????????")]
    public Color xpColorNormal = new Color(0.2f, 0.6f, 1f);
    public Color xpColorAlmost = new Color(1f, 0.8f, 0f);
    public Color xpColorFull = new Color(1f, 0.5f, 0f);

    [Header("??????????????? USTAWIENIA ???????????????")]
    public float smoothSpeed = 5f;

    private float currentHealthFill = 1f;
    private float currentXpFill = 0f;
    private float targetHealthFill = 1f;
    private float targetXpFill = 0f;

    void Start()
    {
        // Szukaj referencji jeœli nie przypisano
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

        // Szukaj gracza i jego PlayerHealth
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        // Ustaw pocz¹tkowe wartoœci pasków
        if (healthFill != null)
        {
            targetHealthFill = 1f;
            currentHealthFill = 1f;
            healthFill.fillAmount = 1f;
        }

        if (xpFill != null)
        {
            targetXpFill = 0f;
            currentXpFill = 0f;
            xpFill.fillAmount = 0f;
        }

        UpdateUI();
        Debug.Log("? PlayerUI na GameManager gotowy!");
    }

    void Update()
    {
        // Szukaj gracza jeœli znikn¹³
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerHealth = player.GetComponent<PlayerHealth>();
        }

        // P³ynne animacje pasków
        if (healthFill != null)
        {
            currentHealthFill = Mathf.Lerp(currentHealthFill, targetHealthFill, Time.deltaTime * smoothSpeed);
            healthFill.fillAmount = currentHealthFill;
        }

        if (xpFill != null)
        {
            currentXpFill = Mathf.Lerp(currentXpFill, targetXpFill, Time.deltaTime * smoothSpeed);
            xpFill.fillAmount = currentXpFill;
        }

        // Aktualizuj UI co klatkê
        UpdateUI();
    }

    public void UpdateUI()
    {
        // === ZDROWIE ===
        if (playerHealth != null)
        {
            float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
            targetHealthFill = healthPercent;

            // Kolor paska zdrowia
            if (healthFill != null)
            {
                if (healthPercent > 0.6f)
                    healthFill.color = healthColorGreen;
                else if (healthPercent > 0.3f)
                    healthFill.color = healthColorYellow;
                else
                    healthFill.color = healthColorRed;
            }

            // Tekst zdrowia
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Round(playerHealth.currentHealth)} / {Mathf.Round(playerHealth.maxHealth)}";
            }
        }

        // === XP ===
        if (levelSystem != null)
        {
            float xpPercent = (float)levelSystem.currentXP / levelSystem.xpRequired;
            targetXpFill = xpPercent;

            // Kolor paska XP
            if (xpFill != null)
            {
                if (xpPercent > 0.8f)
                    xpFill.color = xpColorAlmost;
                else if (xpPercent > 0.95f)
                    xpFill.color = xpColorFull;
                else
                    xpFill.color = xpColorNormal;
            }

            // Tekst XP
            if (xpText != null)
            {
                xpText.text = $"{levelSystem.currentXP} / {levelSystem.xpRequired} XP";
            }

            // Poziom
            if (levelText != null)
            {
                levelText.text = $"Poziom {levelSystem.currentLevel}";
            }

            // Fala
            if (waveText != null && waveSpawner != null)
            {
                waveText.text = $"Fala {waveSpawner.GetCurrentWave()}";
            }
        }
    }

    // ============================================
    // METODY DLA ZEWNÊTRZNYCH SKRYPTÓW
    // ============================================

    public void RefreshUI()
    {
        UpdateUI();
    }

    public void SetLevelSystem(LevelSystem system)
    {
        levelSystem = system;
        UpdateUI();
    }

    public void SetPlayerHealth(PlayerHealth health)
    {
        playerHealth = health;
        UpdateUI();
    }

    public void SetWaveSpawner(WaveSpawner spawner)
    {
        waveSpawner = spawner;
        UpdateUI();
    }
}