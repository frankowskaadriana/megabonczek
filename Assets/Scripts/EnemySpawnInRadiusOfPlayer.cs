using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // Podepnij prefab przeciwnika
    public int maxEnemies = 50; // Maksymalna liczba przeciwników na scenie

    [Header("Spawn Settings")]
    public float spawnRadius = 8f; // Promieñ spawnu od gracza
    public float minSpawnDistance = 5f; // Minimalna odleg³oœæ od gracza
    public float spawnInterval = 1.5f; // Czas miêdzy spawnami
    public int enemiesPerSpawn = 1; // Ile przeciwników naraz

    [Header("Wave Settings")]
    public bool waveMode = false;
    public int enemiesPerWave = 10;
    public float timeBetweenWaves = 5f;

    [Header("Difficulty Scaling")]
    public bool scaleWithTime = true;
    public float maxSpawnInterval = 0.3f; // Minimalny czas miêdzy spawnami (najtrudniej)
    public float scaleTime = 300f; // Czas po którym osi¹ga max trudnoœæ (5 minut)

    [Header("Spawn Area Visualization")]
    public bool showSpawnRadius = true;
    public Color spawnAreaColor = new Color(1, 0, 0, 0.3f);

    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float currentSpawnInterval;
    private float spawnTimer;
    private float gameTimer = 0f;
    private int currentWave = 1;
    private bool waveInProgress = false;

    void Start()
    {
        // ZnajdŸ gracza
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Nie znaleziono gracza z tagiem 'Player'!");
        }

        currentSpawnInterval = spawnInterval;
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (player == null) return;

        // Aktualizuj timer gry dla skalowania trudnoœci
        if (scaleWithTime)
        {
            gameTimer += Time.deltaTime;
            float t = Mathf.Clamp01(gameTimer / scaleTime);
            currentSpawnInterval = Mathf.Lerp(spawnInterval, maxSpawnInterval, t);
        }

        // Odœwie¿ listê aktywnych wrogów (nowa metoda)
        enemyHealth[] enemies = FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);
        activeEnemies.Clear();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                activeEnemies.Add(enemy.gameObject);
        }

        // Spawnowanie w trybie falowym
        if (waveMode)
        {
            if (!waveInProgress && activeEnemies.Count == 0)
            {
                StartCoroutine(StartWave());
            }
        }
        else
        {
            // Normalny spawn
            if (activeEnemies.Count < maxEnemies)
            {
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0f)
                {
                    SpawnEnemies();
                    spawnTimer = currentSpawnInterval;
                }
            }
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < enemiesPerSpawn; i++)
        {
            if (activeEnemies.Count >= maxEnemies) break;

            Vector3 spawnPosition = GetSpawnPosition();
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);

            // Opcjonalnie: ustaw rodzica dla porz¹dku
            newEnemy.transform.parent = transform;
        }
    }

    Vector3 GetSpawnPosition()
    {
        // Losuj k¹t
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Losuj odleg³oœæ miêdzy minSpawnDistance a spawnRadius
        float distance = Random.Range(minSpawnDistance, spawnRadius);

        // Oblicz pozycjê wzglêdem gracza
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        Vector3 spawnPos = player.position + offset;

        // Opcjonalnie: dostosuj wysokoœæ (jeœli gra ma Y jako wysokoœæ)
        spawnPos.y = 0; // Lub player.position.y jeœli chcesz na wysokoœci gracza

        return spawnPos;
    }

    IEnumerator StartWave()
    {
        waveInProgress = true;
        currentWave++;

        int enemiesToSpawn = enemiesPerWave + (currentWave / 2); // Z ka¿d¹ fal¹ wiêcej wrogów

        Debug.Log($"Fala {currentWave} rozpoczyna siê! {enemiesToSpawn} przeciwników");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (activeEnemies.Count >= maxEnemies)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            Vector3 spawnPosition = GetSpawnPosition();
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);
            newEnemy.transform.parent = transform;

            yield return new WaitForSeconds(0.2f); // OpóŸnienie miêdzy spawnami w fali
        }

        waveInProgress = false;
    }

    // Rêczne spawnowanie przeciwnika
    public void SpawnSingleEnemy()
    {
        if (enemyPrefab != null && player != null)
        {
            Vector3 spawnPosition = GetSpawnPosition();
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }
    }

    // Spawnowanie w konkretnej pozycji
    public void SpawnEnemyAtPosition(Vector3 position)
    {
        if (enemyPrefab != null)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }
    }

    // Usuñ wszystkich przeciwników
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }
        activeEnemies.Clear();
    }

    // Zwiêksz trudnoœæ rêcznie
    public void IncreaseDifficulty()
    {
        spawnInterval = Mathf.Max(maxSpawnInterval, spawnInterval * 0.9f);
        enemiesPerSpawn++;
    }

    private void OnDrawGizmosSelected()
    {
        if (showSpawnRadius && player != null)
        {
            // Rysuj zasiêg spawnu
            Gizmos.color = spawnAreaColor;
            Gizmos.DrawWireSphere(player.position, spawnRadius);

            // Rysuj minimaln¹ odleg³oœæ
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, minSpawnDistance);

            // Rysuj przyk³adowe pozycje spawnu
            Gizmos.color = Color.red;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float distance = (minSpawnDistance + spawnRadius) / 2;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
                Gizmos.DrawSphere(player.position + offset, 0.3f);
            }
        }
    }

    // Gettery
    public int GetEnemyCount() => activeEnemies.Count;
    public int GetCurrentWave() => currentWave;
    public float GetCurrentSpawnInterval() => currentSpawnInterval;
}