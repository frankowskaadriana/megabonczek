using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    [Header("═══════════════ LEVEL SETTINGS ═══════════════")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;
    public int currentXP = 0;
    public int xpRequired = 10;

    [Header("═══════════════ ENEMY SETTINGS ═══════════════")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();
    public bool useRandomEnemies = true;
    public int currentEnemyIndex = 0;

    [Header("═══════════════ UI REFERENCES ═══════════════")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;
    public TextMeshProUGUI timerText;

    [Header("═══════════════ PERK SELECTION UI ═══════════════")]
    public GameObject perkSelectionPanel;
    public Button perkButton1;
    public Button perkButton2;
    public Button perkButton3;
    public TextMeshProUGUI perkText1;
    public TextMeshProUGUI perkText2;
    public TextMeshProUGUI perkText3;
    public TextMeshProUGUI perkDescription1;
    public TextMeshProUGUI perkDescription2;
    public TextMeshProUGUI perkDescription3;

    [Header("═══════════════ PLAYER REFERENCES ═══════════════")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public AbilitiesMountainMan mountainManAbilities;
    public SeraphimAbilities seraphimAbilities;
    public ShepherdAbilities shepherdAbilities;

    [Header("═══════════════ TIMER SYSTEM ═══════════════")]
    public float gameTime = 0f;
    public bool isTimerRunning = true;
    public bool showTimerInUI = true;

    [Header("═══════════════ PORTAL SYSTEM ═══════════════")]
    public GameObject portalObject;
    public float portalUnlockTime = 1800f;
    public bool isPortalUnlocked = false;
    public PortalTrigger portalTrigger;

    [Header("═══════════════ HORDE EVENT ═══════════════")]
    public bool enableHordeEvent = true;
    public float firstHordeTime = 300f;
    public float hordeInterval = 180f;
    public int hordeEnemyCount = 20;
    public float hordeSpawnRadius = 15f;

    [Header("═══════════════ BOSS EVENT ═══════════════")]
    public bool enableBossEvent = true;
    public float bossSpawnTime = 600f;
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public string bossName = "Leszy";

    private int enemiesAlive = 0;
    private int enemiesKilled = 0;
    private bool isRespawning = false;
    private bool gameStarted = false;
    private bool isPerkSelectionActive = false;
    private CursorLockMode previousLockMode;
    private bool isHordeSpawning = false;
    private bool isBossSpawned = false;
    private float nextHordeTime = 0f;

    private List<Perk> availablePerks = new List<Perk>();
    private List<Perk> currentPerks = new List<Perk>();

    [System.Serializable]
    public class Perk
    {
        public string name;
        public string description;
        public System.Action applyPerk;

        public Perk(string name, string description, System.Action apply)
        {
            this.name = name;
            this.description = description;
            this.applyPerk = apply;
        }
    }

    [System.Serializable]
    public class TimeEvent
    {
        public string eventName;
        public float triggerTime;
        public bool isTriggered = false;
        public UnityEngine.Events.UnityEvent onTrigger;
    }

    void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Dodaj przynajmniej jednego przeciwnika do listy Enemy Prefabs!");
            return;
        }

        CreatePerksList();
        UpdateUI();

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(false);

        if (perkButton1 != null)
            perkButton1.onClick.AddListener(() => SelectPerk(0));
        if (perkButton2 != null)
            perkButton2.onClick.AddListener(() => SelectPerk(1));
        if (perkButton3 != null)
            perkButton3.onClick.AddListener(() => SelectPerk(2));

        if (timerText != null && showTimerInUI)
            timerText.text = "00:00";

        if (portalObject != null) portalObject.SetActive(false);

        nextHordeTime = firstHordeTime;

        Debug.Log("Czekam na wybor postaci...");
    }

    GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs.Count == 0) return null;

        if (useRandomEnemies)
        {
            int randomIndex = Random.Range(0, enemyPrefabs.Count);
            return enemyPrefabs[randomIndex];
        }
        else
        {
            return enemyPrefabs[currentEnemyIndex];
        }
    }

    public void AddEnemyPrefab(GameObject newEnemy)
    {
        if (!enemyPrefabs.Contains(newEnemy))
        {
            enemyPrefabs.Add(newEnemy);
            Debug.Log($"Dodano nowego przeciwnika: {newEnemy.name}");
        }
    }

    void CreatePerksList()
    {
        availablePerks.Clear();

        availablePerks.Add(new Perk("⚔️ Obrażenia +10", "+10 do obrażeń broni", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
            Debug.Log("Perk: Obrażenia +10");
        }));

        availablePerks.Add(new Perk("📏 Zasięg +0.2m", "Zwiększa zasięg ataku", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
            Debug.Log("Perk: Zasięg +0.2m");
        }));

        availablePerks.Add(new Perk("❤️ Zdrowie +20", "+20 maksymalnego zdrowia", () => {
            if (playerHealth != null)
            {
                playerHealth.AddMaxHealth(20);
                Debug.Log("Perk: Zdrowie +20. Nowe HP: " + playerHealth.maxHealth);
            }
        }));

        availablePerks.Add(new Perk("🔄 Szybki cooldown", "-1s cooldown zdolności", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
            Debug.Log("Perk: Szybki cooldown -1s");
        }));

        availablePerks.Add(new Perk("💥 Mocniejsze obrażenia", "+15 obrażeń zdolności", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialDamage();
            Debug.Log("Perk: Mocniejsze obrażenia +15");
        }));

        availablePerks.Add(new Perk("👟 Szybkie nogi", "+10% prędkości ruchu", () => {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null) movement.maxSpeed += 0.5f;
            Debug.Log("Perk: Szybkie nogi +0.5 prędkości");
        }));

        availablePerks.Add(new Perk("🛡️ Pancerz +10", "+10 pancerza", () => {
            if (playerHealth != null) playerHealth.AddArmor(10);
            Debug.Log("Perk: Pancerz +10");
        }));

        availablePerks.Add(new Perk("⚡ Szybszy atak", "-0.1s czasu ataku", () => {
            if (mountainManAbilities != null)
                mountainManAbilities.attackRate = Mathf.Max(0.5f, mountainManAbilities.attackRate - 0.1f);
            Debug.Log("Perk: Szybszy atak");
        }));

        availablePerks.Add(new Perk("💉 Lifesteal", "1% lifestealu od obrażeń", () => {
            Debug.Log("Perk: Lifesteal aktywowany!");
        }));

        availablePerks.Add(new Perk("✨ Podwójne obrażenia", "Szansa na podwójne obrażenia", () => {
            Debug.Log("Perk: Szansa na podwójne obrażenia!");
        }));
    }

    void ShowPerkSelection()
    {
        Debug.Log("=== WYWOŁANO ShowPerkSelection - POZIOM " + currentLevel + " ===");

        isPerkSelectionActive = true;
        Time.timeScale = 0f;

        previousLockMode = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentPerks.Clear();
        List<Perk> tempPerks = new List<Perk>(availablePerks);

        while (currentPerks.Count < 3 && tempPerks.Count > 0)
        {
            int randomIndex = Random.Range(0, tempPerks.Count);
            currentPerks.Add(tempPerks[randomIndex]);
            tempPerks.RemoveAt(randomIndex);
        }

        if (perkText1 != null && currentPerks.Count > 0)
        {
            perkText1.text = currentPerks[0].name;
            if (perkDescription1 != null)
                perkDescription1.text = currentPerks[0].description;
        }

        if (perkText2 != null && currentPerks.Count > 1)
        {
            perkText2.text = currentPerks[1].name;
            if (perkDescription2 != null)
                perkDescription2.text = currentPerks[1].description;
        }

        if (perkText3 != null && currentPerks.Count > 2)
        {
            perkText3.text = currentPerks[2].name;
            if (perkDescription3 != null)
                perkDescription3.text = currentPerks[2].description;
        }

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(true);

        Debug.Log("═══════════════ WYBIERZ PERK ═══════════════");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log($"{i + 1}. {currentPerks[i].name} - {currentPerks[i].description}");
    }

    void HidePerkPanel()
    {
        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(false);
    }

    void SelectPerk(int index)
    {
        if (!isPerkSelectionActive) return;
        if (index < 0 || index >= currentPerks.Count) return;

        currentPerks[index].applyPerk();
        Debug.Log($"✅ WYBRANO PERK: {currentPerks[index].name}");

        isPerkSelectionActive = false;
        HidePerkPanel();

        Cursor.lockState = previousLockMode;
        Cursor.visible = false;
        Time.timeScale = 1f;

        StartCoroutine(SpawnEnemies());
    }

    void Update()
    {
        if (!gameStarted) return;

        if (isPerkSelectionActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectPerk(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectPerk(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectPerk(2);
            return;
        }

        if (isTimerRunning)
        {
            gameTime += Time.deltaTime;
            UpdateTimerUI();
            CheckPortalUnlock();
            CheckHordeEvent();
            CheckBossEvent();
        }

        if (enemiesAlive <= 0 && !isRespawning && enemiesKilled >= enemiesToKill)
        {
            LevelUp();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null && showTimerInUI)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void CheckPortalUnlock()
    {
        if (!isPortalUnlocked && gameTime >= portalUnlockTime)
        {
            isPortalUnlocked = true;
            if (portalObject != null)
                portalObject.SetActive(true);
            if (portalTrigger != null)
                portalTrigger.SetLevelSystem(this);
            Debug.Log("Portal odblokowany po " + FormatTime(gameTime) + "!");
        }
    }

    public void OnPortalEnter()
    {
        Debug.Log("Gracz wszedl do portalu! Zatrzymanie czasu...");
        isTimerRunning = false;
        Time.timeScale = 0f;
        StartCoroutine(EndGameSequence());
    }

    IEnumerator EndGameSequence()
    {
        yield return new WaitForSecondsRealtime(2f);
        Debug.Log("Koniec gry - osiagnieto portal!");
    }

    void CheckHordeEvent()
    {
        if (!enableHordeEvent) return;
        if (isHordeSpawning) return;
        if (gameTime >= nextHordeTime)
        {
            StartCoroutine(SpawnHorde());
            nextHordeTime += hordeInterval;
        }
    }

    IEnumerator SpawnHorde()
    {
        isHordeSpawning = true;
        Debug.Log("HORDA NADCHODZI! Liczba przeciwnikow: " + hordeEnemyCount);

        Vector3 center = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) center = player.transform.position;

        int spawnedCount = 0;
        for (int i = 0; i < hordeEnemyCount; i++)
        {
            Vector3 spawnPos = center + new Vector3(Random.Range(-hordeSpawnRadius, hordeSpawnRadius), 0, Random.Range(-hordeSpawnRadius, hordeSpawnRadius));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 10f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
                GameObject enemyToSpawn = GetRandomEnemyPrefab();
                if (enemyToSpawn != null)
                {
                    GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                    enemy.name = "Horde_Enemy_" + (i + 1);

                    enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                    if (enemyScript != null)
                        enemyScript.levelSystem = this;

                    spawnedCount++;
                }
            }

            if (i % 5 == 0) yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Horda sie pojawila! Spawnowano " + spawnedCount + " wrogow.");
        isHordeSpawning = false;
    }

    void CheckBossEvent()
    {
        if (!enableBossEvent) return;
        if (isBossSpawned) return;
        if (gameTime >= bossSpawnTime)
        {
            SpawnBoss();
        }
    }

    void SpawnBoss()
    {
        isBossSpawned = true;
        Debug.Log("BOSS NADCHODZI! " + bossName + " sie pojawia!");

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;

        if (bossPrefab != null)
        {
            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
            boss.name = bossName + "_Boss";

            enemyHealth bossHealth = boss.GetComponent<enemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.currentHealth = 500f;
                bossHealth.maxHealth = 500f;
                bossHealth.levelSystem = this;
            }

            boss.transform.localScale = Vector3.one * 1.5f;
        }
        else
        {
            Debug.LogWarning("Brak prefaba bossa! Uzyto losowego przeciwnika.");
            GameObject boss = Instantiate(GetRandomEnemyPrefab(), spawnPos, Quaternion.identity);
            boss.name = bossName + "_Boss";
            boss.transform.localScale = Vector3.one * 2f;

            enemyHealth bossHealth = boss.GetComponent<enemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.currentHealth = 500f;
                bossHealth.maxHealth = 500f;
                bossHealth.levelSystem = this;
            }
        }
    }

    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, secs);
    }

    void LevelUp()
    {
        currentLevel++;
        enemiesToKill += 2;
        enemiesKilled = 0;
        UpdateUI();

        if (playerHealth != null)
            playerHealth.LevelUpHealth();

        Debug.Log("===== LEVEL UP! Poziom " + currentLevel + " =====");
        ShowPerkSelection();
    }

    public void EnemyDied()
    {
        if (!gameStarted) return;
        enemiesAlive--;
        enemiesKilled++;
        currentXP++;

        Debug.Log($"Zabity wróg! {enemiesKilled}/{enemiesToKill} | XP: {currentXP}/{xpRequired}");

        if (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            xpRequired += 10;
            LevelUp();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            isTimerRunning = true;
            gameTime = 0f;
            nextHordeTime = firstHordeTime;

            if (timerText != null && showTimerInUI)
                timerText.text = "00:00";

            Debug.Log("Gra rozpoczeta! Timer startuje...");
            StartCoroutine(SpawnEnemies());
        }
    }

    IEnumerator SpawnEnemies()
    {
        isRespawning = true;
        enemiesAlive = enemiesToKill;
        UpdateUI();

        Vector3 center = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) center = player.transform.position;

        for (int i = 0; i < enemiesToKill; i++)
        {
            Vector3 spawnPos = center + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
                spawnPos = hit.position;

            GameObject enemyToSpawn = GetRandomEnemyPrefab();
            if (enemyToSpawn != null)
            {
                GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                enemy.name = "Enemy_Clone_" + (i + 1);
                enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                if (enemyScript != null)
                    enemyScript.levelSystem = this;
            }

            yield return new WaitForSeconds(0.3f);
        }

        isRespawning = false;
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = "Level: " + currentLevel;
        if (enemiesLeftText != null)
            enemiesLeftText.text = "Enemies: " + enemiesAlive;
    }
}