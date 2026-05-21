using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("═══════════════ ENEMY SETTINGS ═══════════════")]
    public GameObject enemyPrefab;
    public int maxEnemies = 50;

    [Header("═══════════════ SPAWN SETTINGS ═══════════════")]
    public float spawnRadius = 8f;        // Maksymalny promień spawnu od gracza
    public float minSpawnDistance = 5f;   // Minimalna odległość od gracza (NIE SPAWNUJ BLIŻEJ!)
    public float maxSpawnDistance = 12f;  // Maksymalna odległość od gracza (DODANE)
    public float spawnInterval = 1.5f;
    public int enemiesPerSpawn = 1;

    [Header("═══════════════ WAVE SETTINGS ═══════════════")]
    public bool waveMode = false;
    public int enemiesPerWave = 10;
    public float timeBetweenWaves = 5f;

    [Header("═══════════════ DIFFICULTY SCALING ═══════════════")]
    public bool scaleWithTime = true;
    public float maxSpawnInterval = 0.3f;
    public float scaleTime = 300f;

    [Header("═══════════════ SPAWN AREA VISUALIZATION ═══════════════")]
    public bool showSpawnRadius = true;
    public Color spawnAreaColor = new Color(1, 0, 0, 0.3f);
    public Color noSpawnZoneColor = new Color(0, 1, 0, 0.3f);

    private Transform player;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private float currentSpawnInterval;
    private float spawnTimer;
    private float gameTimer = 0f;
    private int currentWave = 1;
    private bool waveInProgress = false;

    void Start()
    {
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

        if (scaleWithTime)
        {
            gameTimer += Time.deltaTime;
            float t = Mathf.Clamp01(gameTimer / scaleTime);
            currentSpawnInterval = Mathf.Lerp(spawnInterval, maxSpawnInterval, t);
        }

        // Odśwież listę aktywnych wrogów
        enemyHealth[] enemies = FindObjectsByType<enemyHealth>(FindObjectsSortMode.None);
        activeEnemies.Clear();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
                activeEnemies.Add(enemy.gameObject);
        }

        if (waveMode)
        {
            if (!waveInProgress && activeEnemies.Count == 0)
            {
                StartCoroutine(StartWave());
            }
        }
        else
        {
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

            // Dodatkowe sprawdzenie czy pozycja nie jest za blisko gracza
            if (IsPositionTooCloseToPlayer(spawnPosition))
            {
                Debug.Log("Pozycja spawnu za blisko gracza! Szukam nowej...");
                spawnPosition = GetAlternativeSpawnPosition();
            }

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);
            newEnemy.transform.parent = transform;
        }
    }

    Vector3 GetSpawnPosition()
    {
        // Losuj kąt (0-360 stopni)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

        // Losuj odległość między minSpawnDistance a maxSpawnDistance
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);

        // Oblicz pozycję względem gracza
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
        Vector3 spawnPos = player.position + offset;

        // Ustaw wysokość na 0 (lub na poziom gracza)
        spawnPos.y = 0;

        return spawnPos;
    }

    // Sprawdza czy pozycja jest za blisko gracza
    bool IsPositionTooCloseToPlayer(Vector3 position)
    {
        float distanceToPlayer = Vector3.Distance(position, player.position);
        return distanceToPlayer < minSpawnDistance;
    }

    // Alternatywna pozycja spawnu - próbuje znaleźć bezpieczne miejsce
    Vector3 GetAlternativeSpawnPosition()
    {
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance + 1f, maxSpawnDistance);
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
            Vector3 spawnPos = player.position + offset;
            spawnPos.y = 0;

            if (!IsPositionTooCloseToPlayer(spawnPos))
            {
                return spawnPos;
            }
        }

        // Jeśli nie znaleziono, zwróć pozycję na maksymalnym dystansie
        Vector3 fallbackPos = player.position + (Vector3.forward * maxSpawnDistance);
        fallbackPos.y = 0;
        Debug.LogWarning("Nie znaleziono bezpiecznej pozycji spawnu! Użyto domyślnej.");
        return fallbackPos;
    }

    IEnumerator StartWave()
    {
        waveInProgress = true;
        currentWave++;

        int enemiesToSpawn = enemiesPerWave + (currentWave / 2);
        Debug.Log($"Fala {currentWave} rozpoczyna się! {enemiesToSpawn} przeciwników");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (activeEnemies.Count >= maxEnemies)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            Vector3 spawnPosition = GetSpawnPosition();

            // Blokada spawnu za blisko gracza w falach
            if (IsPositionTooCloseToPlayer(spawnPosition))
            {
                spawnPosition = GetAlternativeSpawnPosition();
            }

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);
            newEnemy.transform.parent = transform;

            yield return new WaitForSeconds(0.2f);
        }

        waveInProgress = false;
    }

    public void SpawnSingleEnemy()
    {
        if (enemyPrefab != null && player != null)
        {
            Vector3 spawnPosition = GetSpawnPosition();

            if (IsPositionTooCloseToPlayer(spawnPosition))
            {
                spawnPosition = GetAlternativeSpawnPosition();
            }

            GameObject newEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }
    }

    public void SpawnEnemyAtPosition(Vector3 position)
    {
        if (enemyPrefab != null)
        {
            // Sprawdź czy podana pozycja nie jest za blisko gracza
            if (player != null && IsPositionTooCloseToPlayer(position))
            {
                Debug.LogWarning("Próba spawnu wroga za blisko gracza! Spawn anulowany.");
                return;
            }

            GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }
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

    public void IncreaseDifficulty()
    {
        spawnInterval = Mathf.Max(maxSpawnInterval, spawnInterval * 0.9f);
        enemiesPerSpawn++;
        // Zmniejsz minimalną odległość spawnu wraz z trudnością
        minSpawnDistance = Mathf.Max(2f, minSpawnDistance * 0.95f);
    }

    private void OnDrawGizmosSelected()
    {
        if (showSpawnRadius && player != null)
        {
            // Zasięg maksymalny spawnu (czerwony)
            Gizmos.color = spawnAreaColor;
            Gizmos.DrawWireSphere(player.position, maxSpawnDistance);

            // Strefa NO SPAWN (zielona) - tu NIE pojawiają się wrogowie
            Gizmos.color = noSpawnZoneColor;
            Gizmos.DrawWireSphere(player.position, minSpawnDistance);

            // Wypełnij strefę NO SPAWN dla lepszej widoczności
            Gizmos.color = new Color(0, 1, 0, 0.1f);
            Gizmos.DrawSphere(player.position, minSpawnDistance);

            // Przykładowe dopuszczalne pozycje spawnu
            Gizmos.color = Color.yellow;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float distance = (minSpawnDistance + maxSpawnDistance) / 2;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;
                Gizmos.DrawSphere(player.position + offset, 0.3f);
            }
        }
    }

    public int GetEnemyCount() => activeEnemies.Count;
    public int GetCurrentWave() => currentWave;
    public float GetCurrentSpawnInterval() => currentSpawnInterval;
}