using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject gameOverPanel;
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI finalLevelText;
    public TextMeshProUGUI finalWaveText;
    public TextMeshProUGUI finalScoreText;

    [Header("Czas gry")]
    public float gameDuration = 600f;
    private float gameTimer = 0f;

    [Header("Referencje")]
    public WaveSpawner waveSpawner;

    private LevelSystem levelSystem;
    private PlayerHealth playerHealth;
    private GameObject player;
    private float score = 0f;
    private float scoreMultiplier = 1f;
    private bool isGameActive = false;
    private bool isPaused = false;
    private int totalEnemiesKilled = 0;

    void Start()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();
        waveSpawner = FindFirstObjectByType<WaveSpawner>();
        player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        StartGame();
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

        // Sprawdź czy czas minął
        if (gameTimer >= gameDuration)
        {
            Victory("⏰ CZAS MINĄŁ!");
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();

        if (playerHealth != null && playerHealth.currentHealth <= 0)
            GameOver("💀 ZGINĄŁEŚ!");

        // Sprawdź czy wszystkie fale ukończone
        if (waveSpawner != null && waveSpawner.IsWaveComplete() && waveSpawner.GetEnemyCount() == 0)
        {
            Victory("🏆 WSZYSTKIE FALE UKOŃCZONE!");
        }
    }

    void StartGame()
    {
        isGameActive = true;
        gameTimer = 0f;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (levelSystem != null) levelSystem.StartGame();
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void EnemyKilled()
    {
        totalEnemiesKilled++;
        scoreMultiplier = Mathf.Min(1f + totalEnemiesKilled * 0.01f, 5f);
        if (waveSpawner != null) waveSpawner.EnemyDied();
        if (levelSystem != null) levelSystem.EnemyDied();
    }

    public void GameOver(string reason)
    {
        if (!isGameActive) return;
        isGameActive = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryText != null) victoryText.text = reason;
            if (finalLevelText != null && levelSystem != null) finalLevelText.text = $"Poziom: {levelSystem.currentLevel}";
            if (finalWaveText != null && waveSpawner != null) finalWaveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (finalScoreText != null) finalScoreText.text = $"Wynik: {Mathf.RoundToInt(score)}";
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

    public float GetGameTime() => gameTimer;
    public float GetGameDuration() => gameDuration;
    public float GetScore() => score;
    public int GetScoreInt() => Mathf.RoundToInt(score);
    public GameObject GetPlayer() => player;

    public void OnRestartButton() => RestartGame();
    public void OnMainMenuButton() => LoadMainMenu();
    public void OnResumeButton() => ResumeGame();
    public void OnQuitButton() => QuitGame();
    public void SpawnTestBoss() => waveSpawner?.TestSpawnBoss();
    public void SpawnTestBazyliszek() => waveSpawner?.TestSpawnBazyliszek();
    public void SkipWave() => waveSpawner?.SkipWave();
    public void ClearEnemies() => waveSpawner?.ClearAllEnemies();
}