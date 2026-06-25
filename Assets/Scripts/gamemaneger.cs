using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("═══════════════ STAN GRY ═══════════════")]
    public bool isGameRunning = false;
    public bool isPaused = false;
    public float gameTime = 0f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public WaveSpawner waveSpawner;
    public AudioManager audioManager;
    public CameraController cameraController;
    public CharacterSelector characterSelector;

    private float previousTimeScale = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("🎮 GameManager utworzony");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        FindMissingReferences();
        Debug.Log("🎮 GameManager gotowy! Wciśnij B aby spawnić testowego bossa");
    }

    void Update()
    {
        if (isGameRunning && !isPaused)
            gameTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        if (Input.GetKeyDown(KeyCode.B) && waveSpawner != null)
            waveSpawner.TestSpawnBoss();
    }

    void FindMissingReferences()
    {
        if (levelSystem == null) levelSystem = GetComponent<LevelSystem>();
        if (waveSpawner == null) waveSpawner = GetComponent<WaveSpawner>();
        if (audioManager == null) audioManager = GetComponent<AudioManager>();
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();
        if (characterSelector == null) characterSelector = FindFirstObjectByType<CharacterSelector>();
    }

    public void StartGame()
    {
        if (isGameRunning) return;
        isGameRunning = true;
        isPaused = false;
        gameTime = 0f;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (levelSystem != null) levelSystem.StartGame();
        if (waveSpawner != null) waveSpawner.enabled = true;
        Debug.Log("🚀 GRA ROZPOCZĘTA!");
    }

    public void EndGame()
    {
        isGameRunning = false;
        Time.timeScale = 0f;
        Debug.Log("💀 GRA ZAKOŃCZONA!");
    }

    public void PauseGame()
    {
        if (!isGameRunning || isPaused) return;
        isPaused = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("⏸️ PAUZA");
    }

    public void ResumeGame()
    {
        if (!isGameRunning || !isPaused) return;
        isPaused = false;
        Time.timeScale = previousTimeScale;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("▶️ GRA WZNOWIONA");
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void RestartGame()
    {
        Debug.Log("🔄 RESTART...");
        if (waveSpawner != null) waveSpawner.ClearAllEnemies();
        isGameRunning = false;
        isPaused = false;
        gameTime = 0f;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame()
    {
        Debug.Log("👋 ZAMYKANIE GRY");
        Application.Quit();
    }
}