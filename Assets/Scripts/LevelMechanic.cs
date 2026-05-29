using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class LevelSystem : MonoBehaviour
{
    [Header("═══════════════ LEVEL SETTINGS ═══════════════")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;
    public int currentXP = 0;
    public int xpRequired = 10;

    [Header("═══════════════ ENEMY SETTINGS ═══════════════")]
    public GameObject enemyPrefab;

    [Header("═══════════════ UI REFERENCES ═══════════════")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject perkSelectionPanel;
    public TextMeshProUGUI perkText1;
    public TextMeshProUGUI perkText2;
    public TextMeshProUGUI perkText3;
    public TextMeshProUGUI timerText;

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

    [Header("═══════════════ TIME EVENTS ═══════════════")]
    public List<TimeEvent> timeEvents = new List<TimeEvent>();

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
        if (enemyPrefab == null)
        {
            Debug.LogError("Przeciagnij prefab wroga do Enemy Prefab!");
            return;
        }

        CreatePerksList();
        UpdateUI();
        HidePerkPanel();

        // Inicjalizacja timera UI
        if (timerText != null && showTimerInUI)
        {
            timerText.text = "00:00";
        }

        if (portalObject != null) portalObject.SetActive(false);

        nextHordeTime = firstHordeTime;

        Debug.Log("Czekam na wybor postaci...");
    }

    void CreatePerksList()
    {
        availablePerks.Clear();

        availablePerks.Add(new Perk("Obrazenia +10", "+10 obrazen", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));
        availablePerks.Add(new Perk("Zasieg +0.2m", "Wiekszy zasieg ataku", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
        }));
        availablePerks.Add(new Perk("Szybki atak", "Szybsze ataki", () => {
            if (mountainManAbilities != null) mountainManAbilities.attackRate = Mathf.Max(0.5f, mountainManAbilities.attackRate - 0.1f);
        }));
        availablePerks.Add(new Perk("Zdrowie +20", "+20 maksymalnego zdrowia", () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(20);
        }));
        availablePerks.Add(new Perk("Szybki cooldown", "-1s cooldown zdolnosci", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));
        availablePerks.Add(new Perk("Mocniejsze obrazenia", "+15 obrazen zdolnosci", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialDamage();
        }));
    }

    void ShowPerkSelection()
    {
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
            perkText1.text = currentPerks[0].name + "\n" + currentPerks[0].description;
        if (perkText2 != null && currentPerks.Count > 1)
            perkText2.text = currentPerks[1].name + "\n" + currentPerks[1].description;
        if (perkText3 != null && currentPerks.Count > 2)
            perkText3.text = currentPerks[2].name + "\n" + currentPerks[2].description;

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(true);

        Debug.Log("=== WYBIERZ PERK ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log((i + 1) + ": " + currentPerks[i].name);
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
        Debug.Log("Wybrano perk: " + currentPerks[index].name);

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

        // ========== SYSTEM TIMERA ==========
        if (isTimerRunning && !isPerkSelectionActive)
        {
            gameTime += Time.deltaTime;
            UpdateTimerUI();
            CheckTimeEvents();
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

    void CheckTimeEvents()
    {
        foreach (TimeEvent timeEvent in timeEvents)
        {
            if (!timeEvent.isTriggered && gameTime >= timeEvent.triggerTime)
            {
                timeEvent.isTriggered = true;
                timeEvent.onTrigger?.Invoke();
                Debug.Log("Event czasowy: " + timeEvent.eventName + " aktywowany!");
            }
        }
    }

    void CheckPortalUnlock()
    {
        if (!isPortalUnlocked && gameTime >= portalUnlockTime)
        {
            UnlockPortal();
        }
    }

    void UnlockPortal()
    {
        isPortalUnlocked = true;
        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log("Portal odblokowany po " + FormatTime(gameTime) + "!");
        }

        if (portalTrigger != null)
        {
            portalTrigger.SetLevelSystem(this);
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
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.name = "Horde_Enemy_" + (i + 1);

                enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                if (enemyScript != null)
                    enemyScript.levelSystem = this;

                spawnedCount++;
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
                bossHealth.health = 500f;
                bossHealth.levelSystem = this;
            }

            boss.transform.localScale = Vector3.one * 1.5f;
        }
        else
        {
            Debug.LogWarning("Brak prefaba bossa! Uzyto zwyklego wroga.");
            GameObject boss = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            boss.name = bossName + "_Boss";
            boss.transform.localScale = Vector3.one * 2f;

            enemyHealth bossHealth = boss.GetComponent<enemyHealth>();
            if (bossHealth != null)
            {
                bossHealth.health = 500f;
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

        if (playerHealth != null) playerHealth.LevelUpHealth();

        ShowPerkSelection();
    }

    public void EnemyDied()
    {
        if (!gameStarted) return;
        enemiesAlive--;
        enemiesKilled++;
        currentXP++;

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

            // Ręczne ustawienie timera na starcie
            if (timerText != null && showTimerInUI)
            {
                timerText.text = "00:00";
                Debug.Log("Timer UI zainicjalizowany");
            }

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
            {
                spawnPos = hit.position;
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Enemy_Clone_" + (i + 1);

            enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
            if (enemyScript != null)
                enemyScript.levelSystem = this;

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