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

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public Image xpFill;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public Image healthFill;
    public TextMeshProUGUI healthText;

    [Header("Panel Wyboru Perków")]
    public GameObject perkPanel;
    public Button[] perkButtons;
    public TextMeshProUGUI[] perkNameTexts;
    public TextMeshProUGUI[] perkDescTexts;
    public TextMeshProUGUI[] perkLevelTexts;
    public Image[] perkIcons;

    [Header("Komunikaty")]
    public TextMeshProUGUI levelUpMessage;
    public float messageDuration = 2f;

    [Header("Referencje")]
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

        if (perkPanel != null)
        {
            perkPanel.SetActive(false);
            Debug.Log("🔒 Panel perków ukryty");
        }

        if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < perkButtons.Length && i < 4; i++)
        {
            int index = i;
            perkButtons[i].onClick.AddListener(() => ChoosePerk(index));
        }

        isChoosingPerk = false;

        UpdateUI();
        Debug.Log("✅ LevelSystem gotowy! Gra odmrożona.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

    void CreatePerks()
    {
        allPerks.Clear();

        allPerks.Add(new Perk("damage", "⚔️ Obrażenia", "+10 obrażeń", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackDamage += 10f;

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackDamage += 10f;

            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackDamage += 10f;

            Debug.Log($"⚔️ Obrażenia zwiększone! (Poziom {GetPerkLevel("damage") + 1}/5)");
        }, maxLevel: 5));

        allPerks.Add(new Perk("health", "❤️ Zdrowie", "+25 maksymalnego HP", () => {
            if (playerHealth != null)
            {
                playerHealth.AddMaxHealth(25);
                Debug.Log($"❤️ Zdrowie zwiększone! (Poziom {GetPerkLevel("health") + 1}/5)");
            }
        }, maxLevel: 5));

        allPerks.Add(new Perk("attackSpeed", "⚡ Szybkość ataku", "-0.15s cooldown", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRate = Mathf.Max(0.2f, mountain.attackRate - 0.15f);

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackRate = Mathf.Max(0.2f, seraphim.attackRate - 0.15f);

            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackRate = Mathf.Max(0.2f, shepherd.attackRate - 0.15f);

            Debug.Log($"⚡ Szybkość ataku zwiększona! (Poziom {GetPerkLevel("attackSpeed") + 1}/5)");
        }, maxLevel: 5));

        allPerks.Add(new Perk("speed", "👟 Szybkie nogi", "+8% prędkości ruchu", () => {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null)
            {
                movement.maxSpeed *= 1.08f;
                Debug.Log($"👟 Prędkość zwiększona do {movement.maxSpeed:F1}! (Poziom {GetPerkLevel("speed") + 1}/5)");
            }
        }, maxLevel: 5));

        allPerks.Add(new Perk("xp", "📚 Więcej XP", "+1 XP za wroga", () => {
            xpPerEnemy++;
            Debug.Log($"📚 XP za wroga: {xpPerEnemy} (Poziom {GetPerkLevel("xp") + 1}/5)");
        }, maxLevel: 5));

        allPerks.Add(new Perk("armor", "🛡️ Pancerz", "+10 pancerza", () => {
            if (playerHealth != null)
            {
                playerHealth.AddArmor(10);
                Debug.Log($"🛡️ Pancerz: {playerHealth.armor} (Poziom {GetPerkLevel("armor") + 1}/5)");
            }
        }, maxLevel: 5));

        allPerks.Add(new Perk("range", "🏹 Większy zasięg", "+0.3m zasięgu", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRange += 0.3f;

            AbilitiesSeraphim seraphim = FindFirstObjectByType<AbilitiesSeraphim>();
            if (seraphim != null) seraphim.attackRange += 0.3f;

            ShepherdAbilities shepherd = FindFirstObjectByType<ShepherdAbilities>();
            if (shepherd != null) shepherd.attackRange += 0.3f;

            Debug.Log($"🏹 Zasięg zwiększony! (Poziom {GetPerkLevel("range") + 1}/5)");
        }, maxLevel: 5));

        allPerks.Add(new Perk("pushback", "💥 Silny odrzut", "+25% siły odrzutu", () => {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null) ph.pushbackForce *= 1.25f;

            Debug.Log($"💥 Odrzut zwiększony! (Poziom {GetPerkLevel("pushback") + 1}/5)");
        }, maxLevel: 5));
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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentPerks.Clear();
        List<Perk> temp = new List<Perk>(allPerks);

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

        for (int i = 0; i < perkButtons.Length && i < 4; i++)
        {
            if (i < currentPerks.Count)
            {
                perkButtons[i].gameObject.SetActive(true);
                if (perkNameTexts[i] != null) perkNameTexts[i].text = currentPerks[i].name;
                if (perkDescTexts[i] != null) perkDescTexts[i].text = currentPerks[i].description;
                if (perkLevelTexts[i] != null)
                    perkLevelTexts[i].text = $"Poziom {GetPerkLevel(currentPerks[i].id) + 1}/{currentPerks[i].maxLevel}";
                if (perkIcons[i] != null && currentPerks[i].icon != null)
                    perkIcons[i].sprite = currentPerks[i].icon;
            }
            else
            {
                perkButtons[i].gameObject.SetActive(false);
            }
        }

        if (perkPanel != null) perkPanel.SetActive(true);

        ShowLevelUpMessage("🎉 AWANS! Wybierz perka (1-4)");
        AudioManager.Instance?.PlayPerkSelect(); // DŹWIĘK WYBORU PERKA

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
        if (perkPanel != null) perkPanel.SetActive(false);
        if (levelUpMessage != null) levelUpMessage.gameObject.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AudioManager.Instance?.PlayPerkSelect(); // DŹWIĘK WYBORU PERKA

        if (waveSpawner != null && !waveSpawner.IsWaveActive())
        {
            StartCoroutine(ResumeWaveAfterPerk());
        }
    }

    IEnumerator ResumeWaveAfterPerk()
    {
        yield return new WaitForSeconds(0.5f);
        if (waveSpawner != null && !waveSpawner.IsWaveActive())
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

            AudioManager.Instance?.PlayLevelUp(); // DŹWIĘK AWANSU
            ShowPerkSelection();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("🎮 Gra rozpoczęta!");
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
}