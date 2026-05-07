using UnityEngine;
using TMPro;
using System.Collections;

public class LevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;

    [Header("Player References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;

    private int enemiesAlive = 0;
    private int enemiesKilled = 0;
    private bool isRespawning = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Przeciagnij prefab wroga do Enemy Prefab!");
            return;
        }

        UpdateUI();
        StartCoroutine(SpawnEnemies());
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
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Enemy_" + (i + 1);

            enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
            if (enemyScript != null)
                enemyScript.levelSystem = this;

            yield return new WaitForSeconds(0.3f);
        }

        isRespawning = false;
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

        if (playerHealth != null)
        {
            playerHealth.LevelUpHealth();
        }

        if (weaponUpgrade != null)
        {
            int randomUpgrade = Random.Range(0, 7);
            switch (randomUpgrade)
            {
                case 0: weaponUpgrade.UpgradeDamage(); break;
                case 1: weaponUpgrade.UpgradeRange(); break;
                case 2: weaponUpgrade.UpgradeSpecialDamage(); break;
                case 3: weaponUpgrade.UpgradeSpecialCooldown(); break;
                case 4: weaponUpgrade.UpgradeSpecialRotations(); break;
                case 5: weaponUpgrade.UpgradeUltimateDamage(); break;
                case 6: weaponUpgrade.UpgradeUltimateRadius(); break;
            }
        }

        StartCoroutine(SpawnEnemies());
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = "Level: " + currentLevel;
        if (enemiesLeftText != null)
            enemiesLeftText.text = "Enemies: " + enemiesAlive;
    }
}