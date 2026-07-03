using UnityEngine;

public class WeaponUpgradeSystem : MonoBehaviour
{
    [Header("Statystyki Aktualne")]
    public float currentDamage = 50f;
    public float currentRange = 1.5f;
    public float currentSwingAngle = 90f;
    public float currentSpecialCooldown = 20f;
    public float currentSpecialDamage = 80f;
    public int currentSpecialRotations = 1;
    public bool hasBleed = false;
    public float bleedDuration = 2f;
    public float bleedDamage = 3f;
    public float currentUltimateDuration = 10f;
    public float currentUltimateRadius = 1.25f;
    public float currentUltimateDamage = 50f;

    // Seraphim
    public int currentProjectileCount = 1;
    public bool canPierce = false;
    public int pierceCount = 0;

    // Shepherd
    public float currentSheepDamage = 20f;
    public float currentSheepSpawnCooldown = 45f;
    public float currentFeastDamage = 350f;
    public float currentFeastRadius = 3f;
    public int currentRemainingSheep = 1;

    // ============================================================
    // NOWE STATYSTYKI DLA GÓRALA (CZAS UMIEJÊTNOŒCI)
    // ============================================================
    public float currentStompDuration = 0.5f;
    public float currentSpecialDuration = 0.5f;
    public float currentUltimateTime = 3f;
    public float currentSpecialCooldownReduction = 0f;

    private int damageLevel = 0;
    private int rangeLevel = 0;
    private int swingLevel = 0;
    private int specialCooldownLevel = 0;
    private int specialDamageLevel = 0;
    private int specialRotationsLevel = 0;
    private int bleedLevel = 0;
    private int ultimateDurationLevel = 0;
    private int ultimateRadiusLevel = 0;
    private int ultimateDamageLevel = 0;
    private int projectileCountLevel = 0;
    private int pierceLevel = 0;
    private int sheepDamageLevel = 0;
    private int sheepCooldownLevel = 0;
    private int feastDamageLevel = 0;
    private int feastRadiusLevel = 0;

    // ============================================================
    // NOWE POZIOMY DLA GÓRALA
    // ============================================================
    private int stompDurationLevel = 0;
    private int specialDurationLevel = 0;
    private int ultimateTimeLevel = 0;
    private int specialCooldownReductionLevel = 0;

    // ============================================================
    // GÓRAL - ISTNIEJ¥CE
    // ============================================================
    public void UpgradeDamage() { damageLevel++; currentDamage = 50f + (damageLevel * 10f); Debug.Log("Obrazenia: " + currentDamage); }
    public void UpgradeRange() { rangeLevel++; currentRange = Mathf.Min(1.5f + (rangeLevel * 0.1f), 2f); Debug.Log("Zasieg: " + currentRange + "m"); }
    public void UpgradeSwingAngle() { swingLevel++; currentSwingAngle = Mathf.Min(90f + (swingLevel * 10f), 150f); Debug.Log("Kat zamachu: " + currentSwingAngle + "°"); }
    public void UpgradeSpecialDamage() { specialDamageLevel++; currentSpecialDamage = 80f + (specialDamageLevel * 15f); Debug.Log("Obrazenia specjalne: " + currentSpecialDamage); }
    public void UpgradeSpecialCooldown() { specialCooldownLevel++; currentSpecialCooldown = Mathf.Max(20f - (specialCooldownLevel * 1f), 5f); Debug.Log("Cooldown: " + currentSpecialCooldown + "s"); }
    public void UpgradeSpecialRotations() { specialRotationsLevel++; currentSpecialRotations = 1 + Mathf.FloorToInt(specialRotationsLevel * 0.5f); Debug.Log("Obroty: " + currentSpecialRotations); }
    public void UpgradeBleed() { bleedLevel++; hasBleed = bleedLevel >= 1; bleedDuration = Mathf.Min(2f + (bleedLevel * 0.5f), 5f); bleedDamage = 3f + (bleedLevel * 2f); Debug.Log("Krwawienie: " + bleedDamage + "/s przez " + bleedDuration + "s"); }
    public void UpgradeUltimateDuration() { ultimateDurationLevel++; currentUltimateDuration = 10f + (ultimateDurationLevel * 2f); Debug.Log("Czas ultimate: " + currentUltimateDuration + "s"); }
    public void UpgradeUltimateRadius() { ultimateRadiusLevel++; currentUltimateRadius = Mathf.Min(1.25f + (ultimateRadiusLevel * 0.25f), 3f); Debug.Log("Promien ultimate: " + currentUltimateRadius + "m"); }
    public void UpgradeUltimateDamage() { ultimateDamageLevel++; currentUltimateDamage = 50f + (ultimateDamageLevel * 15f); Debug.Log("Obrazenia ultimate: " + currentUltimateDamage + "/s"); }

