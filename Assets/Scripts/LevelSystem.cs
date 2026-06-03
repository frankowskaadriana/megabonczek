using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LevelSystem : MonoBehaviour
{
    [Header("═══════════════ LEVEL SETTINGS ═══════════════")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpRequired = 10;

    [Header("═══════════════ CZAS GRY (dla eventów) ═══════════════")]
    public float gameTime = 0f;  // DODANE - potrzebne dla EventTriggerVolume

    [Header("═══════════════ UI REFERENCES ═══════════════")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;

    [Header("═══════════════ PERK SELECTION UI ═══════════════")]
    public GameObject perkSelectionPanel;
    public Button perkButton1;
    public Button perkButton2;
    public Button perkButton3;
    public TextMeshProUGUI perkText1;
    public TextMeshProUGUI perkText2;
    public TextMeshProUGUI perkText3;
    public TextMeshProUGUI perkDescription1;
    public TextMeshProUGUI perkDescription2;
    public TextMeshProUGUI perkDescription3;

    [Header("═══════════════ PLAYER REFERENCES (dla CharacterSelector) ═══════════════")]
    public AbilitiesMountainMan mountainManAbilities;  // DODANE
    public SeraphimAbilities seraphimAbilities;        // DODANE
    public ShepherdAbilities shepherdAbilities;        // DODANE
    public PlayerHealth playerHealth;                  // DODANE

    [Header("═══════════════ REFERENCES ═══════════════")]
    public WeaponUpgradeSystem weaponUpgrade;
    public WaveSpawner waveSpawner;

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
        CreatePerksList();

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(false);

        if (perkButton1 != null)
            perkButton1.onClick.AddListener(() => SelectPerk(0));
        if (perkButton2 != null)
            perkButton2.onClick.AddListener(() => SelectPerk(1));
        if (perkButton3 != null)
            perkButton3.onClick.AddListener(() => SelectPerk(2));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateUI();
        Debug.Log("LevelSystem gotowy!");
    }

    void Update()
    {
        if (!gameStarted) return;

        // Aktualizuj czas gry
        gameTime += Time.deltaTime;

        if (isPerkSelectionActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectPerk(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectPerk(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectPerk(2);
        }

        UpdateUI();
    }

    void CreatePerksList()
    {
        availablePerks.Clear();

        availablePerks.Add(new Perk("⚔️ Obrażenia +10", "+10 do obrażeń", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeDamage();
        }));

        availablePerks.Add(new Perk("📏 Zasięg +0.2m", "Zwiększa zasięg ataku", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeRange();
        }));

        availablePerks.Add(new Perk("❤️ Zdrowie +20", "+20 maksymalnego zdrowia", () => {
            if (playerHealth != null) playerHealth.AddMaxHealth(20);
        }));

        availablePerks.Add(new Perk("🔄 Szybki cooldown", "-1s cooldown", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialCooldown();
        }));

        availablePerks.Add(new Perk("💥 Mocniejsze obrażenia", "+15 obrażeń zdolności", () => {
            if (weaponUpgrade != null) weaponUpgrade.UpgradeSpecialDamage();
        }));

        availablePerks.Add(new Perk("👟 Szybkie nogi", "+10% prędkości", () => {
            PlayerMovement movement = FindFirstObjectByType<PlayerMovement>();
            if (movement != null) movement.maxSpeed += 0.5f;
        }));

        availablePerks.Add(new Perk("🛡️ Pancerz +10", "+10 pancerza", () => {
            if (playerHealth != null) playerHealth.AddArmor(10);
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
        {
            perkText1.text = currentPerks[0].name;
            if (perkDescription1 != null)
                perkDescription1.text = currentPerks[0].description;
        }

        if (perkText2 != null && currentPerks.Count > 1)
        {
            perkText2.text = currentPerks[1].name;
            if (perkDescription2 != null)
                perkDescription2.text = currentPerks[1].description;
        }

        if (perkText3 != null && currentPerks.Count > 2)
        {
            perkText3.text = currentPerks[2].name;
            if (perkDescription3 != null)
                perkDescription3.text = currentPerks[2].description;
        }

        if (perkSelectionPanel != null)
            perkSelectionPanel.SetActive(true);

        Debug.Log("=== WYBIERZ PERK ===");
        for (int i = 0; i < currentPerks.Count; i++)
            Debug.Log($"{i + 1}. {currentPerks[i].name}");
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
        Debug.Log($"✅ WYBRANO PERK: {currentPerks[index].name}");

        isPerkSelectionActive = false;
        HidePerkPanel();

        Cursor.lockState = previousLockMode;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void EnemyDied()
    {
        if (!gameStarted) return;
        currentXP++;

        Debug.Log($"Zabity wróg! XP: {currentXP}/{xpRequired}");

        if (currentXP >= xpRequired)
        {
            currentXP -= xpRequired;
            xpRequired += 10;
            currentLevel++;

            if (playerHealth != null)
                playerHealth.LevelUpHealth();

            Debug.Log($"===== LEVEL UP! Poziom {currentLevel} =====");
            ShowPerkSelection();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            gameTime = 0f;
            Debug.Log("Gra rozpoczeta!");
        }
    }

    // DODANE - dla portalu
    public void OnPortalEnter()
    {
        Debug.Log("Gracz wszedł do portalu!");
        Time.timeScale = 0f;
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = "Poziom: " + currentLevel;
        if (waveSpawner != null && waveText != null)
            waveText.text = "Fala: " + waveSpawner.currentWave;
    }
}