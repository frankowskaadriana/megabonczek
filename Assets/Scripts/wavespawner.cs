using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyTier
    {
        [Header("═══════════════ USTAWIENIA TIERU ═══════════════")]
        public string tierName = "Nowy Tier";
        [Range(1, 10)] public int tierLevel = 1;
        [Range(0, 100)] public float baseChance = 50f;
        [Range(0, 10)] public float chanceIncreasePerWave = 2f;
        public int minWaveToSpawn = 1;

        [Header("═══════════════ PRZECIWNICY W TIERZE ═══════════════")]
        public List<EnemyEntry> enemies = new List<EnemyEntry>();
    }

    [System.Serializable]
    public class EnemyEntry
    {
        [Header("═══════════════ PRZECIWNIK ═══════════════")]
        public GameObject prefab;
        public string enemyName = "Przeciwnik";

        [Header("═══════════════ STATYSTYKI ═══════════════")]
        [Range(0, 100)] public float weight = 10f;
        public int expReward = 10;
        public bool isBoss = false;
        public int minWaveToSpawn = 1;
        [Range(0.5f, 5f)] public float difficultyScale = 1f;
    }

    [Header("═══════════════ USTAWIENIA FAL ═══════════════")]
    public int currentWave = 1;
    public int baseEnemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 0.8f;
    public int maxWaves = 999;

    [Header("═══════════════ SKALOWANIE TRUDNOŚCI ═══════════════")]
    public int enemiesPerLevel = 1;
    public float waveMultiplier = 1.5f;
    public int maxEnemiesPerWave = 50;

    [Header("═══════════════ TIERY PRZECIWNIKÓW ═══════════════")]
    public List<EnemyTier> enemyTiers = new List<EnemyTier>();

    [Header("═══════════════ SPAWN WOKÓŁ GRACZA ═══════════════")]
    public float spawnRadiusMin = 8f;
    public float spawnRadiusMax = 20f;
    public float spawnHeight = 1f; // STAŁA WYSOKOŚĆ
    public LayerMask groundLayer = ~0;

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public LevelSystem levelSystem;

    private Transform player;
    private List<GameObject> enemies = new List<GameObject>();
    private bool isSpawning = false;
    private int enemiesKilled = 0;
    private int enemiesSpawned = 0;
    private int enemiesThisWave = 0;
    private bool isWaveComplete = false;
    private Camera mainCamera;
    private float gameTime = 0f;

    void Start()
    {
        mainCamera = Camera.main;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        FindPlayer();
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(SpawnWave());
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        gameTime += Time.deltaTime;

        if (player == null) FindPlayer();
        enemies.RemoveAll(e => e == null);

        if (!isSpawning && enemies.Count == 0 && !isWaveComplete && enemiesKilled >= enemiesThisWave)
        {
            isWaveComplete = true;
            AudioManager.Instance?.PlayWaveComplete();
            StartCoroutine(NextWave());
        }
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;
        isWaveComplete = false;
        enemiesSpawned = 0;
        enemiesKilled = 0;

        enemiesThisWave = CalculateEnemiesForWave();

        Debug.Log($"🌊 FALA {currentWave} START! ({enemiesThisWave} wrogów)");
        AudioManager.Instance?.PlayWaveStart();

        for (int i = 0; i < enemiesThisWave; i++)
        {
            if (player == null) break;

            GameObject prefab = GetRandomEnemy();
            if (prefab != null)
            {
                Vector3 pos = GetSpawnPosition();
                if (pos != Vector3.zero)
                {
                    // === SPAWN PRZECIWNIKA ===
                    GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

                    // === WYMUŚ Y = SPAWNHEIGHT ===
                    Vector3 enemyPos = enemy.transform.position;
                    enemyPos.y = spawnHeight;
                    enemy.transform.position = enemyPos;

                    enemies.Add(enemy);
                    enemiesSpawned++;
                    AudioManager.Instance?.OnEnemySpawned();

                    Debug.Log($"📍 Spawn: {enemy.name} na pozycji {enemyPos} (Y = {spawnHeight})");
                }
            }
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        isSpawning = false;
        Debug.Log($"✅ FALA {currentWave} zakończona!");
    }

    int CalculateEnemiesForWave()
    {
        int baseCount = baseEnemiesPerWave;
        int levelBonus = levelSystem != null ? levelSystem.currentLevel * enemiesPerLevel : 0;
        int waveBonus = Mathf.FloorToInt((currentWave - 1) * waveMultiplier);
        int total = baseCount + levelBonus + waveBonus;
        total = Mathf.Min(total, maxEnemiesPerWave);
        total = Mathf.Max(1, total);
        return total;
    }

    GameObject GetRandomEnemy()
    {
        // === SPRAWDŹ BOSSA W TIERACH ===
        foreach (EnemyTier tier in enemyTiers)
        {
            foreach (EnemyEntry entry in tier.enemies)
            {
                if (entry.isBoss && entry.prefab != null && currentWave >= entry.minWaveToSpawn)
                {
                    float bossChance = tier.baseChance / 100f;
                    if (Random.Range(0f, 1f) < bossChance * 0.3f)
                    {
                        Debug.Log($"👑 BOSS {entry.enemyName} w fali {currentWave}!");
                        return entry.prefab;
                    }
                }
            }
        }

        // === WYBÓR PRZECIWNIKA Z TIERÓW ===
        List<EnemyEntry> availableEnemies = new List<EnemyEntry>();
        float totalWeight = 0f;

        foreach (EnemyTier tier in enemyTiers)
        {
            if (currentWave < tier.minWaveToSpawn) continue;

            float tierChance = tier.baseChance + (currentWave - tier.minWaveToSpawn) * tier.chanceIncreasePerWave;
            tierChance = Mathf.Min(tierChance, 100f);

            foreach (EnemyEntry entry in tier.enemies)
            {
                if (entry.prefab == null) continue;
                if (currentWave < entry.minWaveToSpawn) continue;
                if (entry.isBoss) continue;

                float timeScale = 1f + (gameTime / 60f) * 0.1f;
                timeScale = Mathf.Min(timeScale, 3f);

                float weight = entry.weight * (tierChance / 100f) * entry.difficultyScale * timeScale;

                availableEnemies.Add(entry);
                totalWeight += weight;
            }
        }

        if (availableEnemies.Count == 0 || totalWeight <= 0)
        {
            foreach (EnemyTier tier in enemyTiers)
            {
                foreach (EnemyEntry entry in tier.enemies)
                {
                    if (entry.prefab != null && currentWave >= entry.minWaveToSpawn && !entry.isBoss)
                        return entry.prefab;
                }
            }
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (EnemyEntry entry in availableEnemies)
        {
            float timeScale = 1f + (gameTime / 60f) * 0.1f;
            timeScale = Mathf.Min(timeScale, 3f);

            float tierChance = 100f;
            foreach (EnemyTier tier in enemyTiers)
            {
                if (tier.enemies.Contains(entry))
                {
                    tierChance = tier.baseChance + (currentWave - tier.minWaveToSpawn) * tier.chanceIncreasePerWave;
                    tierChance = Mathf.Min(tierChance, 100f);
                    break;
                }
            }

            float weight = entry.weight * (tierChance / 100f) * entry.difficultyScale * timeScale;
            cumulative += weight;

            if (randomValue <= cumulative)
            {
                return entry.prefab;
            }
        }

        return availableEnemies[availableEnemies.Count - 1].prefab;
    }

    Vector3 GetSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        for (int i = 0; i < 30; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);

            // Ustaw Y na spawnHeight (potem i tak wymusimy)
            pos.y = spawnHeight;

            if (!IsVisibleByCamera(pos))
            {
                return pos;
            }
        }

        Vector3 fallbackPos = player.position - player.forward * spawnRadiusMin;
        fallbackPos.y = spawnHeight;
        return fallbackPos;
    }

    bool IsVisibleByCamera(Vector3 pos)
    {
        if (mainCamera == null) return false;
        Vector3 vp = mainCamera.WorldToViewportPoint(pos);
        return vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1 && vp.z > 0;
    }

    IEnumerator NextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;

        if (levelSystem != null) levelSystem.UpdateUI();

        StartCoroutine(SpawnWave());
    }

    public void EnemyDied()
    {
        enemiesKilled++;
    }

    public int GetCurrentWave() => currentWave;
    public int GetEnemyCount() => enemies.Count;
    public bool IsSpawning() => isSpawning;
    public bool IsWaveComplete() => isWaveComplete;

    public void StartNextWave()
    {
        if (!isSpawning && enemies.Count == 0 && isWaveComplete)
        {
            StartCoroutine(NextWave());
        }
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject e in enemies)
            if (e != null) Destroy(e);
        enemies.Clear();
        enemiesKilled = 0;
        enemiesSpawned = 0;
        isWaveComplete = true;
        Debug.Log("🗑️ Wszyscy wrogowie usunięci!");
    }

    public void SkipWave()
    {
        if (isSpawning) StopAllCoroutines();
        ClearAllEnemies();
        isWaveComplete = true;
        StartCoroutine(NextWave());
    }

    public void TestSpawnBoss()
    {
        foreach (EnemyTier tier in enemyTiers)
        {
            foreach (EnemyEntry entry in tier.enemies)
            {
                if (entry.isBoss && entry.prefab != null)
                {
                    Vector3 pos = GetSpawnPosition();
                    if (pos != Vector3.zero)
                    {
                        GameObject boss = Instantiate(entry.prefab, pos, Quaternion.identity);
                        Vector3 bossPos = boss.transform.position;
                        bossPos.y = spawnHeight;
                        boss.transform.position = bossPos;
                        enemies.Add(boss);
                        Debug.Log($"👑 Boss {entry.enemyName} spawned!");
                        return;
                    }
                }
            }
        }
        Debug.LogWarning("❌ Nie znaleziono bossa!");
    }

    public void TestSpawnTier(int tierIndex)
    {
        if (tierIndex < 0 || tierIndex >= enemyTiers.Count) return;

        EnemyTier tier = enemyTiers[tierIndex];
        if (tier.enemies.Count > 0)
        {
            EnemyEntry entry = tier.enemies[Random.Range(0, tier.enemies.Count)];
            Vector3 pos = GetSpawnPosition();
            if (pos != Vector3.zero)
            {
                GameObject enemy = Instantiate(entry.prefab, pos, Quaternion.identity);
                Vector3 enemyPos = enemy.transform.position;
                enemyPos.y = spawnHeight;
                enemy.transform.position = enemyPos;
                enemies.Add(enemy);
                Debug.Log($"🎯 {entry.enemyName} z tieru {tier.tierName} spawned!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(player.position, spawnRadiusMin);
        Gizmos.DrawWireSphere(player.position, spawnRadiusMax);
    }
}