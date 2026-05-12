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
    public ShepherdAbilities shepherdAbilities;

    [Header("Cursor")]
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
        public string category;
        public int rarity; // 0=zwykly, 1=rzadki, 2=epicki, 3=legendarny, 4=mitologiczny
        public System.Action applyPerk;

        public Perk(string name, string description, string category, int rarity, System.Action apply)
        {
            this.name = name;
            this.description = description;
            this.category = category;
            this.rarity = rarity;
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

        // ========== PERKI UNIWERSALNE ==========
        // Zwykle
        availablePerks.Add(new Perk("Zdrowie +10", "+10 maksymalnego zdrowia", "Uniwersalny", 0, () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(10);
        }));
        availablePerks.Add(new Perk("Zdrowie +20", "+20 maksymalnego zdrowia", "Uniwersalny", 1, () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(20);
        }));
        availablePerks.Add(new Perk("Zdrowie +50", "+50 maksymalnego zdrowia", "Uniwersalny", 2, () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(50);
        }));

        // Wiecej XP
        availablePerks.Add(new Perk("Wiecej XP +1", "+1 XP za zabicie wroga", "Uniwersalny", 1, () => {
            // Zwiêksza XP zdobywane z wrogów
        }));

        // Lifesteal (mitologiczny)
        availablePerks.Add(new Perk("Wampiryzm", "0.5% lifestealu od zadanych obrazen", "Uniwersalny", 4, () => {
            // Dodaje efekt lifestealu
        }));

        // ========== PERKI GÓRALA ==========
        // Obrazenia broni
        availablePerks.Add(new Perk("Ostry kamien", "+10 obrazen ciupagi", "Goral", 0, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));
        availablePerks.Add(new Perk("Mocne uderzenie", "+20 obrazen ciupagi", "Goral", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));

        // Zasieg
        availablePerks.Add(new Perk("Dluzsza reka", "+0.2m zasiegu ciupagi", "Goral", 0, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
        }));

        // Rozmiar zamachu
        availablePerks.Add(new Perk("Szeroki zamach", "+10° kata zamachu", "Goral", 0, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSwingAngle();
        }));

        // Cooldown zdolnosci
        availablePerks.Add(new Perk("Szybszy gniew", "-1s cooldown Gniewu Tatr", "Goral", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));

        // Obrazenia zdolnosci
        availablePerks.Add(new Perk("Moc Tatr", "+15 obrazen Gniewu Tatr", "Goral", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialDamage();
        }));

        // Dodatkowy obrot
        availablePerks.Add(new Perk("Wichrowy taniec", "+1 obrot Gniewu Tatr", "Goral", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialRotations();
        }));

        // Krwawienie
        availablePerks.Add(new Perk("Krwawienie", "Gniew Tatr nak³ada krwawienie", "Goral", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeBleed();
        }));

        // Ultimate
        availablePerks.Add(new Perk("Dlugi grom", "+2s trwania Orlego Gromu", "Goral", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateDuration();
        }));
        availablePerks.Add(new Perk("Szerokie skrzydla", "+0.5m srednicy aury", "Goral", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateRadius();
        }));
        availablePerks.Add(new Perk("Moc orla", "+15/s obrazen ultimate", "Goral", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeUltimateDamage();
        }));

        // ========== PERKI SERAPHIMA ==========
        availablePerks.Add(new Perk("Swietliste ostrza", "+5 obrazen wiazki swiatla", "Seraphim", 0, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));
        availablePerks.Add(new Perk("Podwojny strzal", "+1 pocisk", "Seraphim", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeProjectileCount();
        }));
        availablePerks.Add(new Perk("Przebicie", "Pociski przebijaja wrogow", "Seraphim", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradePierce();
        }));
        availablePerks.Add(new Perk("Szybsza szarza", "-1s cooldown Heavenly Charge", "Seraphim", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));
        availablePerks.Add(new Perk("Dluzsza szarza", "+1m zasiegu Heavenly Charge", "Seraphim", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
        }));

        // ========== PERKI PASTERZA ==========
        availablePerks.Add(new Perk("Twardsza skora", "+5 pancerza", "Pasterz", 0, () => {
            if (playerHealth != null) playerHealth.AddArmor(5);
        }));
        availablePerks.Add(new Perk("Silniejsze owce", "+10 obrazen owcy", "Pasterz", 1, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSheepDamage();
        }));
        availablePerks.Add(new Perk("Szybsze przyzywanie", "-5s cooldown przyzywania owcy", "Pasterz", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSheepSpawnCooldown();
        }));
        availablePerks.Add(new Perk("Wilcza uczta", "+100 obrazen Wilczej Uczty", "Pasterz", 2, () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeFeastDamage();
        }));
    }

    public List<Perk> GetRandomPerks(int count, string characterType)
    {
        List<Perk> validPerks = new List<Perk>();

        foreach (Perk perk in availablePerks)
        {
            if (perk.category == "Uniwersalny" || perk.category == characterType)
            {
                validPerks.Add(perk);
            }
        }

        List<Perk> result = new List<Perk>();
        List<Perk> tempPerks = new List<Perk>(validPerks);

        while (result.Count < count && tempPerks.Count > 0)
        {
            int randomIndex = Random.Range(0, tempPerks.Count);
            result.Add(tempPerks[randomIndex]);
            tempPerks.RemoveAt(randomIndex);
        }

        return result;
    }

    void ShowPerkSelection()
    {
        isPerkSelectionActive = true;
        Time.timeScale = 0f;

        previousLockMode = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        string characterType = GetCharacterType();
        currentPerks = GetRandomPerks(3, characterType);

        if (perkText1 != null && currentPerks.Count > 0)
            perkText1.text = GetPerkText(currentPerks[0]);

        if (perkText2 != null && currentPerks.Count > 1)
            perkText2.text = GetPerkText(currentPerks[1]);

        if (perkText3 != null && currentPerks.Count > 2)
            perkText3.text = GetPerkText(currentPerks[2]);

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(true);

        Debug.Log("=== WYBIERZ PERK ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log((i + 1) + ": " + currentPerks[i].name);
    }

    string GetCharacterType()
    {
        if (mountainManAbilities != null && mountainManAbilities.enabled) return "Goral";
        if (seraphimAbilities != null && seraphimAbilities.enabled) return "Seraphim";
        if (shepherdAbilities != null && shepherdAbilities.enabled) return "Pasterz";
        return "Uniwersalny";
    }

    string GetPerkText(Perk perk)
    {
        string rarityColor = "";
        switch (perk.rarity)
        {
            case 0: rarityColor = "#FFFFFF"; break;
            case 1: rarityColor = "#00FF00"; break;
            case 2: rarityColor = "#3399FF"; break;
            case 3: rarityColor = "#CC33FF"; break;
            case 4: rarityColor = "#FF9900"; break;
        }
        return string.Format("<color={0}>{1}</color>\n{2}", rarityColor, perk.name, perk.description);
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
        currentXP++;

        if (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            xpRequired += 10;
            LevelUp();
        }

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

        for (int i = 0; i < enemiesToKill; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 5f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
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