    // ============================================================
    // GÓRAL - NOWE PERKI (CZAS UMIEJÊTNOŒCI)
    // ============================================================

    /// <summary>
    /// Wyd³u¿a czas trwania Stomp o 0.2s
    /// </summary>
    public void UpgradeStompDuration()
    {
        stompDurationLevel++;
        currentStompDuration = 0.5f + (stompDurationLevel * 0.2f);
        Debug.Log("Czas Stomp: " + currentStompDuration + "s");

        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null)
        {
            mountain.stompDuration = currentStompDuration;
        }
    }

    /// <summary>
    /// Wyd³u¿a czas trwania Special o 0.2s
    /// </summary>
    public void UpgradeSpecialDuration()
    {
        specialDurationLevel++;
        currentSpecialDuration = 0.5f + (specialDurationLevel * 0.2f);
        Debug.Log("Czas Special: " + currentSpecialDuration + "s");

        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null)
        {
            mountain.specialDuration = currentSpecialDuration;
        }
    }

    /// <summary>
    /// Wyd³u¿a czas trwania Ultimate o 1s
    /// </summary>
    public void UpgradeUltimateTime()
    {
        ultimateTimeLevel++;
        currentUltimateTime = 3f + (ultimateTimeLevel * 1f);
        Debug.Log("Czas Ultimate: " + currentUltimateTime + "s");

        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null)
        {
            mountain.ultimateTime = currentUltimateTime;
        }
    }

    /// <summary>
    /// Zmniejsza cooldown Special o 1.5s
    /// </summary>
    public void UpgradeSpecialCooldownReduction()
    {
        specialCooldownReductionLevel++;
        currentSpecialCooldownReduction = specialCooldownReductionLevel * 1.5f;
        Debug.Log("Redukcja cooldown Special: -" + currentSpecialCooldownReduction + "s");

        AbilitiesMountainMan mountain = FindFirstObjectByType<AbilitiesMountainMan>();
        if (mountain != null)
        {
            mountain.specialCooldown = Mathf.Max(3f, 12f - currentSpecialCooldownReduction);
        }
    }

    // ============================================================
    // SERAPHIM
    // ============================================================
    public void UpgradeProjectileCount() { projectileCountLevel++; currentProjectileCount = 1 + projectileCountLevel; Debug.Log("Liczba pociskow: " + currentProjectileCount); }
    public void UpgradePierce() { pierceLevel++; canPierce = true; pierceCount = pierceLevel; Debug.Log("Przebicie: " + pierceCount); }

    // ============================================================
    // SHEPHERD
    // ============================================================
    public void UpgradeSheepDamage() { sheepDamageLevel++; currentSheepDamage = 20f + (sheepDamageLevel * 10f); Debug.Log("Obrazenia owcy: " + currentSheepDamage); }
    public void UpgradeSheepSpawnCooldown() { sheepCooldownLevel++; currentSheepSpawnCooldown = Mathf.Max(10f, 45f - (sheepCooldownLevel * 3f)); Debug.Log("Cooldown przyzywania: " + currentSheepSpawnCooldown + "s"); }
    public void UpgradeFeastDamage() { feastDamageLevel++; currentFeastDamage = 350f + (feastDamageLevel * 50f); Debug.Log("Obrazenia Wilczej Uczty: " + currentFeastDamage); }
    public void UpgradeFeastRadius() { feastRadiusLevel++; currentFeastRadius = 3f + (feastRadiusLevel * 0.5f); Debug.Log("Promien Wilczej Uczty: " + currentFeastRadius + "m"); }
}