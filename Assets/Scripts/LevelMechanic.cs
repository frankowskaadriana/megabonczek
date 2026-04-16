using UnityEngine;
using TMPro;

public class LevelMechanic : MonoBehaviour
{
    [Header("Level Settings")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int expToNextLevel = 100;

    [Header("Experience Settings")]
    public float expMultiplier = 1.5f;

    [Header("Upgrade Levels")]
    public int damageUpgradeLevel = 0;
    public int healthUpgradeLevel = 0;
    public int rangeUpgradeLevel = 0;

    [Header("Upgrade Values")]
    public float damageMultiplier = 1f;
    public float healthMultiplier = 1f;
    public float rangeMultiplier = 1f;

    [Header("Upgrade Increments")]
    public float damageIncreasePerLevel = 0.2f;
    public float healthIncreasePerLevel = 0.25f;
    public float rangeIncreasePerLevel = 0.15f;

    [Header("UI")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;
    public GameObject levelUpPanel;

    [Header("Keybindings")]
    public KeyCode damageKey = KeyCode.Alpha1;
    public KeyCode healthKey = KeyCode.Alpha2;
    public KeyCode rangeKey = KeyCode.Alpha3;

    [Header("Upgrade Info Texts")]
    public TextMeshProUGUI damageUpgradeInfo;
    public TextMeshProUGUI healthUpgradeInfo;
    public TextMeshProUGUI rangeUpgradeInfo;

    [Header("Player References")]
    public PlayerHealth playerHealth;
    public AbilitiesMountainMan abilities;

    private bool isLevelingUp = false;
    private bool isPanelActive = false;

    void Start()
    {
        // ZnajdŸ referencje jeœli nie podpiête
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (abilities == null)
            abilities = GetComponent<AbilitiesMountainMan>();

        // Aktualizuj mno¿niki
        if (abilities != null)
        {
            abilities.damageMultiplier = damageMultiplier;
            abilities.rangeMultiplier = rangeMultiplier;
            abilities.UpdateDamage();
            abilities.UpdateRange();
        }

        if (playerHealth != null)
        {
            playerHealth.healthMultiplier = healthMultiplier;
            playerHealth.UpdateMaxHealth();
        }

        UpdateUI();

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        UpdateUpgradeInfo();

        Debug.Log("LevelMechanic zainicjalizowany!");
    }

    void Update()
    {
        if (isPanelActive)
        {
            if (Input.GetKeyDown(damageKey))
                UpgradeDamage();
            else if (Input.GetKeyDown(healthKey))
                UpgradeHealth();
            else if (Input.GetKeyDown(rangeKey))
                UpgradeRange();
        }
    }

    public void AddExp(int amount)
    {
        if (isLevelingUp) return;

        currentExp += amount;
        Debug.Log($"Zdobyto {amount} EXP! £¹cznie: {currentExp}/{expToNextLevel}");

        UpdateUI();

        while (currentExp >= expToNextLevel && !isLevelingUp)
        {
            currentExp -= expToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        isLevelingUp = true;
        currentLevel++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expMultiplier);

        Debug.Log($"LEVEL UP! Teraz poziom: {currentLevel}");

        UpdateUI();

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
            Time.timeScale = 0f;
            isPanelActive = true;
            Debug.Log("Panel ulepszeñ aktywny. Naciœnij 1, 2 lub 3!");
        }

        isLevelingUp = false;
    }

    void UpgradeDamage()
    {
        damageUpgradeLevel++;
        damageMultiplier = 1f + (damageUpgradeLevel * damageIncreasePerLevel);

        if (abilities != null)
        {
            abilities.damageMultiplier = damageMultiplier;
            abilities.UpdateDamage();
        }

        Debug.Log($"Obra¿enia ulepszone! Poziom: {damageUpgradeLevel}, Mno¿nik: {damageMultiplier}x");
        UpdateUpgradeInfo();
        CloseLevelUpPanel();
    }

    void UpgradeHealth()
    {
        healthUpgradeLevel++;
        healthMultiplier = 1f + (healthUpgradeLevel * healthIncreasePerLevel);

        if (playerHealth != null)
        {
            playerHealth.healthMultiplier = healthMultiplier;
            playerHealth.UpdateMaxHealth();
        }

        Debug.Log($"Zdrowie ulepszone! Poziom: {healthUpgradeLevel}, Mno¿nik: {healthMultiplier}x");
        UpdateUpgradeInfo();
        CloseLevelUpPanel();
    }

    void UpgradeRange()
    {
        rangeUpgradeLevel++;
        rangeMultiplier = 1f + (rangeUpgradeLevel * rangeIncreasePerLevel);

        if (abilities != null)
        {
            abilities.rangeMultiplier = rangeMultiplier;
            abilities.UpdateRange();
        }

        Debug.Log($"Zasiêg ulepszony! Poziom: {rangeUpgradeLevel}, Mno¿nik: {rangeMultiplier}x");
        UpdateUpgradeInfo();
        CloseLevelUpPanel();
    }

    void CloseLevelUpPanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
            Time.timeScale = 1f;
            isPanelActive = false;
        }
    }

    void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Poziom: {currentLevel}";

        if (expText != null)
            expText.text = $"EXP: {currentExp}/{expToNextLevel}";
    }

    void UpdateUpgradeInfo()
    {
        if (damageUpgradeInfo != null)
            damageUpgradeInfo.text = $"Obra¿enia [1]\nPoziom: {damageUpgradeLevel}\nMno¿nik: {damageMultiplier:F1}x";

        if (healthUpgradeInfo != null)
            healthUpgradeInfo.text = $"Zdrowie [2]\nPoziom: {healthUpgradeLevel}\nMno¿nik: {healthMultiplier:F1}x";

        if (rangeUpgradeInfo != null)
            rangeUpgradeInfo.text = $"Zasiêg [3]\nPoziom: {rangeUpgradeLevel}\nMno¿nik: {rangeMultiplier:F1}x";
    }
}