using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShepherdAbilities : MonoBehaviour
{
    [Header("═══════════════ PASTERZ STATS ═══════════════")]
    public float maxHealth = 50f;
    public float armor = 20f;

    [Header("═══════════════ PRZYZYWANIE OWCY ═══════════════")]
    public GameObject sheepPrefab;
    public int maxSheepCount = 10;
    public float sheepSpawnCooldown = 3f;
    public float sheepDamage = 20f;

    [Header("═══════════════ ZMARTWYCHWSTANIE (Q) ═══════════════")]
    public float resurrectionCooldown = 45f;

    [Header("═══════════════ WILCZA UCZTA (R) ═══════════════")]
    public float feastRadius = 3f;
    public float feastDamage = 350f;
    public float explosionRadius = 1f;
    public int remainingSheepAfterFeast = 1;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public Transform spawnPoint;

    private List<GameObject> activeSheep = new List<GameObject>();
    private List<GameObject> deadSheep = new List<GameObject>();
    private float spawnTimer = 0f;
    private bool isResurrecting = false;
    private bool isFeasting = false;
    private float resurrectCooldownTimer = 0f;
    private float feastCooldownTimer = 0f;

    void Start()
    {
        Debug.Log("🐑 PASTERZ START");
        Debug.Log($"✅ SheepPrefab: {(sheepPrefab != null ? sheepPrefab.name : "NULL")}");

        if (playerHealth != null)
            playerHealth.SetBaseHealth(maxHealth, armor);

        if (sheepPrefab == null)
        {
            Debug.LogError("❌ SheepPrefab NIE JEST PRZYPISANY! Przeciągnij SheepVar!");
            return;
        }

        if (spawnPoint == null) spawnPoint = transform;

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        Debug.Log("🐑 Uruchamiam pętlę spawnu");

        yield return new WaitForSeconds(0.5f);
        SpawnSheep();

        while (true)
        {
            yield return new WaitForSeconds(sheepSpawnCooldown);

            if (!isResurrecting && !isFeasting && activeSheep.Count < maxSheepCount)
                SpawnSheep();
        }
    }

    void Update()
    {
        // TEST: Spawn na K
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("🔴 Ręczny spawn (K)!");
            SpawnSheep();
        }

        // TEST: Info na F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log($"🐑 Owce: {activeSheep.Count}/{maxSheepCount} | Martwe: {deadSheep.Count}");
            Debug.Log($"   Prefab: {(sheepPrefab != null ? sheepPrefab.name : "NULL")}");
        }

        // Cooldowny
        if (resurrectCooldownTimer > 0)
            resurrectCooldownTimer -= Time.deltaTime;
        if (feastCooldownTimer > 0)
            feastCooldownTimer -= Time.deltaTime;

        // ZMARTWYCHWSTANIE (Q)
        if (Input.GetKeyDown(KeyCode.Q) && resurrectCooldownTimer <= 0 && !isResurrecting)
            StartCoroutine(SheepResurrection());

        // WILCZA UCZTA (R)
        if (Input.GetKeyDown(KeyCode.R) && feastCooldownTimer <= 0 && !isFeasting && activeSheep.Count > 0)
            StartCoroutine(WolfsFeast());
    }

    void SpawnSheep()
    {
        if (sheepPrefab == null)
        {
            Debug.LogError("❌ sheepPrefab NULL!");
            return;
        }

        Vector3 pos = spawnPoint.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        GameObject sheep = Instantiate(sheepPrefab, pos, Quaternion.identity);
        sheep.name = $"Sheep_{activeSheep.Count + 1}";

        Sheep script = sheep.GetComponent<Sheep>();
        if (script == null) script = sheep.AddComponent<Sheep>();

        script.Initialize(this, sheepDamage, false, 30f);

        activeSheep.Add(sheep);
        Debug.Log($"✅ Owca przyzwana! ({activeSheep.Count}/{maxSheepCount})");
    }

    public void SheepDied(GameObject sheep)
    {
        if (activeSheep.Contains(sheep))
        {
            activeSheep.Remove(sheep);
            deadSheep.Add(sheep);
        }
        Debug.Log($"💀 Owca umarła. Żywych: {activeSheep.Count}, Martwych: {deadSheep.Count}");
    }

    IEnumerator SheepResurrection()
    {
        isResurrecting = true;
        resurrectCooldownTimer = resurrectionCooldown;

        Debug.Log("🔄 Zmartwychwstanie...");

        yield return new WaitForSeconds(1f);

        int count = 0;
        foreach (GameObject sheep in deadSheep)
        {
            if (sheep != null)
            {
                Sheep script = sheep.GetComponent<Sheep>();
                if (script != null)
                {
                    script.Resurrect();
                    activeSheep.Add(sheep);
                    count++;
                }
            }
        }

        deadSheep.Clear();
        isResurrecting = false;
        Debug.Log($"✨ Zmartwychwstało {count} owiec!");
    }

    IEnumerator WolfsFeast()
    {
        isFeasting = true;
        feastCooldownTimer = 60f;

        Debug.Log($"🐺 Wilcza Uczta! ({activeSheep.Count} owiec)");

        if (activeSheep.Count == 0)
        {
            isFeasting = false;
            yield break;
        }

        // Ustaw owce w kole
        float angleStep = 360f / activeSheep.Count;
        for (int i = 0; i < activeSheep.Count; i++)
        {
            if (activeSheep[i] != null)
            {
                float angle = angleStep * i;
                Vector3 pos = transform.position + new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * feastRadius,
                    0,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * feastRadius
                );
                activeSheep[i].transform.position = pos;

                Sheep script = activeSheep[i].GetComponent<Sheep>();
                if (script != null) script.SetFormationMode(true);
            }
        }

        yield return new WaitForSeconds(1.5f);

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

        // Zostań z kilkoma owcami
        for (int i = 0; i < remainingSheepAfterFeast; i++)
            SpawnSheep();

        isFeasting = false;
        Debug.Log($"🐺 Wilcza Uczta: {totalDamage} obrażeń!");
    }

    public void DisableAbilities() { isEnabled = false; }
    public void EnableAbilities() { isEnabled = true; }

    private bool isEnabled = true;
}