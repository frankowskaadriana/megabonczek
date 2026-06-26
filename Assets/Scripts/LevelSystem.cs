using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [Header("Perki")]
    public GameObject perkPanel;
    public Button[] perkButtons;
    public TextMeshProUGUI[] perkTexts;
    public TextMeshProUGUI[] perkDescriptions;

    [Header("Referencje")]
    public WaveSpawner waveSpawner;

    private PlayerHealth playerHealth;
    private bool isChoosingPerk = false;
    private float targetXpFill = 0f;
    private float currentXpFill = 0f;
    private float targetHealthFill = 1f;
    private float currentHealthFill = 1f;
    private const float SMOOTH_SPEED = 5f;
    private List<Perk> allPerks = new List<Perk>();
    private List<Perk> currentPerks = new List<Perk>();
    private bool gameStarted = false;

    private class Perk
    {
        public string name, description;
        public System.Action apply;
        public Perk(string n, string d, System.Action a) { name = n; description = d; apply = a; }
    }

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) playerHealth = player.GetComponent<PlayerHealth>();

        if (waveSpawner == null) waveSpawner = FindFirstObjectByType<WaveSpawner>();

        CreatePerks();
        if (perkPanel != null) perkPanel.SetActive(false);

        for (int i = 0; i < perkButtons.Length && i < perkTexts.Length; i++)
        {
            int index = i;
            perkButtons[i].onClick.AddListener(() => ChoosePerk(index));
        }

        UpdateUI();
    }

    void Update()
    {
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
        }

        UpdateUI();
    }

    void CreatePerks()
    {
        allPerks.Clear();
        allPerks.Add(new Perk("⚔️ Obrażenia", "+10 dmg", () => { /* logika */ }));
        allPerks.Add(new Perk("❤️ Zdrowie", "+20 HP", () => { if (playerHealth != null) playerHealth.AddMaxHealth(20); }));
        allPerks.Add(new Perk("⚡ Szybkość", "Szybszy atak", () => { /* logika */ }));
        allPerks.Add(new Perk("👟 Prędkość", "+10% ruchu", () => { /* logika */ }));
        allPerks.Add(new Perk("📚 XP", "+1 XP", () => { xpPerEnemy++; }));
        allPerks.Add(new Perk("🛡️ Pancerz", "+10 pancerza", () => { if (playerHealth != null) playerHealth.AddArmor(10); }));
    }

    void ShowPerkSelection()
    {
        isChoosingPerk = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentPerks.Clear();
        List<Perk> temp = new List<Perk>(allPerks);

        while (currentPerks.Count < 3 && temp.Count > 0)
        {
            int idx = Random.Range(0, temp.Count);
            currentPerks.Add(temp[idx]);
            temp.RemoveAt(idx);
        }

        for (int i = 0; i < currentPerks.Count && i < perkTexts.Length; i++)
        {
            if (perkTexts[i] != null) perkTexts[i].text = currentPerks[i].name;
            if (perkDescriptions[i] != null) perkDescriptions[i].text = currentPerks[i].description;
        }

        if (perkPanel != null) perkPanel.SetActive(true);
    }

    void ChoosePerk(int index)
    {
        if (!isChoosingPerk || index >= currentPerks.Count) return;

        currentPerks[index].apply();
        isChoosingPerk = false;
        if (perkPanel != null) perkPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    public void EnemyDied()
    {
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

            ShowPerkSelection();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        gameStarted = true;
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
}