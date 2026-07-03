using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelSystem : MonoBehaviour
{
    [Header("Poziom i XP")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpRequired = 10;
    public int xpPerEnemy = 1;

    [Header("Statystyki przeciwnikow")]
    public float baseEnemyHealth = 30f;
    public float healthIncreasePerLevel = 5f;
    public float healthIncreasePerWave = 2f;
    public int baseExpReward = 10;
    public int expIncreasePerLevel = 1;
    public int expIncreasePerWave = 1;

    [Header("Czas gry")]
    public float gameTime = 0f;
    public float difficultyMultiplier = 1f;

    [Header("UI - TEKSTY")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;

    [Header("Panel wyboru perkow")]
    public GameObject perkPanel;
    public Transform perkContainer;
    public GameObject perkButtonPrefab;
    public TextMeshProUGUI perkStatsText;

    [Header("Komunikaty")]
    public TextMeshProUGUI levelUpMessage;
    public float messageDuration = 2f;

    [Header("Folder z ikonami")]
    public string iconFolderPath = "PerkIcons/";

    [Header("Referencje")]
    public WaveSpawner waveSpawner;
    public PlayerUI playerUI; // Referencja do PlayerUI

    private PlayerHealth playerHealth;
    private bool isChoosingPerk = false;
    private float targetXpFill = 0f;
    private float currentXpFill = 0f;
    private bool gameStarted = false;
    private float messageTimer = 0f;
    private bool showMessage = false;

    private List<PerkData> allPerks = new List<PerkData>();
    private List<PerkData> currentPerks = new List<PerkData>();
    private Dictionary<string, int> perkLevels = new Dictionary<string, int>();

    private int currentDamageBonus = 0;
    private int currentHealthBonus = 0;
    private float currentAttackSpeedBonus = 0f;
    private float currentSpeedBonus = 0f;
    private int currentXpBonus = 0;
    private int currentArmorBonus = 0;
    private float currentRangeBonus = 0f;

    private float currentEnemyHealth;
    private int currentExpReward;

    private float baseAttackDamage = 25f;
    private float baseAttackRange = 3f;
    private float baseAttackRate = 1f;
    private float baseMoveSpeed = 5f;

    [System.Serializable]
    public class PerkData
    {
        public string id;
        public string name;
        public string description;
        public string iconName;
        public int maxLevel = 5;
        public System.Action apply;
        public System.Func<string> getDescription;

        public PerkData(string id, string name, string iconName, System.Action apply, System.Func<string> getDescription, int maxLevel = 5)
        {
            this.id = id;
            this.name = name;
            this.iconName = iconName;
            this.apply = apply;
            this.getDescription = getDescription;
            this.maxLevel = maxLevel;
            this.description = "";
        }
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (waveSpawner == null) waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (playerUI == null) playerUI = FindFirstObjectByType<PlayerUI>();

        GetBaseStats();
        CreatePerks();

        if (perkPanel != null) perkPanel.SetActive(false);
        if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);

        Time.timeScale = 1f;
        isChoosingPerk = false;

        UpdateEnemyStats();
        UpdateUI();
        UpdatePerkStatsUI();
        Debug.Log("LevelSystem gotowy!");
    }

    void GetBaseStats()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null)
        {
            baseAttackDamage = mountain.attackDamage;
            baseAttackRange = mountain.attackRange;
            baseAttackRate = mountain.attackRate;
        }

        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null)
        {
            baseAttackDamage = seraphim.attackDamage;
            baseAttackRange = seraphim.attackRange;
            baseAttackRate = seraphim.attackRate;
        }

        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
        {
            baseMoveSpeed = movement.maxSpeed;
        }
    }

    void Update()
    {
        if (gameStarted)
        {
            gameTime += Time.deltaTime;
            difficultyMultiplier = 1f + (gameTime / 60f) * 0.05f;
            difficultyMultiplier = Mathf.Min(difficultyMultiplier, 3f);

            if (Time.frameCount % 300 == 0)
            {
                UpdateEnemyStats();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = 1f;
            isChoosingPerk = false;
            if (perkPanel != null) perkPanel.SetActive(false);
            Debug.Log("AWARYJNE ODBLOKOWANIE GRY!");
        }

        if (!gameStarted) return;

        if (playerHealth == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        }

        // XP fill - aktualizacja przez PlayerUI
        if (playerUI != null)
        {
            playerUI.UpdateUI();
        }

        if (isChoosingPerk)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChoosePerk(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ChoosePerk(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) ChoosePerk(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) ChoosePerk(3);
        }

        if (showMessage)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0)
            {
                showMessage = false;
                if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);
            }
        }

        UpdateUI();
    }

    void CreatePerks()
    {
        allPerks.Clear();

        // ============================================================
        // UNIWERSALNE PERKI
        // ============================================================

        allPerks.Add(new PerkData("damage", "Obrazenia", "IkonaAtak", () => {
            currentDamageBonus += 10;
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackDamage += 10f;
            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackDamage += 10f;
            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackDamage += 10f;
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentDamage();
            return "+10 obrazen (obecnie: " + current.ToString("F0") + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("range", "Zasieg", "IkonaZasieg", () => {
            currentRangeBonus += 0.2f;
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRange += 0.2f;
            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackRange += 0.2f;
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentRange();
            return "+0.2 zasiegu (obecnie: " + current.ToString("F1") + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("health", "Zdrowie", "IkonaZdrowie", () => {
            currentHealthBonus += 25;
            if (playerHealth != null) playerHealth.AddMaxHealth(25);
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentHealth();
            return "+25 maksymalnego HP (obecnie: " + current.ToString("F0") + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("attackSpeed", "Szybkosc ataku", "IkonaSzybkoscAtaku", () => {
            currentAttackSpeedBonus += 0.15f;
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRate = Mathf.Max(0.2f, mountain.attackRate - 0.15f);
            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackRate = Mathf.Max(0.2f, seraphim.attackRate - 0.15f);
            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackRate = Mathf.Max(0.2f, shepherd.attackRate - 0.15f);
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentAttackRate();
            return "-0.15s cooldown (obecnie: " + current.ToString("F2") + "s)";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("speed", "Predkosc ruchu", "IkonaPredkosc", () => {
            currentSpeedBonus += 0.08f;
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null) movement.maxSpeed *= 1.08f;
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentSpeed();
            return "+8% predkosci ruchu (obecnie: " + current.ToString("F1") + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("xp", "Wiecej XP", "IkonaXP", () => {
            currentXpBonus += 1;
            xpPerEnemy++;
            UpdatePerkStatsUI();
        }, () => {
            return "+1 XP za wroga (obecnie: " + xpPerEnemy + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("armor", "Pancerz", "IkonaPancerz", () => {
            currentArmorBonus += 10;
            if (playerHealth != null) playerHealth.AddArmor(10);
            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentArmor();
            return "+10 pancerza (obecnie: " + current.ToString("F0") + ")";
        }, maxLevel: 5));

        allPerks.Add(new PerkData("bleed", "Krwawy cios", "IkonaKrwawyCios", () => {
            Debug.Log("Krwawy cios aktywowany!");
        }, () => {
            return "Wrogowie krwawia przez 3s";
        }, maxLevel: 3));

        allPerks.Add(new PerkData("shield", "Tarcza", "IkonaTarcza", () => {
            Debug.Log("Tarcza aktywowana!");
        }, () => {
            return "Otrzymujesz tarcze na 5s";
        }, maxLevel: 3));

        allPerks.Add(new PerkData("vampire", "Wampiryzm", "IkonaWampiryzm", () => {
            Debug.Log("Wampiryzm aktywowany!");
        }, () => {
            return "10% obrazen leczonych jako HP";
        }, maxLevel: 3));

        allPerks.Add(new PerkData("ultimateDuration", "Wydluzenie ultimate", "IkonaUltimateCzas", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.ultimateCooldown += 2f;

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.judgmentDuration += 2f;

            UpdatePerkStatsUI();
        }, () => {
            return "+2s czasu ultimate (obecnie: " + GetCurrentUltimateDuration().ToString("F1") + "s)";
        }, maxLevel: 3));

        allPerks.Add(new PerkData("ultimateDamage", "Obrazenia ultimate", "IkonaUltimateObrazenia", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.ultimateDamage += 15f;

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.judgmentDamage += 15f;

            UpdatePerkStatsUI();
        }, () => {
            float current = GetCurrentUltimateDamage();
            return "+15 obrazen ultimate (obecnie: " + current.ToString("F0") + ")";
        }, maxLevel: 3));

        // ============================================================
        // UNIKALNE PERKI - GÓRAL
        // ============================================================
        bool isGoral = FindFirstObjectByType<AbilitiesMountainMan>() != null;

        if (isGoral)
        {
            allPerks.Add(new PerkData("goral_mocnyCios", "Mocny cios", "GoralIkonaMocnyCios", () => {
                AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
                if (mountain != null)
                {
                    mountain.attackDamage += 15f;
                    mountain.attackRate += 0.3f;
                }
                UpdatePerkStatsUI();
            }, () => {
                float dmg = FindFirstObjectByType<AbilitiesMountainMan>()?.attackDamage ?? 25f;
                return "+15 obrazen, ale wolniejszy atak (obecnie: " + dmg.ToString("F0") + ")";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("goral_ziemia", "Uderzenie w ziemie", "GoralIkonaZiemia", () => {
                AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
                if (mountain != null) mountain.stompRange += 1f;
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<AbilitiesMountainMan>()?.stompRange ?? 5f;
                return "+1 zasiegu Stomp (obecnie: " + current.ToString("F1") + ")";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("goral_wytrzymalosc", "Wytrzymalosc", "GoralIkonaWytrzymalosc", () => {
                if (playerHealth != null)
                {
                    playerHealth.AddMaxHealth(30);
                    playerHealth.AddArmor(5);
                }
                UpdatePerkStatsUI();
            }, () => {
                float hp = playerHealth?.maxHealth ?? 100f;
                return "+30 HP, +5 pancerza (obecnie: " + hp.ToString("F0") + " HP)";
            }, maxLevel: 3));

            // ============================================================
            // GÓRAL - PERKI CZASU UMIEJĘTNOŚCI
            // ============================================================

            allPerks.Add(new PerkData("goral_stompTime", "Dlugi stomp", "GoralIkonaStompCzas", () => {
                WeaponUpgradeSystem weapon = FindFirstObjectByType<WeaponUpgradeSystem>();
                if (weapon != null) weapon.UpgradeStompDuration();
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<WeaponUpgradeSystem>()?.currentStompDuration ?? 0.5f;
                return "+0.2s czasu Stomp (obecnie: " + current.ToString("F2") + "s)";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("goral_specialTime", "Dlugi special", "GoralIkonaSpecialCzas", () => {
                WeaponUpgradeSystem weapon = FindFirstObjectByType<WeaponUpgradeSystem>();
                if (weapon != null) weapon.UpgradeSpecialDuration();
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<WeaponUpgradeSystem>()?.currentSpecialDuration ?? 0.5f;
                return "+0.2s czasu Special (obecnie: " + current.ToString("F2") + "s)";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("goral_ultimateTime", "Dlugi ultimate", "GoralIkonaUltimateCzas", () => {
                WeaponUpgradeSystem weapon = FindFirstObjectByType<WeaponUpgradeSystem>();
                if (weapon != null) weapon.UpgradeUltimateTime();
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<WeaponUpgradeSystem>()?.currentUltimateTime ?? 3f;
                return "+1s czasu Ultimate (obecnie: " + current.ToString("F1") + "s)";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("goral_specialCD", "Szybszy special", "GoralIkonaSpecialCD", () => {
                WeaponUpgradeSystem weapon = FindFirstObjectByType<WeaponUpgradeSystem>();
                if (weapon != null) weapon.UpgradeSpecialCooldownReduction();
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<WeaponUpgradeSystem>()?.currentSpecialCooldownReduction ?? 0f;
                return "-1.5s cooldown Special (obecnie: -" + current.ToString("F1") + "s)";
            }, maxLevel: 3));
        }

        // ============================================================
        // UNIKALNE PERKI - SERAPHIM
        // ============================================================
        bool isSeraphim = FindFirstObjectByType<AbilitiesSeraphim>() != null;

        if (isSeraphim)
        {
            allPerks.Add(new PerkData("seraphim_swiatlo", "Swiatlo", "SeraphimIkonaSwiatlo", () => {
                AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
                if (seraphim != null) seraphim.attackRange += 3f;
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<AbilitiesSeraphim>()?.attackRange ?? 10f;
                return "+3 zasiegu lasera (obecnie: " + current.ToString("F1") + ")";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("seraphim_uzdrowienie", "Uzdrowienie", "SeraphimIkonaUzdrowienie", () => {
                AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
                if (seraphim != null) seraphim.healAmount += 20f;
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<AbilitiesSeraphim>()?.healAmount ?? 30f;
                return "+20 leczenia (obecnie: " + current.ToString("F0") + ")";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("seraphim_aniol", "Anielska tarcza", "SeraphimIkonaAniol", () => {
                AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
                if (seraphim != null) seraphim.shieldDuration += 2f;
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<AbilitiesSeraphim>()?.shieldDuration ?? 5f;
                return "+2s tarczy (obecnie: " + current.ToString("F1") + "s)";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("seraphim_pierce", "Przebicie", "SeraphimIkonaPrzebicie", () => {
                WeaponUpgradeSystem weapon = FindFirstObjectByType<WeaponUpgradeSystem>();
                if (weapon != null) weapon.UpgradePierce();
                UpdatePerkStatsUI();
            }, () => {
                int count = FindFirstObjectByType<WeaponUpgradeSystem>()?.pierceCount ?? 0;
                return "Laser przebija wrogow (obecnie: " + count + " przebic)";
            }, maxLevel: 2));
        }

        // ============================================================
        // UNIKALNE PERKI - PASTERZ
        // ============================================================
        bool isShepherd = FindFirstObjectByType<ShepherdAbilities>() != null;

        if (isShepherd)
        {
            allPerks.Add(new PerkData("shepherd_owca", "Wiecej owiec", "PasterzIkonaOwca", () => {
                ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
                if (shepherd != null) shepherd.maxSheep++;
                UpdatePerkStatsUI();
            }, () => {
                int current = FindFirstObjectByType<ShepherdAbilities>()?.maxSheep ?? 3;
                return "+1 maksymalna owca (obecnie: " + current + ")";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("shepherd_stado", "Stado", "PasterzIkonaStado", () => {
                ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
                if (shepherd != null) shepherd.sheepAttackCooldown = Mathf.Max(0.3f, shepherd.sheepAttackCooldown - 0.2f);
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<ShepherdAbilities>()?.sheepAttackCooldown ?? 1f;
                return "Owce atakuja szybciej (obecnie: " + current.ToString("F2") + "s)";
            }, maxLevel: 3));

            allPerks.Add(new PerkData("shepherd_pasterz", "Mistrz pasterz", "PasterzIkonaPasterz", () => {
                ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
                if (shepherd != null) shepherd.sheepAttackDamage += 10f;
                UpdatePerkStatsUI();
            }, () => {
                float current = FindFirstObjectByType<ShepherdAbilities>()?.sheepAttackDamage ?? 15f;
                return "+10 obrazen owiec (obecnie: " + current.ToString("F0") + ")";
            }, maxLevel: 3));
        }
    }

    // ============================================================
    // POMOCNICZE - OBLICZANIE AKTUALNYCH STATYSTYK
    // ============================================================

    float GetCurrentDamage()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null) return mountain.attackDamage;
        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null) return seraphim.attackDamage;
        ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
        if (shepherd != null) return shepherd.attackDamage;
        return baseAttackDamage;
    }

    float GetCurrentRange()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null) return mountain.attackRange;
        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null) return seraphim.attackRange;
        return baseAttackRange;
    }

    float GetCurrentHealth()
    {
        if (playerHealth != null) return playerHealth.maxHealth;
        return 100f;
    }

    float GetCurrentAttackRate()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null) return mountain.attackRate;
        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null) return seraphim.attackRate;
        ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
        if (shepherd != null) return shepherd.attackRate;
        return baseAttackRate;
    }

    float GetCurrentSpeed()
    {
        PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
        if (movement != null) return movement.maxSpeed;
        return baseMoveSpeed;
    }

    float GetCurrentArmor()
    {
        if (playerHealth != null) return playerHealth.armor;
        return 0f;
    }

    float GetCurrentUltimateDuration()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null) return mountain.ultimateCooldown;
        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null) return seraphim.judgmentDuration;
        return 3f;
    }

    float GetCurrentUltimateDamage()
    {
        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null) return mountain.ultimateDamage;
        AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
        if (seraphim != null) return seraphim.judgmentDamage;
        return 50f;
    }

    // ============================================================
    // WYSZUKIWANIE IKON
    // ============================================================

    Sprite LoadPerkIcon(string iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return null;

        Sprite icon = Resources.Load<Sprite>(iconFolderPath + iconName);
        if (icon != null) return icon;

        icon = Resources.Load<Sprite>(iconName);
        if (icon != null) return icon;

        Debug.LogWarning("Nie znaleziono ikony: " + iconName);
        return null;
    }

    // ============================================================
    // WYŚWIETLANIE STATYSTYK PERKÓW
    // ============================================================

    void UpdatePerkStatsUI()
    {
        if (perkStatsText == null) return;

        string stats = "";

        if (currentDamageBonus > 0)
            stats += "Obrazenia: +" + currentDamageBonus + "\n";

        if (currentHealthBonus > 0)
            stats += "Zdrowie: +" + currentHealthBonus + "\n";

        if (currentAttackSpeedBonus > 0)
            stats += "Szybkosc ataku: -" + currentAttackSpeedBonus.ToString("F2") + "s\n";

        if (currentSpeedBonus > 0)
            stats += "Predkosc: +" + (currentSpeedBonus * 100).ToString("F0") + "%\n";

        if (currentXpBonus > 0)
            stats += "XP: +" + currentXpBonus + "\n";

        if (currentArmorBonus > 0)
            stats += "Pancerz: +" + currentArmorBonus + "\n";

        if (currentRangeBonus > 0)
            stats += "Zasieg: +" + currentRangeBonus.ToString("F1") + "\n";

        if (string.IsNullOrEmpty(stats))
        {
            stats = "Brak ulepszen";
        }

        perkStatsText.text = stats;
    }

    int GetPerkLevel(string id)
    {
        if (perkLevels.ContainsKey(id))
            return perkLevels[id];
        return 0;
    }

    // ============================================================
    // POKAZYWANIE PERKÓW
    // ============================================================

    void ShowPerkSelection()
    {
        if (isChoosingPerk) return;

        isChoosingPerk = true;
        Time.timeScale = 0f;

        currentPerks.Clear();
        List<PerkData> temp = new List<PerkData>(allPerks);

        for (int i = temp.Count - 1; i >= 0; i--)
        {
            if (GetPerkLevel(temp[i].id) >= temp[i].maxLevel)
            {
                temp.RemoveAt(i);
            }
        }

        if (temp.Count == 0)
        {
            temp = new List<PerkData>(allPerks);
        }

        int perksToShow = Mathf.Min(4, temp.Count);
        while (currentPerks.Count < perksToShow && temp.Count > 0)
        {
            int idx = Random.Range(0, temp.Count);
            currentPerks.Add(temp[idx]);
            temp.RemoveAt(idx);
        }

        // ============================================================
        // UŻYJ GOTOWYCH PRZYCISKÓW
        // ============================================================
        if (perkContainer != null)
        {
            Button[] perkButtons = perkContainer.GetComponentsInChildren<Button>();

            for (int i = 0; i < perkButtons.Length && i < 4; i++)
            {
                Button btn = perkButtons[i];

                if (i < currentPerks.Count)
                {
                    PerkData perk = currentPerks[i];

                    btn.gameObject.SetActive(true);

                    int index = i;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => ChoosePerk(index));

                    // Szukaj dzieci
                    Image iconImg = btn.transform.Find("Icon")?.GetComponent<Image>();
                    if (iconImg == null)
                    {
                        iconImg = btn.GetComponentInChildren<Image>();
                    }

                    TextMeshProUGUI[] allTexts = btn.GetComponentsInChildren<TextMeshProUGUI>();
                    TextMeshProUGUI nameTxt = allTexts.Length > 0 ? allTexts[0] : null;
                    TextMeshProUGUI descTxt = allTexts.Length > 1 ? allTexts[1] : null;

                    // Ustaw ikonę
                    if (iconImg != null)
                    {
                        Sprite icon = LoadPerkIcon(perk.iconName);
                        if (icon != null)
                        {
                            iconImg.sprite = icon;
                            iconImg.gameObject.SetActive(true);
                        }
                        else
                        {
                            iconImg.gameObject.SetActive(false);
                        }
                    }

                    // Ustaw nazwę
                    if (nameTxt != null)
                    {
                        string lvlText = GetPerkLevel(perk.id) + 1 + "/" + perk.maxLevel;
                        nameTxt.text = perk.name + " (Lvl " + lvlText + ")";
                    }

                    // Ustaw opis
                    if (descTxt != null)
                    {
                        descTxt.text = perk.getDescription?.Invoke() ?? perk.description;
                    }
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        if (perkPanel != null)
        {
            perkPanel.SetActive(true);
            Debug.Log("Panel perkow widoczny");
        }

        ShowLevelUpMessage("AWANS! Wybierz perka (1-4)");
        AudioManager.Instance?.PlayPerkSelect();

        Debug.Log("=== WYBIERZ PERKA (1-4) ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log((i + 1) + ". " + currentPerks[i].name + " - " + currentPerks[i].getDescription?.Invoke());
    }

    void ChoosePerk(int index)
    {
        if (!isChoosingPerk) return;
        if (index >= currentPerks.Count) return;

        currentPerks[index].apply();

        string id = currentPerks[index].id;
        if (perkLevels.ContainsKey(id))
            perkLevels[id]++;
        else
            perkLevels[id] = 1;

        Debug.Log("WYBRANO: " + currentPerks[index].name + " (Poziom " + perkLevels[id] + "/" + currentPerks[index].maxLevel + ")");

        isChoosingPerk = false;

        if (perkPanel != null) perkPanel.SetActive(false);
        if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);

        Time.timeScale = 1f;

        AudioManager.Instance?.PlayPerkSelect();

        if (waveSpawner != null && waveSpawner.GetEnemyCount() == 0 && !waveSpawner.IsSpawning())
        {
            StartCoroutine(ResumeWaveAfterPerk());
        }
    }

    IEnumerator ResumeWaveAfterPerk()
    {
        yield return new WaitForSeconds(0.5f);
        if (waveSpawner != null && waveSpawner.GetEnemyCount() == 0 && !waveSpawner.IsSpawning())
        {
            waveSpawner.StartNextWave();
        }
    }

    void ShowLevelUpMessage(string text)
    {
        if (levelUpMessage != null)
        {
            levelUpMessage.text = text;
            levelUpMessage.gameObject.SetActive(true);
            showMessage = true;
            messageTimer = messageDuration;
        }
    }

    // ============================================================
    // STATYSTYKI PRZECIWNIKÓW
    // ============================================================

    public void UpdateEnemyStats()
    {
        currentEnemyHealth = baseEnemyHealth + (currentLevel * healthIncreasePerLevel) + (waveSpawner != null ? waveSpawner.GetCurrentWave() * healthIncreasePerWave : 0);
        currentEnemyHealth = Mathf.Min(currentEnemyHealth, 500f);

        currentExpReward = baseExpReward + (currentLevel * expIncreasePerLevel) + (waveSpawner != null ? waveSpawner.GetCurrentWave() * expIncreasePerWave : 0);
        currentExpReward = Mathf.Min(currentExpReward, 100);

        Debug.Log("Statystyki wrogow: HP=" + currentEnemyHealth.ToString("F1") + ", EXP=" + currentExpReward);
    }

    public float GetEnemyHealth() => currentEnemyHealth;
    public int GetEnemyExpReward() => currentExpReward;

    // ============================================================
    // METODY PUBLICZNE
    // ============================================================

    public void EnemyDied()
    {
        if (!gameStarted) return;

        currentXP += xpPerEnemy;
        targetXpFill = (float)currentXP / xpRequired;

        if (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            xpRequired += 10;
            currentLevel++;
            targetXpFill = 0f;
            currentXpFill = 0f;

            if (playerHealth != null)
            {
                playerHealth.LevelUpHealth();
            }

            UpdateEnemyStats();
            AudioManager.Instance?.PlayLevelUp();
            ShowPerkSelection();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        gameStarted = true;
        gameTime = 0f;
        difficultyMultiplier = 1f;
        Time.timeScale = 1f;
        UpdateEnemyStats();
        UpdatePerkStatsUI();
        Debug.Log("Gra rozpoczeta!");
    }

    public float GetGameTime() => gameTime;
    public float GetDifficultyMultiplier() => difficultyMultiplier;

    public void UpdateUI()
    {
        if (playerHealth == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        }

        if (levelText != null) levelText.text = "Poziom: " + currentLevel;
        if (xpText != null) xpText.text = currentXP + " / " + xpRequired;

        if (waveSpawner != null)
        {
            if (waveText != null) waveText.text = "Fala: " + waveSpawner.GetCurrentWave();
            if (enemiesLeftText != null) enemiesLeftText.text = "Wrogowie: " + waveSpawner.GetEnemyCount();
        }

        // Odśwież PlayerUI (paski zdrowia i XP)
        if (playerUI != null)
        {
            playerUI.UpdateUI();
        }
    }

    public bool IsChoosingPerk() => isChoosingPerk;

    public int GetDamageBonus() => currentDamageBonus;
    public int GetHealthBonus() => currentHealthBonus;
    public float GetAttackSpeedBonus() => currentAttackSpeedBonus;
    public float GetSpeedBonus() => currentSpeedBonus;
    public int GetXpBonus() => currentXpBonus;
    public int GetArmorBonus() => currentArmorBonus;
    public float GetRangeBonus() => currentRangeBonus;
}