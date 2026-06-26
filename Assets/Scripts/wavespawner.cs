using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour
{
    [Header("Fale")]
    public int currentWave = 1;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 5f;
    public float timeBetweenSpawns = 1f;
    public int maxWaves = 10;

    [Header("Prefaby")]
    public GameObject polnocnicaPrefab;
    public GameObject strzygaPrefab;
    public GameObject upiorPrefab;
    public GameObject bazyliszekPrefab;
    public GameObject leszyPrefab;

    [Header("Szansę (0-100)")]
    [Range(0, 100)] public float polnocnicaChance = 40f;
    [Range(0, 100)] public float strzygaChance = 25f;
    [Range(0, 100)] public float upiorChance = 20f;
    [Range(0, 100)] public float bazyliszekChance = 10f;

    [Header("Boss")]
    public float bossSpawnInterval = 240f;
    [Range(0, 100)] public float bossSpawnChance = 60f;

    [Header("Spawn wokół gracza")]
    public float spawnRadiusMin = 8f;
    public float spawnRadiusMax = 20f;
    public LayerMask groundLayer = ~0;

    [Header("Referencje")]
    public GameManager gameManager;

    private Transform player;
    private List<GameObject> enemies = new List<GameObject>();
    private bool isSpawning = false;
    private int enemiesKilled = 0;
    private int enemiesSpawned = 0;
    private bool isWaveComplete = false;
    private float bossTimer = 0f;
    private bool bossSpawnedThisWave = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindFirstObjectByType<GameManager>();
        FindPlayer();
        bossTimer = bossSpawnInterval;
        StartCoroutine(StartWaves());
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

        if (!isSpawning && enemies.Count == 0 && enemiesKilled >= enemiesPerWave && !isWaveComplete)
        {
            isWaveComplete = true;
            StartCoroutine(NextWave());
        }
    }

    IEnumerator StartWaves()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        isSpawning = true;
        isWaveComplete = false;
        enemiesSpawned = 0;
        enemiesKilled = 0;
        bossSpawnedThisWave = false;

        int count = enemiesPerWave + (currentWave - 1) * 2;
        count = Mathf.Min(count, 30);

        Debug.Log($"🌊 Fala {currentWave}: {count} wrogów");

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = GetRandomEnemy();
            if (prefab != null)
            {
                Vector3 pos = GetSpawnPosition();
                if (pos != Vector3.zero)
                {
                    GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);
                    enemies.Add(enemy);
                    enemiesSpawned++;
                }
            }
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        isSpawning = false;
        Debug.Log($"✅ Fala {currentWave} zakończona!");
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
        // Boss
        if (bossTimer >= bossSpawnInterval && !bossSpawnedThisWave && currentWave > 3 && leszyPrefab != null)
        {
            if (Random.Range(0f, 100f) < bossSpawnChance)
            {
                bossSpawnedThisWave = true;
                bossTimer = 0f;
                Debug.Log($"👑 BOSS w fali {currentWave}!");
                return leszyPrefab;
            }
        }

        float total = polnocnicaChance + strzygaChance + upiorChance + bazyliszekChance;
        if (total <= 0) return polnocnicaPrefab;

        float mult = Mathf.Min(1f + (currentWave - 1) * 0.05f, 2.5f);
        float p = polnocnicaChance / mult;
        float s = strzygaChance * Mathf.Lerp(1f, 1.5f, (currentWave - 1) / 10f);
        float u = upiorChance * Mathf.Lerp(1f, 2f, (currentWave - 1) / 10f);
        float b = bazyliszekChance * Mathf.Lerp(1f, 2.5f, (currentWave - 1) / 10f);

        float newTotal = p + s + u + b;
        float rand = Random.Range(0f, newTotal);
        float cum = 0f;

        cum += p; if (rand < cum && polnocnicaPrefab != null) return polnocnicaPrefab;
        cum += s; if (rand < cum && strzygaPrefab != null) return strzygaPrefab;
        cum += u; if (rand < cum && upiorPrefab != null) return upiorPrefab;
        cum += b; if (rand < cum && bazyliszekPrefab != null) return bazyliszekPrefab;

        return polnocnicaPrefab;
    }

    IEnumerator NextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;

        if (currentWave > maxWaves)
        {
            if (gameManager != null) gameManager.Victory("🏆 WSZYSTKIE FALE UKOŃCZONE!");
            yield break;
        }

        StartCoroutine(SpawnWave());
    }

    public void EnemyDied()
    {
        enemiesKilled++;
    }

    public int GetCurrentWave() => currentWave;
    public int GetEnemyCount() => enemies.Count;
    public int GetMaxWaves() => maxWaves;
    public bool IsWaveComplete() => currentWave > maxWaves && enemies.Count == 0 && !isSpawning;
    public bool IsWaveActive() => isSpawning || enemies.Count > 0;

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

    public void SkipWave()
    {
        if (isSpawning) StopAllCoroutines();
        ClearAllEnemies();
        isWaveComplete = true;
        StartCoroutine(NextWave());
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject e in enemies)
            if (e != null) Destroy(e);
        enemies.Clear();
        enemiesKilled = 0;
        enemiesSpawned = 0;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(player.position, spawnRadiusMin);
        Gizmos.DrawWireSphere(player.position, spawnRadiusMax);
    }
}