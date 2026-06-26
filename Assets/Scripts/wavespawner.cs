using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA FAL ═══════════════")]
    public int currentWave = 1;
    public int baseEnemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 0.8f;
    public int maxWaves = 999;

    [Header("═══════════════ SKALOWANIE TRUDNOŚCI ═══════════════")]
    public int enemiesPerLevel = 1;
    public float spawnSpeedPerLevel = 0.02f;
    public float waveMultiplier = 1.5f;
    public int maxEnemiesPerWave = 50;

    [Header("═══════════════ PREFABY ═══════════════")]
    public GameObject polnocnicaPrefab;
    public GameObject strzygaPrefab;
    public GameObject upiorPrefab;
    public GameObject bazyliszekPrefab;
    public GameObject leszyPrefab;

    [Header("═══════════════ SZANSĘ (0-100) ═══════════════")]
    [Range(0, 100)] public float polnocnicaChance = 40f;
    [Range(0, 100)] public float strzygaChance = 25f;
    [Range(0, 100)] public float upiorChance = 20f;
    [Range(0, 100)] public float bazyliszekChance = 10f;

    [Header("═══════════════ BOSS ═══════════════")]
    public float bossSpawnInterval = 240f;
    [Range(0, 100)] public float bossSpawnChance = 60f;
    public int bossMinWave = 5;

    [Header("═══════════════ SPAWN WOKÓŁ GRACZA ═══════════════")]
    public float spawnRadiusMin = 8f;
    public float spawnRadiusMax = 20f;
    public LayerMask groundLayer = ~0;

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public LevelSystem levelSystem;

    // ===== ZMIENNE PRYWATNE =====
    private Transform player;
    private List<GameObject> enemies = new List<GameObject>();
    private bool isSpawning = false;
    private int enemiesKilled = 0;
    private int enemiesSpawned = 0;
    private int enemiesThisWave = 0;
    private bool isWaveComplete = false;
    private float bossTimer = 0f;
    private bool bossSpawnedThisWave = false;
    private Camera mainCamera;
    private Coroutine spawnCoroutine;

    void Start()
    {
        mainCamera = Camera.main;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        FindPlayer();
        bossTimer = bossSpawnInterval;

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
        if (player == null) FindPlayer();

        enemies.RemoveAll(e => e == null);

        if (isSpawning || enemies.Count > 0)
            bossTimer += Time.deltaTime;

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
        bossSpawnedThisWave = false;

        enemiesThisWave = CalculateEnemiesForWave();

        Debug.Log($"🌊 FALA {currentWave} START! ({enemiesThisWave} wrogów)");
        AudioManager.Instance?.PlayWaveStart();

        float spawnDelay = Mathf.Max(0.2f, timeBetweenSpawns - (levelSystem != null ? levelSystem.currentLevel * spawnSpeedPerLevel : 0));
        spawnDelay = Mathf.Max(0.2f, spawnDelay);

        for (int i = 0; i < enemiesThisWave; i++)
        {
            if (player == null) break;

            GameObject prefab = GetRandomEnemy();
            if (prefab != null)
            {
                Vector3 pos = GetSpawnPosition();
                if (pos != Vector3.zero)
                {
                    GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
                    enemies.Add(enemy);
                    enemiesSpawned++;
                    AudioManager.Instance?.OnEnemySpawned();
                }
            }

            float dynamicDelay = spawnDelay * (1f + (i / (float)enemiesThisWave) * 0.3f);
            yield return new WaitForSeconds(dynamicDelay);
        }

        isSpawning = false;
        Debug.Log($"✅ FALA {currentWave} zakończona! Spawnowano: {enemiesSpawned} wrogów");
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

    IEnumerator NextWave()
    {
        Debug.Log($"⏳ Czekam {timeBetweenWaves}s na następną falę...");
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        Debug.Log($"🌊 ROZPOCZYNAM FALĘ {currentWave}!");

        if (levelSystem != null) levelSystem.UpdateUI();

        StartCoroutine(SpawnWave());
    }

    Vector3 GetSpawnPosition()
    {
        if (player == null) return Vector3.zero;

        for (int i = 0; i < 30; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);

            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out hit, 100f, groundLayer))
                pos.y = hit.point.y;
            else
                pos.y = 0;

            if (!IsVisibleByCamera(pos))
                return pos;
        }

        return player.position - player.forward * spawnRadiusMin;
    }

    bool IsVisibleByCamera(Vector3 pos)
    {
        if (mainCamera == null) return false;
        Vector3 vp = mainCamera.WorldToViewportPoint(pos);
        return vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1 && vp.z > 0;
    }

    GameObject GetRandomEnemy()
    {
        if (bossTimer >= bossSpawnInterval && !bossSpawnedThisWave && currentWave >= bossMinWave && leszyPrefab != null)
        {
            if (Random.Range(0f, 100f) < bossSpawnChance)
            {
                bossSpawnedThisWave = true;
                bossTimer = 0f;
                AudioManager.Instance?.OnBossSpawned();
                Debug.Log($"👑 BOSS w fali {currentWave}!");
                return leszyPrefab;
            }
        }

        int playerLevel = levelSystem != null ? levelSystem.currentLevel : 1;
        float difficultyMultiplier = 1f + (playerLevel - 1) * 0.05f;
        difficultyMultiplier = Mathf.Min(difficultyMultiplier, 3f);

        float total = polnocnicaChance + strzygaChance + upiorChance + bazyliszekChance;
        if (total <= 0) return polnocnicaPrefab;

        float p = polnocnicaChance / difficultyMultiplier;
        float s = strzygaChance * Mathf.Lerp(1f, 1.5f, (currentWave - 1) / 10f);
        float u = upiorChance * Mathf.Lerp(1f, 2f, (currentWave - 1) / 10f);
        float b = bazyliszekChance * Mathf.Lerp(1f, 2.5f, (currentWave - 1) / 10f);

        s *= (1f + (playerLevel - 1) * 0.02f);
        u *= (1f + (playerLevel - 1) * 0.03f);
        b *= (1f + (playerLevel - 1) * 0.04f);

        float newTotal = p + s + u + b;
        float rand = Random.Range(0f, newTotal);
        float cum = 0f;

        cum += p; if (rand < cum && polnocnicaPrefab != null) return polnocnicaPrefab;
        cum += s; if (rand < cum && strzygaPrefab != null) return strzygaPrefab;
        cum += u; if (rand < cum && upiorPrefab != null) return upiorPrefab;
        cum += b; if (rand < cum && bazyliszekPrefab != null) return bazyliszekPrefab;

        return polnocnicaPrefab;
    }

    // ============================================
    // METODY PUBLICZNE - POPRAWIONE NAZWY
    // ============================================

    public void EnemyDied()
    {
        enemiesKilled++;
        AudioManager.Instance?.OnEnemyDied();

        if (!isSpawning && enemies.Count == 0 && enemiesKilled >= enemiesThisWave && !isWaveComplete)
        {
            isWaveComplete = true;
            AudioManager.Instance?.PlayWaveComplete();
            StartCoroutine(NextWave());
        }
    }

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
        if (isSpawning && spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        ClearAllEnemies();
        isWaveComplete = true;
        StartCoroutine(NextWave());
    }

    // ============================================
    // GETTERY - DLA INNYCH SKRYPTÓW
    // ============================================

    public int GetCurrentWave() => currentWave;
    public int GetEnemyCount() => enemies.Count;
    public bool IsWaveActive() => isSpawning || enemies.Count > 0;
    public bool IsWaveComplete() => isWaveComplete;
    public int GetEnemiesKilled() => enemiesKilled;
    public int GetEnemiesSpawned() => enemiesSpawned;
    public int GetEnemiesThisWave() => enemiesThisWave;

    // ============================================
    // METODY TESTOWE
    // ============================================

    public void TestSpawnBoss()
    {
        if (leszyPrefab != null)
        {
            Vector3 pos = GetSpawnPosition();
            if (pos != Vector3.zero)
            {
                enemies.Add(Instantiate(leszyPrefab, pos, Quaternion.identity));
                Debug.Log("👑 Boss spawned!");
            }
        }
    }

    public void TestSpawnBazyliszek()
    {
        if (bazyliszekPrefab != null)
        {
            Vector3 pos = GetSpawnPosition();
            if (pos != Vector3.zero)
            {
                enemies.Add(Instantiate(bazyliszekPrefab, pos, Quaternion.identity));
                Debug.Log("🐉 Bazyliszek spawned!");
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