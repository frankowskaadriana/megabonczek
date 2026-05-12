using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;



public class LevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int enemiesToKill = 5;
    public int currentXP = 0;
    public int xpRequired = 10;

    [Header("Enemy Settings")]
    public GameObject enemyPrefab;

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

    [Header("Cursor")]
    public Texture2D cursorTexture;
    private CursorLockMode previousLockMode;

    private int enemiesAlive = 0;
    private int enemiesKilled = 0;
    private bool isRespawning = false;
    private bool gameStarted = false;
    private bool isPerkSelectionActive = false;

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

        // PERKI UNIWERSALNE
        availablePerks.Add(new Perk("Zdrowie +10", "+10 maksymalnego zdrowia", () => {
            if (playerHealth != null)
            {
                playerHealth.maxHealth += 10;
                playerHealth.currentHealth += 10;
                playerHealth.UpdateUI();
            }
        }));

        // PERKI BRONI (G”RAL)
        availablePerks.Add(new Perk("Obraøenia Broni +10", "+10 obraøeÒ ciupagi", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));

        availablePerks.Add(new Perk("ZasiÍg Broni +0.2m", "ZwiÍksza zasiÍg ciupagi o 0.2m", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
        }));

        availablePerks.Add(new Perk("Rozmiar Zamachu +10∞", "ZwiÍksza kπt zamachu o 10∞", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSwingAngle();
        }));

        // PERKI ZDOLNOåCI
        availablePerks.Add(new Perk("Cooldown Zdolnoúci -1s", "Zmniejsza cooldown Gniewu Tatr o 1s", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));

        availablePerks.Add(new Perk("Obraøenia Zdolnoúci +15", "+15 obraøeÒ Gniewu Tatr", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialDamage();
        }));

        availablePerks.Add(new Perk("Dodatkowy ObrÛt", "Gniew Tatr wykonuje dodatkowy obrÛt", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialRotations();
        }));

        availablePerks.Add(new Perk("Krwawienie", "Gniew Tatr nak≥ada krwawienie", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeBleed();
        }));

        // PERKI ULTIMATE
        availablePerks.Add(new Perk("Czas Trwania Ultimate +2s", "+2 sekundy trwania", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateDuration();
        }));

        availablePerks.Add(new Perk("årednica Aury +0.5m", "ZwiÍksza promieÒ aury", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateRadius();
        }));

        availablePerks.Add(new Perk("Obraøenia Ultimate +15/s", "+15 obraøeÒ na sekundÍ", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateDamage();
        }));

        // DODATKOWE
        availablePerks.Add(new Perk("SzybkoúÊ Ataku", "Szybsze ataki ciupagπ", () => {
            if (mountainManAbilities != null) mountainManAbilities.attackRate = Mathf.Max(0.5f, mountainManAbilities.attackRate - 0.1f);
        }));

        availablePerks.Add(new Perk("Leczenie +20", "Natychmiastowe leczenie", () => {
            if (playerHealth != null) playerHealth.Heal(20f);
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

        if (playerHealth != null)
        {
            playerHealth.LevelUpHealth();
        }

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

        // Sprawdü czy NavMesh istnieje
        NavMeshSurface surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogWarning("Brak NavMeshSurface w scenie! Dodaj NavMeshSurface do Plane i kliknij Bake.");
        }

        for (int i = 0; i < enemiesToKill; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

            // Sprawdü czy pozycja jest na NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }
            else
            {
                Debug.LogWarning("Nie znaleziono pozycji na NavMesh dla wroga!");
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Enemy_" + (i + 1);

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