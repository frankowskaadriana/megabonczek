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
    public float gameTime = 0f;

    [Header("═══════════════ UI REFERENCES ═══════════════")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI enemiesLeftText;
    public TextMeshProUGUI timerText;

    [Header("═══════════════ PERK SELECTION UI ═══════════════")]
    public GameObject perkPanel;
    public Button perkButton1;
    public Button perkButton2;
    public Button perkButton3;
    public TextMeshProUGUI perkText1;
    public TextMeshProUGUI perkText2;
    public TextMeshProUGUI perkText3;
    public TextMeshProUGUI perkDescription1;
    public TextMeshProUGUI perkDescription2;
    public TextMeshProUGUI perkDescription3;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public WaveSpawner waveSpawner;

    private bool gameStarted = false;
    private bool isChoosingPerk = false;
    private CursorLockMode previousCursorState;

    private List<Perk> allPerks = new List<Perk>();
    private List<Perk> currentPerks = new List<Perk>();

    [System.Serializable]
    public class Perk
    {
        public string name;
        public string description;
        public System.Action apply;

        public Perk(string name, string description, System.Action apply)
        {
            this.name = name;
            this.description = description;
            this.apply = apply;
        }
    }

    void Start()
    {
        CreatePerksList();

        if (perkPanel != null) perkPanel.SetActive(false);

        if (perkButton1 != null) perkButton1.onClick.AddListener(() => ChoosePerk(0));
        if (perkButton2 != null) perkButton2.onClick.AddListener(() => ChoosePerk(1));
        if (perkButton3 != null) perkButton3.onClick.AddListener(() => ChoosePerk(2));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateUI();
        Debug.Log("LevelSystem gotowy!");
    }

    void Update()
    {
        if (!gameStarted) return;

        gameTime += Time.deltaTime;
        UpdateTimerUI();

        if (isChoosingPerk)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ChoosePerk(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) ChoosePerk(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) ChoosePerk(2);
        }

        UpdateUI();
    }

    void CreatePerksList()
    {
        allPerks.Clear();

        allPerks.Add(new Perk("⚔️ Więcej obrażeń", "+10 do obrażeń", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));

        allPerks.Add(new Perk("❤️ Więcej zdrowia", "+20 maksymalnego zdrowia", () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(20);
        }));

        allPerks.Add(new Perk("⚡ Szybszy atak", "Szybsze ataki", () => {
            AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
            if (mountain != null) mountain.attackRate = Mathf.Max(0.4f, mountain.attackRate - 0.1f);
        }));

        allPerks.Add(new Perk("👟 Szybkie nogi", "+10% prędkości ruchu", () => {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null) movement.maxSpeed += 0.5f;
        }));

        allPerks.Add(new Perk("🔄 Szybszy cooldown", "-1s cooldown zdolności", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));

        allPerks.Add(new Perk("📚 Więcej XP", "+1 XP za wroga", () => {
            xpPerEnemy++;
        }));

        allPerks.Add(new Perk("🛡️ Więcej pancerza", "+10 pancerza", () => {
            if (playerHealth != null) playerHealth.AddArmor(10);
        }));
    }

    void ShowPerkSelection()
    {
        isChoosingPerk = true;
        Time.timeScale = 0f;

        previousCursorState = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentPerks.Clear();
        List<Perk> tempPerks = new List<Perk>(allPerks);

        while (currentPerks.Count < 3 && tempPerks.Count > 0)
        {
            int randomIndex = Random.Range(0, tempPerks.Count);
            currentPerks.Add(tempPerks[randomIndex]);
            tempPerks.RemoveAt(randomIndex);
        }

        if (perkText1 != null && currentPerks.Count > 0)
            perkText1.text = currentPerks[0].name;
        if (perkDescription1 != null && currentPerks.Count > 0)
            perkDescription1.text = currentPerks[0].description;

        if (perkText2 != null && currentPerks.Count > 1)
            perkText2.text = currentPerks[1].name;
        if (perkDescription2 != null && currentPerks.Count > 1)
            perkDescription2.text = currentPerks[1].description;

        if (perkText3 != null && currentPerks.Count > 2)
            perkText3.text = currentPerks[2].name;
        if (perkDescription3 != null && currentPerks.Count > 2)
            perkDescription3.text = currentPerks[2].description;

        if (perkPanel != null) perkPanel.SetActive(true);

        Debug.Log("=== WYBIERZ PERK ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log($"{i + 1}. {currentPerks[i].name}");
    }

    void ChoosePerk(int index)
    {
        if (!isChoosingPerk) return;
        if (index < 0 || index >= currentPerks.Count) return;

        currentPerks[index].apply();
        Debug.Log($"✅ WYBRANO: {currentPerks[index].name}");

        isChoosingPerk = false;
        if (perkPanel != null) perkPanel.SetActive(false);

        Cursor.lockState = previousCursorState;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void EnemyDied()
    {
        if (!gameStarted) return;

        currentXP += xpPerEnemy;

        if (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            xpRequired += 10;
            currentLevel++;

            if (playerHealth != null) playerHealth.LevelUpHealth();

            Debug.Log($"🎉 AWANS! Poziom {currentLevel} 🎉");
            ShowPerkSelection();
        }

        UpdateUI();
    }

    public void UpdateEnemiesLeft(int count)
    {
        if (enemiesLeftText != null)
            enemiesLeftText.text = $"Wrogowie: {count}";
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            gameTime = 0f;
            Debug.Log("Gra rozpoczęta!");
        }
    }

    public void OnPortalEnter()
    {
        Debug.Log("Gracz wszedł do portalu!");
        Time.timeScale = 0f;
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Poziom: {currentLevel}";

        if (xpText != null)
            xpText.text = $"XP: {currentXP}/{xpRequired}";

        if (waveSpawner != null && waveText != null)
            waveText.text = $"Fala: {waveSpawner.currentWave}";

        if (enemiesLeftText != null)
        {
            int enemyCount = 0;
            if (waveSpawner != null)
                enemyCount = waveSpawner.GetEnemyCount();
            enemiesLeftText.text = $"Wrogowie: {enemyCount}";
        }
    }
}