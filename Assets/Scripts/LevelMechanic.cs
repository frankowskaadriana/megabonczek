using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class LevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;
    public int currentXP = 0;
    public int xpRequired = 10;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // Oryginalny prefab - NIE Modyfikowany

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemiesLeftText;
    public GameObject perkSelectionPanel;
    public TextMeshProUGUI perkText1;
    public TextMeshProUGUI perkText2;
    public TextMeshProUGUI perkText3;

    [Header("Player References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public AbilitiesMountainMan mountainManAbilities;
    public SeraphimAbilities seraphimAbilities;
    public ShepherdAbilities shepherdAbilities;

    private int enemiesAlive = 0;
    private int enemiesKilled = 0;
    private bool isRespawning = false;
    private bool gameStarted = false;
    private bool isPerkSelectionActive = false;
    private CursorLockMode previousLockMode;

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

        if (enemiesAlive <= 0 && !isRespawning && enemiesKilled >= enemiesToKill)
        {
            LevelUp();
        }
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
        UpdateUI();
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            StartCoroutine(SpawnEnemies());
            Debug.Log("Gra rozpoczeta! Spawnuje wrogow...");
        }
    }

    IEnumerator SpawnEnemies()
    {
        isRespawning = true;
        enemiesAlive = enemiesToKill;
        UpdateUI();

        // ZnajdŸ pozycjê gracza
        Vector3 center = transform.position;
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) center = player.transform.position;

        for (int i = 0; i < enemiesToKill; i++)
        {
            Vector3 spawnPos = center + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));

            // ZnajdŸ najbli¿szy punkt na NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            // Instantiate tworzy KOPIÊ prefaba - oryginalny prefab pozostaje nienaruszony!
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Enemy_Clone_" + (i + 1); // Nazwa wskazuje ¿e to kopia

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