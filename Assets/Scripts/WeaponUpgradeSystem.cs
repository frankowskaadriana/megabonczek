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

    private int damageLevel = 0;
    private int rangeLevel = 0;
    private int specialCooldownLevel = 0;
    private int specialDamageLevel = 0;
    private int specialRotationsLevel = 0;
    private int ultimateDamageLevel = 0;
    private int ultimateRadiusLevel = 0;

    public void UpgradeDamage()
    {
        damageLevel++;
        currentDamage = 50f + (damageLevel * 10f);
        Debug.Log("Obrazenia: " + currentDamage);
    }

    public void UpgradeRange()
    {
        rangeLevel++;
        currentRange = Mathf.Min(1.5f + (rangeLevel * 0.1f), 2f);
        Debug.Log("Zasieg: " + currentRange + "m");
    }

    public void UpgradeSpecialDamage()
    {
        specialDamageLevel++;
        currentSpecialDamage = 80f + (specialDamageLevel * 15f);
        Debug.Log("Obrazenia specjalne: " + currentSpecialDamage);
    }

    public void UpgradeSpecialCooldown()
    {
        specialCooldownLevel++;
        currentSpecialCooldown = Mathf.Max(20f - (specialCooldownLevel * 1f), 5f);
        Debug.Log("Cooldown: " + currentSpecialCooldown + "s");
    }

    public void UpgradeSpecialRotations()
    {
        specialRotationsLevel++;
        currentSpecialRotations = 1 + Mathf.FloorToInt(specialRotationsLevel * 0.5f);
        Debug.Log("Obroty: " + currentSpecialRotations);
    }

    public void UpgradeUltimateDamage()
    {
        ultimateDamageLevel++;
        currentUltimateDamage = 50f + (ultimateDamageLevel * 15f);
        Debug.Log("Obrazenia ultimate: " + currentUltimateDamage);
    }

    public void UpgradeUltimateRadius()
    {
        ultimateRadiusLevel++;
        currentUltimateRadius = Mathf.Min(1.25f + (ultimateRadiusLevel * 0.25f), 3f);
        Debug.Log("Promien ultimate: " + currentUltimateRadius);
    }
}