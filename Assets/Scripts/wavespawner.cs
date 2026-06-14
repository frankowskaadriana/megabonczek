using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("═══════════════ LISTA PRZECIWNIKÓW (PODEPNIJ TUTAJ!) ═══════════════")]
    public List<GameObject> enemyTemplates;

    [Header("═══════════════ USTAWIENIA SPAWNU ═══════════════")]
    public float baseSpawnDelay = 0.8f;
    public float minSpawnDelay = 0.2f;
    public int maxEnemiesOnScreen = 30;

    [Header("═══════════════ PROMIEŃ SPAWNU ═══════════════")]
    public float minSpawnDistance = 6f;
    public float maxSpawnDistance = 12f;

    [Header("═══════════════ SKALOWANIE TRUDNOŚCI ═══════════════")]
    public float difficultyIncreasePerWave = 0.05f;
    public int maxEnemiesIncreasePerWave = 2;

    [Header("═══════════════ BOSS ═══════════════")]
    public GameObject bossTemplate;
    public int bossWave = 5;
    public KeyCode spawnBossKey = KeyCode.B;

    [Header("═══════════════ KONTENER NA WROGÓW ═══════════════")]
    public Transform enemiesContainer;

    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float spawnTimer = 0f;
    private float currentSpawnDelay;
    public int currentMaxEnemies;
    public int currentWave = 0;
    private int enemiesSpawnedInWave = 0;
    private int enemiesToSpawnThisWave = 0;
    private bool bossSpawnedThisWave = false;
    private LevelSystem levelSystem;

    void Start()
    {
        Debug.Log("=== PŁYNNY WAVESPAWNER START ===");

        levelSystem = FindFirstObjectByType<LevelSystem>();

        if (enemyTemplates == null || enemyTemplates.Count == 0)
        {
            Debug.LogError("❌ BRAK PRZECIWNIKÓW! Przeciągnij przeciwników z hierarchii!");
            return;
        }

        foreach (var enemy in enemyTemplates)
        {
            if (enemy != null)
                Debug.Log($"✅ Podpięto: {enemy.name}");
        }

        if (bossTemplate != null)
            Debug.Log($"✅ Boss podpięty: {bossTemplate.name}");

        currentWave = 0;
        currentSpawnDelay = baseSpawnDelay;
        currentMaxEnemies = maxEnemiesOnScreen / 2;
        enemiesToSpawnThisWave = 20;

        Debug.Log($"🌊 Rozpoczynanie gry!");
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnBossKey))
            TestSpawnBoss();

        if (player == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
                activeEnemies.RemoveAt(i);
        }

        UpdateDifficulty();

        if (activeEnemies.Count < currentMaxEnemies && enemyTemplates.Count > 0)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= currentSpawnDelay)
            {
                spawnTimer = 0f;

                if (!bossSpawnedThisWave && currentWave > 0 && currentWave % bossWave == 0 && bossTemplate != null)
                {
                    SpawnBoss();
                    bossSpawnedThisWave = true;
                }
                else
                {
                    SpawnEnemy();
                    enemiesSpawnedInWave++;
                }

                if (enemiesSpawnedInWave >= enemiesToSpawnThisWave)
                    NextWave();
            }
        }
    }

    void UpdateDifficulty()
    {
        float difficulty = Mathf.Pow(1 + difficultyIncreasePerWave, currentWave);
        currentSpawnDelay = Mathf.Max(minSpawnDelay, baseSpawnDelay / difficulty);
        currentMaxEnemies = Mathf.Min(maxEnemiesOnScreen, (int)((maxEnemiesOnScreen / 2) + currentWave * maxEnemiesIncreasePerWave));
    }

    void NextWave()
    {
        currentWave++;
        enemiesSpawnedInWave = 0;
        enemiesToSpawnThisWave = 20 + currentWave * 5;
        bossSpawnedThisWave = false;
        Debug.Log($"🌊 FALA {currentWave}");
    }

    void SpawnBoss()
    {
        if (bossTemplate == null) return;
        if (player == null) return;

        Vector3 spawnPos = GetRandomPosition();
        GameObject boss = Instantiate(bossTemplate, spawnPos, Quaternion.identity);
        boss.name = $"BOSS_Wave{currentWave}";

        if (enemiesContainer != null)
            boss.transform.SetParent(enemiesContainer);

        Leszy leszyScript = boss.GetComponent<Leszy>();
        if (leszyScript != null)
        {
            leszyScript.levelSystem = levelSystem;
            leszyScript.currentHealth = leszyScript.maxHealth;
        }

        activeEnemies.Add(boss);
        Debug.Log($"👑 BOSS SPAWNOWANY!");
    }

    public void TestSpawnBoss()
    {
        if (bossTemplate == null)
        {
            Debug.LogError("❌ BOSS NIE JEST PRZYPISANY!");
            return;
        }
        if (player == null) return;

        Vector3 spawnPos = player.position + player.forward * 5f;
        GameObject boss = Instantiate(bossTemplate, spawnPos, Quaternion.identity);
        boss.name = "TEST_BOSS";

        if (enemiesContainer != null)
            boss.transform.SetParent(enemiesContainer);

        Leszy leszyScript = boss.GetComponent<Leszy>();
        if (leszyScript != null)
        {
            leszyScript.levelSystem = levelSystem;
            leszyScript.currentHealth = leszyScript.maxHealth;
        }

        activeEnemies.Add(boss);
        Debug.Log($"🧪 TESTOWY BOSS SPAWNOWANY!");
    }

    void SpawnEnemy()
    {
        List<GameObject> validTemplates = new List<GameObject>();
        foreach (var t in enemyTemplates)
            if (t != null) validTemplates.Add(t);

        if (validTemplates.Count == 0) return;

        int randomIndex = 0;
        if (currentWave >= 10 && validTemplates.Count > 2 && Random.Range(0, 100) < 30)
            randomIndex = 2;
        else if (currentWave >= 5 && validTemplates.Count > 1 && Random.Range(0, 100) < 20)
            randomIndex = 1;

        GameObject selectedTemplate = validTemplates[randomIndex];
        Vector3 spawnPos = GetRandomPosition();

        GameObject enemy = Instantiate(selectedTemplate, spawnPos, Quaternion.identity);
        enemy.name = $"{selectedTemplate.name}_{currentWave}";

        if (enemiesContainer != null)
            enemy.transform.SetParent(enemiesContainer);

        enemyHealth healthScript = enemy.GetComponent<enemyHealth>();
        if (healthScript != null)
        {
            healthScript.levelSystem = levelSystem;
            float difficulty = 1f + (currentWave * 0.05f);
            healthScript.maxHealth = Mathf.RoundToInt(healthScript.maxHealth * difficulty);
            healthScript.currentHealth = healthScript.maxHealth;
        }

        activeEnemies.Add(enemy);
    }

    Vector3 GetRandomPosition()
    {
        if (player == null) return Vector3.zero;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        float x = player.position.x + Mathf.Cos(angle) * distance;
        float z = player.position.z + Mathf.Sin(angle) * distance;
        return new Vector3(x, 0, z);
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
            if (enemy != null) Destroy(enemy);
        activeEnemies.Clear();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 25), $"FALA: {currentWave}");
        GUI.Label(new Rect(10, 35, 300, 25), $"Wrogowie: {activeEnemies.Count}/{currentMaxEnemies}");
        GUI.Label(new Rect(10, 60, 300, 25), $"Szybkość: co {currentSpawnDelay:F2}s");
        if (GUI.Button(new Rect(10, 90, 200, 30), "TEST BOSSA (B)"))
            TestSpawnBoss();
    }
}