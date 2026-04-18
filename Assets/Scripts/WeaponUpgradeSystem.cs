using UnityEngine;

public class WeaponUpgradeSystem : MonoBehaviour
{
    [Header("=== BRON (CIUPAGA) ===")]
    public float currentDamage = 50f;
    public float currentRange = 1.5f;
    public float currentSwingAngle = 90f;

    [Header("=== ZDOLNOŒÆ (Gniew Tatr) ===")]
    public float currentSpecialCooldown = 20f;
    public float currentSpecialDamage = 80f;
    public int currentSpecialRotations = 1;
    public bool hasBleed = false;
    public float bleedDuration = 2f;
    public float bleedDamage = 3f;

    [Header("=== ULTIMATE (Orli Grom) ===")]
    public float currentUltimateDuration = 10f;
    public float currentUltimateRadius = 1.25f;
    public float currentUltimateDamage = 50f;

    // Publiczne pola dla poziomów ulepszeñ (dla dostêpu z innych skryptów)
    public int damageLevel = 0;
    public int rangeLevel = 0;
    public int swingLevel = 0;
    public int specialCooldownLevel = 0;
    public int specialDamageLevel = 0;
    public int specialRotationsLevel = 0;
    public int bleedLevel = 0;
    public int ultimateDurationLevel = 0;
    public int ultimateRadiusLevel = 0;
    public int ultimateDamageLevel = 0;

    public void UpgradeDamage()
    {
        damageLevel++;
        currentDamage = 50f + (damageLevel * 10f);
        Debug.Log($"Obra¿enia broni: {currentDamage}");
    }

    public void UpgradeRange()
    {
        rangeLevel++;
        currentRange = Mathf.Min(1.5f + (rangeLevel * 0.1f), 2f);
        Debug.Log($"Zasiêg broni: {currentRange}m");
    }

    public void UpgradeSwingAngle()
    {
        swingLevel++;
        currentSwingAngle = Mathf.Min(90f + (swingLevel * 10f), 150f);
        Debug.Log($"K¹t zamachu: {currentSwingAngle}°");
    }

    public void UpgradeSpecialCooldown()
    {
        specialCooldownLevel++;
        currentSpecialCooldown = Mathf.Max(20f - (specialCooldownLevel * 1f), 5f);
        Debug.Log($"Cooldown zdolnoœci: {currentSpecialCooldown}s");
    }

    public void UpgradeSpecialDamage()
    {
        specialDamageLevel++;
        currentSpecialDamage = 80f + (specialDamageLevel * 15f);
        Debug.Log($"Obra¿enia zdolnoœci: {currentSpecialDamage}");
    }

    public void UpgradeSpecialRotations()
    {
        specialRotationsLevel++;
        currentSpecialRotations = 1 + Mathf.FloorToInt(specialRotationsLevel * 0.5f);
        Debug.Log($"Iloœæ obrotów: {currentSpecialRotations}");
    }

    public void UpgradeBleed()
    {
        bleedLevel++;
        hasBleed = bleedLevel >= 1;
        bleedDuration = Mathf.Min(2f + (bleedLevel * 0.5f), 5f);
        bleedDamage = 3f + (bleedLevel * 2f);
        Debug.Log($"Krwawienie: {bleedDamage}/s przez {bleedDuration}s");
    }

    public void UpgradeUltimateDuration()
    {
        ultimateDurationLevel++;
        currentUltimateDuration = 10f + (ultimateDurationLevel * 2f);
        Debug.Log($"Czas ultimate: {currentUltimateDuration}s");
    }

    public void UpgradeUltimateRadius()
    {
        ultimateRadiusLevel++;
        currentUltimateRadius = Mathf.Min(1.25f + (ultimateRadiusLevel * 0.25f), 3f);
        Debug.Log($"Promieñ ultimate: {currentUltimateRadius}m");
    }

    public void UpgradeUltimateDamage()
    {
        ultimateDamageLevel++;
        currentUltimateDamage = 50f + (ultimateDamageLevel * 15f);
        Debug.Log($"Obra¿enia ultimate: {currentUltimateDamage}/s");
    }
}