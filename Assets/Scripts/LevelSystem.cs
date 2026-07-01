using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelSystem : MonoBehaviour
{
    [Header("═══════════════ POZIOM I XP ═══════════════")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpRequired = 10;
    public int xpPerEnemy = 1;

    [Header("═══════════════ STATYSTYKI PRZECIWNIKÓW ═══════════════")]
    public float baseEnemyHealth = 30f;
    public float healthIncreasePerLevel = 5f;
    public float healthIncreasePerWave = 2f;
    public int baseExpReward = 10;
    public int expIncreasePerLevel = 1;
    public int expIncreasePerWave = 1;

    [Header("═══════════════ CZAS GRY ═══════════════")]
    public float gameTime = 0f;
    public float difficultyMultiplier = 1f;

    [Header("═══════════════ UI ═══════════════")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Image xpFill;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    [Header("═══════════════ PANEL WYBORU PERKÓW ═══════════════")]
    public GameObject perkPanel;
    public Button[] perkButtons;
    public TextMeshProUGUI[] perkNameTexts;
    public TextMeshProUGUI[] perkDescTexts;
    public TextMeshProUGUI[] perkLevelTexts;
    public Image[] perkIcons;

    [Header("═══════════════ WYŚWIETLANIE STATYSTYK PERKÓW ═══════════════")]
    public TextMeshProUGUI perkStatsText;

    [Header("═══════════════ KOMUNIKATY ═══════════════")]
    public TextMeshProUGUI levelUpMessage;
    public float messageDuration = 2f;

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public WaveSpawner waveSpawner;

    private PlayerHealth playerHealth;
    private bool isChoosingPerk = false;
    private float targetXpFill = 0f;
    private float currentXpFill = 0f;
    private float targetHealthFill = 1f;
    private float currentHealthFill = 1f;
    private const float SMOOTH_SPEED = 5f;
    private bool gameStarted = false;
    private float messageTimer = 0f;
    private bool showMessage = false;

    private List<Perk> allPerks = new List<Perk>();
    private List<Perk> currentPerks = new List<Perk>();
    private Dictionary<string, int> perkLevels = new Dictionary<string, int>();

    // === STATYSTYKI GRACZA ===
    private int currentDamageBonus = 0;
    private int currentHealthBonus = 0;
    private float currentAttackSpeedBonus = 0f;
    private float currentSpeedBonus = 0f;
    private int currentXpBonus = 0;
    private int currentArmorBonus = 0;

    private float currentEnemyHealth;
    private int currentExpReward;

    [System.Serializable]
    public class Perk
    {
        public string id;
        public string name;
        public string description;
        public int maxLevel = 5;
        public System.Action apply;
        public Sprite icon;

        public Perk(string id, string name, string description, System.Action apply, Sprite icon = null, int maxLevel = 5)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.apply = apply;
            this.icon = icon;
            this.maxLevel = maxLevel;
        }
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (waveSpawner == null) waveSpawner = FindFirstObjectByType<WaveSpawner>();

        CreatePerks();

        // === UKRYJ PANEL PERKÓW ===
        if (perkPanel != null) perkPanel.SetActive(false);
        if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);

        Time.timeScale = 1f;

        // === PRZYPISZ PRZYCISKI ===
        for (int i = 0; i < perkButtons.Length && i < 4; i++)
        {
            int index = i;
            if (perkButtons[i] != null)
            {
                perkButtons[i].onClick.RemoveAllListeners();
                perkButtons[i].onClick.AddListener(() => ChoosePerk(index));
                Debug.Log($"✅ Przypisano przycisk {i + 1}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Przycisk {i + 1} nie jest przypisany!");
            }
        }

        isChoosingPerk = false;

        UpdateEnemyStats();
        UpdateUI();
        UpdatePerkStatsUI();
        Debug.Log("✅ LevelSystem gotowy!");
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
            Debug.Log("🔓 AWARYJNE ODBLOKOWANIE GRY!");
        }

        if (!gameStarted) return;

        if (playerHealth == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        }

        if (xpFill != null)
        {
            currentXpFill = Mathf.Lerp(currentXpFill, targetXpFill, Time.deltaTime * SMOOTH_SPEED);
            xpFill.fillAmount = currentXpFill;
        }

        if (healthFill != null && playerHealth != null)
        {
            currentHealthFill = Mathf.Lerp(currentHealthFill, targetHealthFill, Time.deltaTime * SMOOTH_SPEED);
            healthFill.fillAmount = currentHealthFill;

            float hp = playerHealth.currentHealth / playerHealth.maxHealth;
            healthFill.color = hp > 0.6f ? Color.green : (hp > 0.3f ? Color.yellow : Color.red);
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

    // ============================================
    // PERKI
    // ============================================

    void CreatePerks()
    {
        allPerks.Clear();

        allPerks.Add(new Perk("damage", "⚔️ Obrażenia", "+10 obrażeń", () => {
            currentDamageBonus += 10;
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackDamage += 10f;

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackDamage += 10f;

            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackDamage += 10f;

            Debug.Log($"⚔️ Obrażenia zwiększone! (+{currentDamageBonus})");
            UpdatePerkStatsUI();
        }, maxLevel: 5));

        allPerks.Add(new Perk("health", "❤️ Zdrowie", "+25 maksymalnego HP", () => {
            currentHealthBonus += 25;
            if (playerHealth != null)
            {
                playerHealth.AddMaxHealth(25);
                Debug.Log($"❤️ Zdrowie zwiększone! (+{currentHealthBonus})");
            }
            UpdatePerkStatsUI();
        }, maxLevel: 5));

        allPerks.Add(new Perk("attackSpeed", "⚡ Szybkość ataku", "-0.15s cooldown", () => {
            currentAttackSpeedBonus += 0.15f;
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRate = Mathf.Max(0.2f, mountain.attackRate - 0.15f);

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackRate = Mathf.Max(0.2f, seraphim.attackRate - 0.15f);

            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackRate = Mathf.Max(0.2f, shepherd.attackRate - 0.15f);

            Debug.Log($"⚡ Szybkość ataku zwiększona! (-{currentAttackSpeedBonus:F2}s)");
            UpdatePerkStatsUI();
        }, maxLevel: 5));

        allPerks.Add(new Perk("speed", "👟 Szybkie nogi", "+8% prędkości ruchu", () => {
            currentSpeedBonus += 0.08f;
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null)
            {
                movement.maxSpeed *= 1.08f;
                Debug.Log($"👟 Prędkość zwiększona! (+{currentSpeedBonus * 100:F0}%)");
            }
            UpdatePerkStatsUI();
        }, maxLevel: 5));

        allPerks.Add(new Perk("xp", "📚 Więcej XP", "+1 XP za wroga", () => {
            currentXpBonus += 1;
            xpPerEnemy++;
            Debug.Log($"📚 XP za wroga: {xpPerEnemy} (+{currentXpBonus})");
            UpdatePerkStatsUI();
        }, maxLevel: 5));

        allPerks.Add(new Perk("armor", "🛡️ Pancerz", "+10 pancerza", () => {
            currentArmorBonus += 10;
            if (playerHealth != null)
            {
                playerHealth.AddArmor(10);
                Debug.Log($"🛡️ Pancerz: {playerHealth.armor} (+{currentArmorBonus})");
            }
            UpdatePerkStatsUI();
        }, maxLevel: 5));
    }

    void UpdatePerkStatsUI()
    {
        if (perkStatsText == null) return;

        string stats = "";

        if (currentDamageBonus > 0)
            stats += $"⚔️ Obrażenia: +{currentDamageBonus}\n";

        if (currentHealthBonus > 0)
            stats += $"❤️ Zdrowie: +{currentHealthBonus}\n";

        if (currentAttackSpeedBonus > 0)
            stats += $"⚡ Szybkość ataku: -{currentAttackSpeedBonus:F2}s\n";

        if (currentSpeedBonus > 0)
            stats += $"👟 Prędkość: +{currentSpeedBonus * 100:F0}%\n";

        if (currentXpBonus > 0)
            stats += $"📚 XP: +{currentXpBonus}\n";

        if (currentArmorBonus > 0)
            stats += $"🛡️ Pancerz: +{currentArmorBonus}\n";

        if (string.IsNullOrEmpty(stats))
        {
            stats = "Brak ulepszeń";
        }

        perkStatsText.text = stats;
    }

    int GetPerkLevel(string id)
    {
        if (perkLevels.ContainsKey(id))
            return perkLevels[id];
        return 0;
    }

    void ShowPerkSelection()
    {
        if (isChoosingPerk) return;

        isChoosingPerk = true;
        Time.timeScale = 0f;

        currentPerks.Clear();
        List<Perk> temp = new List<Perk>(allPerks);

        // Usuń perki które osiągnęły max level
        for (int i = temp.Count - 1; i >= 0; i--)
        {
            if (GetPerkLevel(temp[i].id) >= temp[i].maxLevel)
            {
                temp.RemoveAt(i);
            }
        }

        if (temp.Count == 0)
        {
            temp = new List<Perk>(allPerks);
        }

        int perksToShow = Mathf.Min(4, temp.Count);
        while (currentPerks.Count < perksToShow && temp.Count > 0)
        {
            int idx = Random.Range(0, temp.Count);
            currentPerks.Add(temp[idx]);
            temp.RemoveAt(idx);
        }

        // === WYŚWIETL PERKI W UI ===
        for (int i = 0; i < perkButtons.Length && i < 4; i++)
        {
            if (i < currentPerks.Count && perkButtons[i] != null)
            {
                perkButtons[i].gameObject.SetActive(true);

                if (perkNameTexts[i] != null)
                {
                    perkNameTexts[i].text = currentPerks[i].name;
                    perkNameTexts[i].gameObject.SetActive(true);
                    Debug.Log($"📝 Perk {i + 1} nazwa: {currentPerks[i].name}");
                }

                if (perkDescTexts[i] != null)
                {
                    perkDescTexts[i].text = currentPerks[i].description;
                    perkDescTexts[i].gameObject.SetActive(true);
                    Debug.Log($"📝 Perk {i + 1} opis: {currentPerks[i].description}");
                }

                if (perkLevelTexts[i] != null)
                {
                    perkLevelTexts[i].text = $"Poziom {GetPerkLevel(currentPerks[i].id) + 1}/{currentPerks[i].maxLevel}";
                    perkLevelTexts[i].gameObject.SetActive(true);
                }

                if (perkIcons[i] != null && currentPerks[i].icon != null)
                {
                    perkIcons[i].sprite = currentPerks[i].icon;
                    perkIcons[i].gameObject.SetActive(true);
                }
            }
            else if (perkButtons[i] != null)
            {
                perkButtons[i].gameObject.SetActive(false);
                if (perkNameTexts[i] != null) perkNameTexts[i].gameObject.SetActive(false);
                if (perkDescTexts[i] != null) perkDescTexts[i].gameObject.SetActive(false);
                if (perkLevelTexts[i] != null) perkLevelTexts[i].gameObject.SetActive(false);
                if (perkIcons[i] != null) perkIcons[i].gameObject.SetActive(false);
            }
        }

        // === POKAŻ PANEL ===
        if (perkPanel != null)
        {
            perkPanel.SetActive(true);
            Debug.Log("📦 Panel perków widoczny");
        }
        else
        {
            Debug.LogError("❌ Perk Panel nie jest przypisany!");
        }

        ShowLevelUpMessage("🎉 AWANS! Wybierz perka (1-4)");
        AudioManager.Instance?.PlayPerkSelect();

        Debug.Log("=== WYBIERZ PERKA (1-4) ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log($"{i + 1}. {currentPerks[i].name} - {currentPerks[i].description}");
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

        Debug.Log($"✅ WYBRANO: {currentPerks[index].name} (Poziom {perkLevels[id]}/{currentPerks[index].maxLevel})");

        isChoosingPerk = false;

        // === UKRYJ PANEL ===
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

    // ============================================
    // STATYSTYKI PRZECIWNIKÓW
    // ============================================

    public void UpdateEnemyStats()
    {
        currentEnemyHealth = baseEnemyHealth + (currentLevel * healthIncreasePerLevel) + (waveSpawner != null ? waveSpawner.GetCurrentWave() * healthIncreasePerWave : 0);
        currentEnemyHealth = Mathf.Min(currentEnemyHealth, 500f);

        currentExpReward = baseExpReward + (currentLevel * expIncreasePerLevel) + (waveSpawner != null ? waveSpawner.GetCurrentWave() * expIncreasePerWave : 0);
        currentExpReward = Mathf.Min(currentExpReward, 100);

        Debug.Log($"📊 Statystyki wrogów: HP={currentEnemyHealth:F1}, EXP={currentExpReward}");
    }

    public float GetEnemyHealth()
    {
        return currentEnemyHealth;
    }

    public int GetEnemyExpReward()
    {
        return currentExpReward;
    }

    // ============================================
    // METODY PUBLICZNE
    // ============================================

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
                targetHealthFill = playerHealth.currentHealth / playerHealth.maxHealth;
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
        Debug.Log("🎮 Gra rozpoczęta!");
    }

    public float GetGameTime()
    {
        return gameTime;
    }

    public float GetDifficultyMultiplier()
    {
        return difficultyMultiplier;
    }

    public void UpdateUI()
    {
        if (playerHealth == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        }

        if (levelText != null) levelText.text = $"Poziom: {currentLevel}";
        if (xpText != null) xpText.text = $"{currentXP} / {xpRequired}";
        if (xpFill != null) targetXpFill = (float)currentXP / xpRequired;

        if (playerHealth != null)
        {
            targetHealthFill = playerHealth.currentHealth / playerHealth.maxHealth;
            if (healthText != null) healthText.text = $"{Mathf.Round(playerHealth.currentHealth)} / {Mathf.Round(playerHealth.maxHealth)}";
        }

        if (waveSpawner != null)
        {
            if (waveText != null) waveText.text = $"Fala: {waveSpawner.GetCurrentWave()}";
            if (enemiesLeftText != null) enemiesLeftText.text = $"Wrogowie: {waveSpawner.GetEnemyCount()}";
        }
    }

    public bool IsChoosingPerk()
    {
        return isChoosingPerk;
    }

    // ============================================
    // GETTERY DLA STATYSTYK
    // ============================================

    public int GetDamageBonus() => currentDamageBonus;
    public int GetHealthBonus() => currentHealthBonus;
    public float GetAttackSpeedBonus() => currentAttackSpeedBonus;
    public float GetSpeedBonus() => currentSpeedBonus;
    public int GetXpBonus() => currentXpBonus;
    public int GetArmorBonus() => currentArmorBonus;
}