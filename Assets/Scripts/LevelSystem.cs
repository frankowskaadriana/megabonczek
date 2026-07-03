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
    public TextMeshProUGUI perkStatsText;

    [Header("Miejsca na perki (podpinasz ręcznie)")]
    public Transform perkSlot1;
    public Transform perkSlot2;
    public Transform perkSlot3;
    public Transform perkSlot4;

    [Header("Gotowe przyciski z ikonami (podpinasz ręcznie)")]
    public GameObject perkButton_Attack;
    public GameObject perkButton_Range;
    public GameObject perkButton_Health;
    public GameObject perkButton_AttackSpeed;
    public GameObject perkButton_Speed;
    public GameObject perkButton_XP;
    public GameObject perkButton_Armor;
    public GameObject perkButton_Bleed;
    public GameObject perkButton_Shield;
    public GameObject perkButton_Vampire;
    public GameObject perkButton_UltimateDuration;
    public GameObject perkButton_UltimateDamage;

    // GÓRAL
    public GameObject perkButton_GoralMocnyCios;
    public GameObject perkButton_GoralZiemia;
    public GameObject perkButton_GoralWytrzymalosc;
    public GameObject perkButton_GoralStompCzas;
    public GameObject perkButton_GoralSpecialCzas;
    public GameObject perkButton_GoralUltimateCzas;
    public GameObject perkButton_GoralSpecialCD;

    // SERAPHIM
    public GameObject perkButton_SeraphimSwiatlo;
    public GameObject perkButton_SeraphimUzdrowienie;
    public GameObject perkButton_SeraphimAniol;
    public GameObject perkButton_SeraphimPrzebicie;

    // PASTERZ
    public GameObject perkButton_ShepherdOwca;
    public GameObject perkButton_ShepherdStado;
    public GameObject perkButton_ShepherdPasterz;

    [Header("Komunikaty")]
    public TextMeshProUGUI levelUpMessage;
    public float messageDuration = 2f;

    [Header("Referencje")]
    public WaveSpawner waveSpawner;
    public PlayerUI playerUI;

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

    private Transform[] perkSlots;
    private Dictionary<string, GameObject> perkButtonsMap = new Dictionary<string, GameObject>();

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

        perkSlots = new Transform[] { perkSlot1, perkSlot2, perkSlot3, perkSlot4 };
        BuildPerkButtonMap();

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

    void BuildPerkButtonMap()
    {
        perkButtonsMap.Clear();
        perkButtonsMap["damage"] = perkButton_Attack;
        perkButtonsMap["range"] = perkButton_Range;
        perkButtonsMap["health"] = perkButton_Health;
        perkButtonsMap["attackSpeed"] = perkButton_AttackSpeed;
        perkButtonsMap["speed"] = perkButton_Speed;
        perkButtonsMap["xp"] = perkButton_XP;
        perkButtonsMap["armor"] = perkButton_Armor;
        perkButtonsMap["bleed"] = perkButton_Bleed;
        perkButtonsMap["shield"] = perkButton_Shield;
        perkButtonsMap["vampire"] = perkButton_Vampire;
        perkButtonsMap["ultimateDuration"] = perkButton_UltimateDuration;
        perkButtonsMap["ultimateDamage"] = perkButton_UltimateDamage;
        perkButtonsMap["goral_mocnyCios"] = perkButton_GoralMocnyCios;
        perkButtonsMap["goral_ziemia"] = perkButton_GoralZiemia;
        perkButtonsMap["goral_wytrzymalosc"] = perkButton_GoralWytrzymalosc;
        perkButtonsMap["goral_stompTime"] = perkButton_GoralStompCzas;
        perkButtonsMap["goral_specialTime"] = perkButton_GoralSpecialCzas;
        perkButtonsMap["goral_ultimateTime"] = perkButton_GoralUltimateCzas;
        perkButtonsMap["goral_specialCD"] = perkButton_GoralSpecialCD;
        perkButtonsMap["seraphim_swiatlo"] = perkButton_SeraphimSwiatlo;
        perkButtonsMap["seraphim_uzdrowienie"] = perkButton_SeraphimUzdrowienie;
        perkButtonsMap["seraphim_aniol"] = perkButton_SeraphimAniol;
        perkButtonsMap["seraphim_pierce"] = perkButton_SeraphimPrzebicie;
        perkButtonsMap["shepherd_owca"] = perkButton_ShepherdOwca;
        perkButtonsMap["shepherd_stado"] = perkButton_ShepherdStado;
        perkButtonsMap["shepherd_pasterz"] = perkButton_ShepherdPasterz;
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

        // ====== GÓRAL ======
        bool isGoral = FindFirstObjectByType<AbilitiesMountainMan>() != null;
        if (isGoral)
        {
            allPerks.Add(new PerkData("goral_mocnyCios", "Mocny cios", "GoralIkonaMocnyCios", () => {
                AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
                if (mountain != null) { mountain.attackDamage += 15f; mountain.attackRate += 0.3f; }
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
                if (playerHealth != null) { playerHealth.AddMaxHealth(30); playerHealth.AddArmor(5); }
                UpdatePerkStatsUI();
            }, () => {
                float hp = playerHealth?.maxHealth ?? 100f;
                return "+30 HP, +5 pancerza (obecnie: " + hp.ToString("F0") + " HP)";
            }, maxLevel: 3));

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

        // ====== SERAPHIM ======
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

        // ====== PASTERZ ======
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
    // POMOCNICZE
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

    void UpdatePerkStatsUI()
    {
        if (perkStatsText == null) return;
        string stats = "";
        if (currentDamageBonus > 0) stats += "Obrazenia: +" + currentDamageBonus + "\n";
        if (currentHealthBonus > 0) stats += "Zdrowie: +" + currentHealthBonus + "\n";
        if (currentAttackSpeedBonus > 0) stats += "Szybkosc ataku: -" + currentAttackSpeedBonus.ToString("F2") + "s\n";
        if (currentSpeedBonus > 0) stats += "Predkosc: +" + (currentSpeedBonus * 100).ToString("F0") + "%\n";
        if (currentXpBonus > 0) stats += "XP: +" + currentXpBonus + "\n";
        if (currentArmorBonus > 0) stats += "Pancerz: +" + currentArmorBonus + "\n";
        if (currentRangeBonus > 0) stats += "Zasieg: +" + currentRangeBonus.ToString("F1") + "\n";
        if (string.IsNullOrEmpty(stats)) stats = "Brak ulepszen";
        perkStatsText.text = stats;
    }

    int GetPerkLevel(string id)
    {
        if (perkLevels.ContainsKey(id)) return perkLevels[id];
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
            if (GetPerkLevel(temp[i].id) >= temp[i].maxLevel) temp.RemoveAt(i);
        }

        if (temp.Count == 0) temp = new List<PerkData>(allPerks);

        int perksToShow = Mathf.Min(4, temp.Count);
        while (currentPerks.Count < perksToShow && temp.Count > 0)
        {
            int idx = Random.Range(0, temp.Count);
            currentPerks.Add(temp[idx]);
            temp.RemoveAt(idx);
        }

        // ============================================================
        // WKLEJ PRZYCISKI DO SLOTÓW I USTAW OPIS
        // ============================================================
        for (int i = 0; i < perkSlots.Length && i < currentPerks.Count; i++)
        {
            Transform slot = perkSlots[i];
            if (slot == null) continue;

            // Usuń stare dzieci
            foreach (Transform child in slot) Destroy(child.gameObject);

            PerkData perk = currentPerks[i];
            GameObject sourceButton = perkButtonsMap.ContainsKey(perk.id) ? perkButtonsMap[perk.id] : null;

            if (sourceButton == null)
            {
                Debug.LogWarning("Brak przycisku dla perka: " + perk.id);
                continue;
            }

            // Skopiuj przycisk
            GameObject newButton = Instantiate(sourceButton, slot);
            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.localScale = Vector3.one;
            newButton.transform.localRotation = Quaternion.identity;

            // ============================================================
            // USTAW OPIS W SKOPIOWANYM PRZYCISKU (szukamy "opis")
            // ============================================================
            TextMeshProUGUI descTxt = newButton.transform.Find("opis")?.GetComponent<TextMeshProUGUI>();

            if (descTxt != null)
            {
                descTxt.text = perk.getDescription?.Invoke() ?? perk.description;
                Debug.Log("Ustawiono opis w " + newButton.name + ": " + descTxt.text);
            }
            else
            {
                Debug.LogWarning("Brak dziecka 'opis' w przycisku: " + newButton.name);
            }

            // Przypisz kliknięcie
            Button btn = newButton.GetComponent<Button>();
            if (btn != null)
            {
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ChoosePerk(index));
            }
        }

        if (perkPanel != null) perkPanel.SetActive(true);

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
        if (perkLevels.ContainsKey(id)) perkLevels[id]++;
        else perkLevels[id] = 1;

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
            waveSpawner.StartNextWave();
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

            if (playerHealth != null) playerHealth.LevelUpHealth();

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

        if (playerUI != null) playerUI.UpdateUI();
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