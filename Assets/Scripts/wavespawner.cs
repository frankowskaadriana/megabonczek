using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA FAL ═══════════════")]
    public int currentWave = 0;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 0.5f;

    [Header("═══════════════ SPAWN USTAWIENIA ═══════════════")]
    public List<GameObject> enemyPrefabs;
    public float spawnRadius = 12f;
    public float minSpawnDistance = 8f;

    [Header("═══════════════ SKALOWANIE TRUDNOŚCI ═══════════════")]
    public int enemiesIncreasePerWave = 2;
    public float spawnDelayDecreasePerWave = 0.05f;
    public float minSpawnDelay = 0.2f;

    [Header("═══════════════ USTAWIENIA BOSSA ═══════════════")]
    public bool enableBoss = true;
    public int bossWave = 5;           // Która fala pojawia się boss
    public GameObject bossPrefab;      // Prefab bossa (Leszy)
    public float bossHealthMultiplier = 3f;
    public float bossDamageMultiplier = 2f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public Camera mainCamera;

    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private bool waveInProgress = false;
    private bool isBossWave = false;
    private float waveTimer = 0f;
    private float cameraMargin = 2f;

    void Start()
    {
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        StartNewWave();
    }

    void Update()
    {
        if (waveInProgress)
        {
            // Usuń martwych przeciwników z listy
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                    activeEnemies.RemoveAt(i);
            }

            // Sprawdź czy fala się skończyła
            if (activeEnemies.Count == 0 && !isSpawning)
            {
                waveInProgress = false;
                waveTimer = timeBetweenWaves;

                if (isBossWave)
                {
                    Debug.Log($"Boss pokonany! Fala {currentWave} zakończona!");
                    isBossWave = false;
                }
                else
                {
                    Debug.Log($"Fala {currentWave} zakończona! Następna fala za {timeBetweenWaves} sekund");
                }
            }
        }
        else
        {
            if (waveTimer > 0)
            {
                waveTimer -= Time.deltaTime;
                if (waveTimer <= 0)
                {
                    StartNewWave();
                }
            }
        }
    }

    void StartNewWave()
    {
        currentWave++;

        // Sprawdź czy to fala z bossem
        if (enableBoss && currentWave % bossWave == 0)
        {
            isBossWave = true;
            Debug.Log($"!!! FALA {currentWave} - BOSS NADCHODZI !!!");
            StartCoroutine(SpawnBossWave());
        }
        else
        {
            isBossWave = false;
            int enemiesToSpawn = enemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
            float currentSpawnDelay = Mathf.Max(minSpawnDelay, timeBetweenSpawns - (currentWave - 1) * spawnDelayDecreasePerWave);

            Debug.Log($"=== FALA {currentWave} ===");
            Debug.Log($"Przeciwników: {enemiesToSpawn}, Odstęp: {currentSpawnDelay:F1}s");

            StartCoroutine(SpawnWave(enemiesToSpawn, currentSpawnDelay));
        }
    }

    IEnumerator SpawnBossWave()
    {
        isSpawning = true;
        waveInProgress = true;

        // Najpierw spawnuj normalnych wrogów (mniej niż zwykle)
        int normalEnemies = enemiesPerWave / 2;
        float currentSpawnDelay = 0.3f;

        for (int i = 0; i < normalEnemies; i++)
        {
            Vector3 spawnPos = GetSpawnPositionOutsideCamera();
            GameObject enemyToSpawn = GetRandomEnemy();

            if (enemyToSpawn != null)
            {
                GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                enemy.name = $"Enemy_Wave{currentWave}_{i + 1}";

                enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                if (enemyScript != null)
                {
                    if (levelSystem != null)
                        enemyScript.levelSystem = levelSystem;

                    float healthMultiplier = 1f + (currentWave - 1) * 0.1f;
                    enemyScript.maxHealth = Mathf.RoundToInt(enemyScript.maxHealth * healthMultiplier);
                    enemyScript.currentHealth = enemyScript.maxHealth;
                }

                activeEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(currentSpawnDelay);
        }

        // Spawnuj bossa
        if (bossPrefab != null)
        {
            Vector3 bossSpawnPos = GetSpawnPositionOutsideCamera();
            GameObject boss = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);
            boss.name = $"BOSS_Leszy_Wave{currentWave}";

            enemyHealth bossScript = boss.GetComponent<enemyHealth>();
            if (bossScript != null)
            {
                if (levelSystem != null)
                    bossScript.levelSystem = levelSystem;

                // Skaluj bossa
                bossScript.maxHealth = Mathf.RoundToInt(bossScript.maxHealth * bossHealthMultiplier);
                bossScript.currentHealth = bossScript.maxHealth;
                bossScript.damage = Mathf.RoundToInt(bossScript.damage * bossDamageMultiplier);
                bossScript.expReward = 500;

                // Wizualne powiększenie bossa
                boss.transform.localScale = Vector3.one * 2.5f;
            }

            activeEnemies.Add(boss);
            Debug.Log($"!!! BOSS LESZY POJAWIŁ SIĘ !!! HP: {bossScript.maxHealth}");
        }
        else
        {
            Debug.LogWarning("Brak prefaba bossa! Użyto zwykłego przeciwnika.");
            Vector3 bossSpawnPos = GetSpawnPositionOutsideCamera();
            GameObject boss = Instantiate(enemyPrefabs[0], bossSpawnPos, Quaternion.identity);
            boss.name = $"BOSS_Wave{currentWave}";
            boss.transform.localScale = Vector3.one * 2f;
            activeEnemies.Add(boss);
        }

        isSpawning = false;
        Debug.Log($"Fala {currentWave} (BOSS) rozpoczęta!");
    }

    IEnumerator SpawnWave(int enemyCount, float spawnDelay)
    {
        isSpawning = true;
        waveInProgress = true;

        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPos = GetSpawnPositionOutsideCamera();
            GameObject enemyToSpawn = GetRandomEnemy();

            if (enemyToSpawn != null)
            {
                GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);
                enemy.name = $"Enemy_Wave{currentWave}_{i + 1}";

                enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                if (enemyScript != null)
                {
                    if (levelSystem != null)
                        enemyScript.levelSystem = levelSystem;

                    // Skaluj zdrowie z falą
                    float healthMultiplier = 1f + (currentWave - 1) * 0.1f;
                    enemyScript.maxHealth = Mathf.RoundToInt(enemyScript.maxHealth * healthMultiplier);
                    enemyScript.currentHealth = enemyScript.maxHealth;
                    enemyScript.expReward = Mathf.RoundToInt(enemyScript.expReward * healthMultiplier);

                    // Skaluj obrażenia
                    enemyScript.damage = Mathf.RoundToInt(enemyScript.damage * (1f + (currentWave - 1) * 0.05f));
                }

                activeEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
        Debug.Log($"Fala {currentWave} rozpoczęta! {activeEnemies.Count} przeciwników");
    }

    Vector3 GetSpawnPositionOutsideCamera()
    {
        Vector3 center = GetPlayerPosition();
        int maxAttempts = 30;

        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(minSpawnDistance, spawnRadius);
            Vector3 spawnPos = center + new Vector3(Mathf.Cos(angle) * distance, 0, Mathf.Sin(angle) * distance);

            if (mainCamera == null)
                return spawnPos;

            if (!IsPositionInCameraView(spawnPos))
            {
                UnityEngine.AI.NavMeshHit hit;
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    return hit.position;
                }
                return spawnPos;
            }
        }

        Vector3 fallbackPos = center - (mainCamera.transform.forward * spawnRadius);
        fallbackPos.y = 0;
        return fallbackPos;
    }

    bool IsPositionInCameraView(Vector3 position)
    {
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(position);
        return viewportPoint.x >= -cameraMargin && viewportPoint.x <= 1 + cameraMargin &&
               viewportPoint.y >= -cameraMargin && viewportPoint.y <= 1 + cameraMargin &&
               viewportPoint.z > 0;
    }

    Vector3 GetPlayerPosition()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            return player.transform.position;
        return Vector3.zero;
    }

    GameObject GetRandomEnemy()
    {
        if (enemyPrefabs.Count == 0) return null;

        if (currentWave >= 10 && enemyPrefabs.Count > 2)
        {
            if (Random.Range(0, 100) < 30)
                return enemyPrefabs[2];
        }
        if (currentWave >= 5 && enemyPrefabs.Count > 1)
        {
            if (Random.Range(0, 100) < 20)
                return enemyPrefabs[1];
        }

        return enemyPrefabs[0];
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = GetPlayerPosition();
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center, minSpawnDistance);
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center, spawnRadius);
    }
}