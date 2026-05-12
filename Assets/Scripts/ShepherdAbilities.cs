using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShepherdAbilities : MonoBehaviour
{
    [Header("Statystyki Podstawowe")]
    public float maxHealth = 50f;
    public float armor = 20f;
    public float moveSpeed = 5.5f;

    [Header("Przyzywanie Owcy")]
    public GameObject sheepPrefab;
    public int currentSheepCount = 1;
    public int maxSheepCount = 10;
    public float sheepSpawnCooldown = 45f;
    public float sheepDamage = 20f;
    public bool sheepCanTakeDamage = false;
    public float sheepMaxDamage = 30f;

    [Header("Sheep Resurrection (R)")]
    public float resurrectionCooldown = 45f;
    public float resurrectionCastTime = 2f;

    [Header("Wolf's Feast (T)")]
    public float feastRadius = 3f;
    public float feastFormationTime = 1.5f;
    public float feastDamage = 350f;
    public float explosionRadius = 1f;
    public int remainingSheepAfterFeast = 1;

    [Header("References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public Transform spawnPoint;

    private List<GameObject> activeSheep = new List<GameObject>();
    private List<GameObject> deadSheep = new List<GameObject>();
    private float sheepSpawnTimer = 0f;
    private bool isResurrecting = false;
    private bool isFeasting = false;
    private float currentResurrectCooldown = 0f;
    private float currentFeastCooldown = 0f;
    private bool canSpawnSheep = true;
    private bool isEnabled = true;

    void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.maxHealth = maxHealth;
            playerHealth.currentHealth = maxHealth;
            playerHealth.UpdateUI();
        }

        if (weaponUpgrade != null)
        {
            sheepDamage = weaponUpgrade.currentSheepDamage;
            sheepSpawnCooldown = weaponUpgrade.currentSheepSpawnCooldown;
            feastDamage = weaponUpgrade.currentFeastDamage;
            feastRadius = weaponUpgrade.currentFeastRadius;
        }

        if (spawnPoint == null) spawnPoint = transform;

        StartCoroutine(SpawnInitialSheep());
    }

    IEnumerator SpawnInitialSheep()
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < currentSheepCount; i++)
        {
            SpawnSheep();
        }
    }

    void Update()
    {
        if (!isEnabled || isResurrecting || isFeasting) return;

        if (canSpawnSheep && currentSheepCount < maxSheepCount)
        {
            sheepSpawnTimer += Time.deltaTime;
            if (sheepSpawnTimer >= sheepSpawnCooldown)
            {
                sheepSpawnTimer = 0f;
                SpawnSheep();
            }
        }

        if (currentResurrectCooldown > 0)
        {
            currentResurrectCooldown -= Time.deltaTime;
        }

        if (currentFeastCooldown > 0)
        {
            currentFeastCooldown -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.R) && currentResurrectCooldown <= 0 && !isResurrecting)
        {
            StartCoroutine(SheepResurrection());
        }

        if (Input.GetKeyDown(KeyCode.T) && currentFeastCooldown <= 0 && !isFeasting && activeSheep.Count > 0)
        {
            StartCoroutine(WolfsFeast());
        }
    }

    void SpawnSheep()
    {
        if (sheepPrefab == null) return;

        Vector3 spawnPos = spawnPoint.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        GameObject sheep = Instantiate(sheepPrefab, spawnPos, Quaternion.identity);

        Sheep sheepScript = sheep.GetComponent<Sheep>();
        if (sheepScript == null)
        {
            sheepScript = sheep.AddComponent<Sheep>();
        }

        sheepScript.Initialize(this, sheepDamage, sheepCanTakeDamage, sheepMaxDamage);

        activeSheep.Add(sheep);
        currentSheepCount = activeSheep.Count;

        Debug.Log("Przyzwano owce! Liczba owiec: " + currentSheepCount);
    }

    // METODA SheepDied - potrzebna dla owiec
    public void SheepDied(GameObject sheep)
    {
        if (activeSheep.Contains(sheep))
        {
            activeSheep.Remove(sheep);
            deadSheep.Add(sheep);
        }
        currentSheepCount = activeSheep.Count;
        Debug.Log("Owca umarla. Pozostale owce: " + currentSheepCount);
    }

    IEnumerator SheepResurrection()
    {
        isResurrecting = true;
        currentResurrectCooldown = resurrectionCooldown;

        Debug.Log("Zmartwychwstanie owiec - rozkladanie...");

        float castTimer = 0f;
        while (castTimer < resurrectionCastTime)
        {
            castTimer += Time.deltaTime;
            yield return null;
        }

        int resurrectedCount = 0;
        foreach (GameObject sheep in deadSheep)
        {
            if (sheep != null)
            {
                Sheep sheepScript = sheep.GetComponent<Sheep>();
                if (sheepScript != null)
                {
                    sheepScript.Resurrect();
                    activeSheep.Add(sheep);
                    resurrectedCount++;
                }
            }
        }

        deadSheep.Clear();
        currentSheepCount = activeSheep.Count;

        isResurrecting = false;
        Debug.Log("Zmartwychwstalo " + resurrectedCount + " owiec!");
    }

    IEnumerator WolfsFeast()
    {
        isFeasting = true;
        currentFeastCooldown = 60f;

        Debug.Log("Wilcza Uczta - owce ustawiaja sie w kole...");

        if (activeSheep.Count == 0)
        {
            isFeasting = false;
            yield break;
        }

        // Zapisz pozycje owiec
        List<Vector3> sheepPositions = new List<Vector3>();
        float angleStep = 360f / activeSheep.Count;

        for (int i = 0; i < activeSheep.Count; i++)
        {
            if (activeSheep[i] != null)
            {
                float angle = angleStep * i;
                Vector3 pos = transform.position + new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * feastRadius, 0, Mathf.Cos(angle * Mathf.Deg2Rad) * feastRadius);
                sheepPositions.Add(pos);

                Sheep sheepScript = activeSheep[i].GetComponent<Sheep>();
                if (sheepScript != null) sheepScript.SetFormationMode(true);

                activeSheep[i].transform.position = pos;
            }
        }

        // Czekaj na ustawienie
        float formationTimer = 0f;
        while (formationTimer < feastFormationTime)
        {
            formationTimer += Time.deltaTime;
            yield return null;
        }

        // Eksplozje
        int totalDamage = 0;
        foreach (GameObject sheep in activeSheep)
        {
            if (sheep != null)
            {
                Collider[] enemies = Physics.OverlapSphere(sheep.transform.position, explosionRadius);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        enemyHealth e = enemy.GetComponent<enemyHealth>();
                        if (e != null)
                        {
                            e.TakeDamage(feastDamage);
                            totalDamage++;
                        }
                    }
                }
                Destroy(sheep);
            }
        }

        activeSheep.Clear();

        // Przywroc pozostale owce
        for (int i = 0; i < remainingSheepAfterFeast; i++)
        {
            SpawnSheep();
        }

        isFeasting = false;
        Debug.Log("Wilcza Uczta zadala " + totalDamage + " obrazen!");
    }

    public void DisableAbilities()
    {
        isEnabled = false;
    }

    public void EnableAbilities()
    {
        isEnabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, feastRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}