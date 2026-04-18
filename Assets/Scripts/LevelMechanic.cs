using UnityEngine;
using TMPro;
using System.Collections;

public class LevelSystem : MonoBehaviour
{
    [Header("Character Selection")]
    public GameObject mountainManPrefab;
    public GameObject angelPrefab;
    public Transform playerSpawnPoint;

    [Header("Camera")]
    public CameraController cameraController;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject characterSelectPanel;
    public TextMeshProUGUI characterSelectText;

    [Header("Player References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;

    private GameObject currentPlayer;
    private int enemiesAlive = 0;
    private int enemiesKilled = 0;
    private bool isRespawning = false;
    private bool gameStarted = false;

    void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Przeciagnij prefab wroga do Enemy Prefab!");
            return;
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }

        ShowCharacterSelection();
    }

    void ShowCharacterSelection()
    {
        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(true);
            if (characterSelectText != null)
            {
                characterSelectText.text = "Wybierz postac:\n\n1 - Goral (Mountain Man)\n2 - Aniol (Angel)";
            }
        }
        Time.timeScale = 0f;
        Debug.Log("Wyswietlono panel wyboru postaci. Wcisnij 1 lub 2.");
    }

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Wcisnieto klawisz 1 - wybor Gorala");
                SelectMountainMan();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Wcisnieto klawisz 2 - wybor Aniola");
                SelectAngel();
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Debug.Log("Wcisnieto klawisz numpad 1 - wybor Gorala");
                SelectMountainMan();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Debug.Log("Wcisnieto klawisz numpad 2 - wybor Aniola");
                SelectAngel();
            }

            return;
        }

        if (enemiesAlive <= 0 && !isRespawning && enemiesKilled >= enemiesToKill)
        {
            LevelUp();
        }
    }

    void SelectMountainMan()
    {
        Debug.Log("SelectMountainMan - rozpoczynam");

        if (mountainManPrefab == null)
        {
            Debug.LogError("mountainManPrefab nie jest przypisany!");
            return;
        }

        SpawnPlayer(mountainManPrefab, 100f);
        gameStarted = true;

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        UpdateUI();
        StartCoroutine(SpawnEnemies());

        Debug.Log("Goral wybrany - gra rozpoczeta");
    }

    void SelectAngel()
    {
        Debug.Log("SelectAngel - rozpoczynam");

        if (angelPrefab == null)
        {
            Debug.LogError("angelPrefab nie jest przypisany!");
            return;
        }

        SpawnPlayer(angelPrefab, 40f);
        gameStarted = true;

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        Time.timeScale = 1f;
        UpdateUI();
        StartCoroutine(SpawnEnemies());

        Debug.Log("Aniol wybrany - gra rozpoczeta");
    }

    void SpawnPlayer(GameObject playerPrefab, float baseHealth)
    {
        Debug.Log("SpawnPlayer - tworzenie postaci");

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        currentPlayer.tag = "Player";

        // Dodaj komponent ruchu
        PlayerMovement movement = currentPlayer.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            movement = currentPlayer.AddComponent<PlayerMovement>();
        }

        if (currentPlayer.name.ToLower().Contains("angel"))
        {
            movement.maxSpeed = 6f;
        }
        else
        {
            movement.maxSpeed = 5f;
        }

        // NIE ustawiamy cameraController.target - kamera sama znajdzie gracza po tagu

        playerHealth = currentPlayer.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.SetBaseHealth(baseHealth, currentLevel);
        }

        AbilitiesMountainMan abilities = currentPlayer.GetComponent<AbilitiesMountainMan>();
        if (abilities != null && weaponUpgrade != null)
        {
            abilities.weaponUpgrade = weaponUpgrade;
            abilities.playerHealth = playerHealth;
        }

        AngelAbilities angelAbilities = currentPlayer.GetComponent<AngelAbilities>();
        if (angelAbilities != null && weaponUpgrade != null)
        {
            angelAbilities.weaponUpgrade = weaponUpgrade;
            angelAbilities.playerHealth = playerHealth;
        }

        Debug.Log("Postac utworzona: " + currentPlayer.name);
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