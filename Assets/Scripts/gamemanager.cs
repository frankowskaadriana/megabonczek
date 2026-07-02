using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("═══════════════ UI PANELE ═══════════════")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel; // ← TO SIĘ WŁĄCZA PRZY ZWYCIĘSTWIE
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI finalLevelText;
    public TextMeshProUGUI finalWaveText;
    public TextMeshProUGUI finalScoreText;

    [Header("═══════════════ PASKI ═══════════════")]
    public Image healthFill;
    public Image xpFill;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;

    [Header("═══════════════ KOLORY ═══════════════")]
    public Color healthColorGreen = Color.green;
    public Color healthColorYellow = Color.yellow;
    public Color healthColorRed = Color.red;
    public Color xpColorNormal = new Color(0.2f, 0.6f, 1f);
    public Color xpColorAlmost = new Color(1f, 0.8f, 0f);
    public Color xpColorFull = new Color(1f, 0.5f, 0f);

    [Header("═══════════════ USTAWIENIA ═══════════════")]
    public float smoothSpeed = 5f;
    public float gameDuration = 600f; // Czas gry (10 minut)

    [Header("═══════════════ PORTAL ═══════════════")]
    public GameObject portalPrefab;
    public Vector3 portalSpawnPosition = new Vector3(0, 0, 0);

    [Header("═══════════════ KOMUNIKAT PORTALU ═══════════════")]
    public GameObject portalMessagePanel;
    public TextMeshProUGUI portalMessageText;
    public float portalMessageDuration = 5f;

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public WaveSpawner waveSpawner;

    private LevelSystem levelSystem;
    private PlayerHealth playerHealth;
    private GameObject player;
    private float gameTimer = 0f;
    private float score = 0f;
    private float scoreMultiplier = 1f;
    private bool isGameActive = false;
    private bool isPaused = false;
    private int totalEnemiesKilled = 0;

    private float currentHealthFill = 1f;
    private float currentXpFill = 0f;
    private float targetHealthFill = 1f;
    private float targetXpFill = 0f;

    // ===== PORTAL =====
    private PortalTrigger portalTrigger;
    private bool isGameFinished = false;
    private bool portalSpawned = false;

    void Start()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        // Ukryj wszystkie panele
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (portalMessagePanel != null) portalMessagePanel.SetActive(false);

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

        StartGame();
        UpdateUI();
    }

    void Update()
    {
        if (!isGameActive) return;

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player != null) playerHealth = player.GetComponent<PlayerHealth>();
        }

        gameTimer += Time.deltaTime;
        score += Time.deltaTime * scoreMultiplier;

        // === SPAWN PORTALU PO CZASIE ===
        if (gameTimer >= gameDuration && !portalSpawned && !isGameFinished)
        {
            SpawnPortal();
        }

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

        // Jeśli czas minął - nie sprawdzaj innych warunków końca gry
        if (gameTimer >= gameDuration)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();

        if (playerHealth != null && playerHealth.currentHealth <= 0)
            GameOver("💀 ZGINĄŁEŚ!");

        if (waveSpawner != null && waveSpawner.IsWaveComplete() && waveSpawner.GetEnemyCount() == 0)
            Victory("🏆 WSZYSTKIE FALE UKOŃCZONE!");

        UpdateUI();
    }

    void StartGame()
    {
        isGameActive = true;
        gameTimer = 0f;
        Time.timeScale = 1f;
        if (levelSystem != null) levelSystem.StartGame();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void EnemyKilled()
    {
        totalEnemiesKilled++;
        scoreMultiplier = Mathf.Min(1f + totalEnemiesKilled * 0.01f, 5f);
        if (waveSpawner != null) waveSpawner.EnemyDied();
        if (levelSystem != null) levelSystem.EnemyDied();
        UpdateUI();
    }

    public void GameOver(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverText != null) gameOverText.text = reason;
            if (finalLevelText != null && levelSystem != null) finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null) finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null) finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
        }
    }

    public void Victory(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameFinished = true;
        Time.timeScale = 0f;

        // === WŁĄCZ VICTORY SCREEN ===
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null) victoryText.text = reason;
            if (finalLevelText != null && levelSystem != null) finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null) finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null) finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
        }

        Debug.Log($"🏆 VICTORY! {reason}");
        AudioManager.Instance?.PlayVictory();
    }

    // ============================================
    // PORTAL
    // ============================================

    void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("⚠️ Brak prefabu portalu!");
            return;
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }

        Vector3 spawnPos = portalSpawnPosition;
        if (player != null)
        {
            Vector3 direction = player.transform.forward;
            spawnPos = player.transform.position + direction * 12f;
            spawnPos.y = 0.5f;
        }

        GameObject portalObj = Instantiate(portalPrefab, spawnPos, Quaternion.identity);
        portalTrigger = portalObj.GetComponent<PortalTrigger>();

        if (portalTrigger == null)
        {
            portalTrigger = portalObj.AddComponent<PortalTrigger>();
        }

        portalTrigger.gameManager = this;

        // Szukaj PortalON w dzieciach
        Transform portalON = portalObj.transform.Find("PortalON");
        if (portalON != null)
        {
            portalTrigger.portalON = portalON.gameObject;
            Debug.Log("🌀 Znaleziono PortalON w prefabie!");
        }

        portalSpawned = true;
        isGameFinished = true;

        Debug.Log($"🌀 Portal spawnnięty na pozycji: {spawnPos}");

        // Wywołaj aktywację portalu
        if (portalTrigger != null)
        {
            portalTrigger.ForceActivate();
        }
    }

    public bool IsGameFinished()
    {
        return isGameFinished;
    }

    public void ShowPortalMessage(string message)
    {
        if (portalMessagePanel != null)
        {
            portalMessagePanel.SetActive(true);
            if (portalMessageText != null)
            {
                portalMessageText.text = message;
            }
            StartCoroutine(HidePortalMessage());
        }
        else
        {
            Debug.Log($"📢 {message}");
        }
    }

    IEnumerator HidePortalMessage()
    {
        yield return new WaitForSeconds(portalMessageDuration);
        if (portalMessagePanel != null)
        {
            portalMessagePanel.SetActive(false);
        }
    }

    // ============================================
    // UI
    // ============================================

    public void UpdateUI()
    {
        if (playerHealth != null)
        {
            float healthPercent = playerHealth.currentHealth / playerHealth.maxHealth;
            targetHealthFill = healthPercent;

            if (healthFill != null)
            {
                if (healthPercent > 0.6f)
                    healthFill.color = healthColorGreen;
                else if (healthPercent > 0.3f)
                    healthFill.color = healthColorYellow;
                else
                    healthFill.color = healthColorRed;
            }

            if (healthText != null)
            {
                healthText.text = $"{Mathf.Round(playerHealth.currentHealth)} / {Mathf.Round(playerHealth.maxHealth)}";
            }
        }

        if (levelSystem != null)
        {
            float xpPercent = (float)levelSystem.currentXP / levelSystem.xpRequired;
            targetXpFill = xpPercent;

            if (xpFill != null)
            {
                if (xpPercent > 0.8f)
                    xpFill.color = xpColorAlmost;
                else if (xpPercent > 0.95f)
                    xpFill.color = xpColorFull;
                else
                    xpFill.color = xpColorNormal;
            }

            if (xpText != null)
            {
                xpText.text = $"{levelSystem.currentXP} / {levelSystem.xpRequired} XP";
            }

            if (levelText != null)
            {
                levelText.text = $"Poziom {levelSystem.currentLevel}";
            }

            if (waveText != null && waveSpawner != null)
            {
                waveText.text = $"Fala {waveSpawner.GetCurrentWave()}";
            }
        }
    }

    // ============================================
    // METODY PUBLICZNE
    // ============================================

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void ResumeGame()
    {
        if (isPaused) TogglePause();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }

    public float GetScore() => score;
    public int GetScoreInt() => Mathf.RoundToInt(score);
    public GameObject GetPlayer() => player;
    public float GetGameTime() => gameTimer;

    // ============================================
    // PRZYCISKI UI
    // ============================================

    public void OnRestartButton() => RestartGame();
    public void OnMainMenuButton() => LoadMainMenu();
    public void OnResumeButton() => ResumeGame();
    public void OnQuitButton() => QuitGame();

    // ============================================
    // METODY TESTOWE
    // ============================================

    public void SpawnTestBoss() => Debug.Log("👑 Test Boss");
    public void SpawnTestBazyliszek() => Debug.Log("🐉 Test Bazyliszek");
    public void SkipWave() => Debug.Log("⏭️ Skip Wave");
    public void ClearEnemies() => Debug.Log("🗑️ Clear Enemies");
}