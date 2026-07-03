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

    [Header("═══════════════ WARUNEK WYGRANEJ ═══════════════")]
    public float winTime = 300f;
    public TextMeshProUGUI timerText;

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
    private bool hasWon = false;

    void Start()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        isGameActive = true;
        gameTimer = 0f;
        Debug.Log($"🎮 Gra rozpoczęta! Wygrana po {winTime} sekundach");
    }

    void Update()
    {
        if (!isGameActive) return;

        gameTimer += Time.deltaTime;
        score += Time.deltaTime * scoreMultiplier;

        // Timer UI
        if (timerText != null)
        {
            float remaining = winTime - gameTimer;
            if (remaining > 0)
            {
                int minutes = Mathf.FloorToInt(remaining / 60f);
                int seconds = Mathf.FloorToInt(remaining % 60f);
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
            else
            {
                timerText.text = "00:00";
            }
        }

        // ============================================================
        // SPRAWDŹ WARUNEK WYGRANEJ
        // ============================================================
        if (gameTimer >= winTime && !hasWon)
        {
            hasWon = true;
            Victory("⏰ PRZETRWAŁEŚ " + (winTime / 60f).ToString("F0") + " MINUT! ZWYCIĘSTWO!");
        }

        // Pauza
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();

        // Game Over
        if (playerHealth != null && playerHealth.currentHealth <= 0)
            GameOver("💀 ZGINĄŁEŚ!");

        UpdateUI();
    }

    // ============================================================
    // METODY PUBLICZNE
    // ============================================================

    public void UpdateUI()
    {
        // UI aktualizowane przez inne skrypty
    }

    public void Victory(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;
        hasWon = true;
        Time.timeScale = 0f;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null) victoryText.text = reason;
            if (finalLevelText != null && levelSystem != null)
                finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null)
                finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null)
                finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
        }

        AudioManager.Instance?.PlayVictory();
        Debug.Log($"🏆 VICTORY! {reason}");
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
            if (finalLevelText != null && levelSystem != null)
                finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null)
                finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null)
                finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
        }

        AudioManager.Instance?.PlayGameOver();
        Debug.Log($"💀 GAME OVER! {reason}");
    }

    public void EnemyKilled()
    {
        scoreMultiplier = Mathf.Min(1f + (Time.frameCount * 0.001f), 5f);
        if (waveSpawner != null) waveSpawner.EnemyDied();
        if (levelSystem != null) levelSystem.EnemyDied();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public float GetGameTime() => gameTimer;
    public float GetScore() => score;
    public int GetScoreInt() => Mathf.RoundToInt(score);
    public GameObject GetPlayer() => player;
    public bool HasWon() => hasWon;

    // ============================================================
    // PRZYCISKI UI
    // ============================================================

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
}