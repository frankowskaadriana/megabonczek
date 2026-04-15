using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;
    public int enemiesKilled = 0;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject levelUpPanel; // Panel z wyborem ulepszeñ
    public Button damageButton;     // Przycisk obra¿enia
    public Button healthButton;     // Przycisk zdrowie
    public Button spinButton;       // Przycisk ulepszenie spina

    [Header("Player References")]
    public PlayerHealth playerHealth;
    public AbilitiesMountainMan abilities;

    [Header("Upgrades")]
    public int damageUpgradeLevel = 0;
    public int healthUpgradeLevel = 0;
    public int spinUpgradeLevel = 0;

    private bool isRespawning = false;
    private int enemiesAlive = 0;

    void Start()
    {
        UpdateUI();
        StartCoroutine(SpawnEnemies());

        // Podepnij przyciski
        if (damageButton != null)
            damageButton.onClick.AddListener(() => UpgradeDamage());
        if (healthButton != null)
            healthButton.onClick.AddListener(() => UpgradeHealth());
        if (spinButton != null)
            spinButton.onClick.AddListener(() => UpgradeSpin());

        // Ukryj panel na start
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    void Update()
    {
        if (enemiesAlive <= 0 && !isRespawning && enemiesKilled >= enemiesToKill)
        {
            LevelUp();
        }
    }

    IEnumerator SpawnEnemies()
    {
        isRespawning = true;
        enemiesAlive = enemiesToKill;
        UpdateUI();

        for (int i = 0; i < enemiesToKill; i++)
        {
            Vector3 spawnPos = GetSpawnPosition();
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
            if (enemyScript != null)
            {
                enemyScript.levelSystem = this;
            }
            yield return new WaitForSeconds(0.3f);
        }

        isRespawning = false;
    }

    Vector3 GetSpawnPosition()
    {
        if (spawnPoints.Length > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }
        else
        {
            return transform.position + new Vector3(Random.Range(-7f, 7f), 0, Random.Range(-7f, 7f));
        }
    }

    public void EnemyDied()
    {
        enemiesAlive--;
        enemiesKilled++;
        UpdateUI();
    }

    void LevelUp()
    {
        currentLevel++;
        enemiesToKill += 2;
        enemiesKilled = 0;
        UpdateUI();

        // Poka¿ panel wyboru ulepszeñ
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f; // Zatrzymaj grê
        }
    }

    void UpgradeDamage()
    {
        damageUpgradeLevel++;
        // Zwiêksz obra¿enia spina
        if (abilities != null)
        {
            abilities.damage += 10f;
            Debug.Log($"Damage upgraded! Now: {abilities.damage}");
        }
        CloseLevelUpPanel();
    }

    void UpgradeHealth()
    {
        healthUpgradeLevel++;
        // Zwiêksz maksymalne zdrowie i ulecz gracza
        if (playerHealth != null)
        {
            playerHealth.maxHealth += 25f;
            playerHealth.HeathValue = playerHealth.maxHealth;
            playerHealth.UpdateHealthUI();
            Debug.Log($"Health upgraded! Max health: {playerHealth.maxHealth}");
        }
        CloseLevelUpPanel();
    }

    void UpgradeSpin()
    {
        spinUpgradeLevel++;
        // Ulepszenie spina
        if (abilities != null)
        {
            abilities.SpinRange += 1f;
            abilities.cooldown -= 0.5f;
            abilities.damage += 5f;
            Debug.Log($"Spin upgraded! Range: {abilities.SpinRange}, Cooldown: {abilities.cooldown}");
        }
        CloseLevelUpPanel();
    }

    void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f; // Odpal grê
        }

        // Rozpocznij nowy poziom
        StartCoroutine(SpawnEnemies());
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level: {currentLevel}";

        if (enemiesLeftText != null)
            enemiesLeftText.text = $"Enemies: {enemiesAlive}";
    }
}