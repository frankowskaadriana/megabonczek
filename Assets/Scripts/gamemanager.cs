using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("═══════════════ UI PANELE ═══════════════")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
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
    public float gameDuration = 600f;

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

    void Start()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

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

        if (gameTimer >= gameDuration)
        {
            Victory("⏰ CZAS MINĄŁ!");
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
        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null) victoryText.text = reason;
            if (finalLevelText != null && levelSystem != null) finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null) finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null) finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
        }
    }

    public void UpdateUI()
    {
        // === ZDROWIE ===
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

        // === XP ===
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

    public void OnRestartButton() => RestartGame();
    public void OnMainMenuButton() => LoadMainMenu();
    public void OnResumeButton() => ResumeGame();
    public void OnQuitButton() => QuitGame();

    public void SpawnTestBoss() => Debug.Log("👑 Test Boss");
    public void SpawnTestBazyliszek() => Debug.Log("🐉 Test Bazyliszek");
    public void SkipWave() => Debug.Log("⏭️ Skip Wave");
    public void ClearEnemies() => Debug.Log("🗑️ Clear Enemies");
}