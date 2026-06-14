using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("═══════════════ PODSTAWOWY ATAK ═══════════════")]
    public float attackDamage = 50f;
    public float attackRange = 1.5f;
    public float attackAngle = 90f;
    public float attackRate = 0.8f;

    [Header("═══════════════ GNIEW TATR (Q) ═══════════════")]
    public float specialDamage = 80f;
    public float specialCooldown = 20f;
    public int specialRotations = 1;
    public float healValue = 30f;
    public float specialRange = 3f;

    [Header("═══════════════ ORLI GROM (R) ═══════════════")]
    public float ultimateDuration = 10f;
    public float ultimateRadius = 1.25f;
    public float ultimateDamage = 50f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public AbilityVisuals abilityVisuals;

    private float attackTimer = 0f;
    private bool isSpecialOnCooldown = false;
    private bool isUltimateOnCooldown = false;
    private float specialCooldownTimer = 0f;
    private float ultimateCooldownTimer = 0f;
    private AudioManager audioManager;

    void Start()
    {
        if (weaponUpgrade != null)
        {
            attackDamage = weaponUpgrade.currentDamage;
            attackRange = weaponUpgrade.currentRange;
            attackAngle = weaponUpgrade.currentSwingAngle;
            specialDamage = weaponUpgrade.currentSpecialDamage;
            specialCooldown = weaponUpgrade.currentSpecialCooldown;
            specialRotations = weaponUpgrade.currentSpecialRotations;
            ultimateDuration = weaponUpgrade.currentUltimateDuration;
            ultimateRadius = weaponUpgrade.currentUltimateRadius;
            ultimateDamage = weaponUpgrade.currentUltimateDamage;
        }

        if (abilityVisuals == null)
            abilityVisuals = GetComponent<AbilityVisuals>();

        audioManager = AudioManager.Instance;
        Debug.Log("AbilitiesMountainMan zainicjalizowany!");
    }

    void Update()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            PerformBasicAttack();
        }

        if (isSpecialOnCooldown)
        {
            specialCooldownTimer -= Time.deltaTime;
            if (specialCooldownTimer <= 0)
            {
                isSpecialOnCooldown = false;
                Debug.Log("Gniew Tatr gotowy do użycia!");
            }
        }

        if (isUltimateOnCooldown)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0)
            {
                isUltimateOnCooldown = false;
                Debug.Log("Orli Grom gotowy do użycia!");
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isSpecialOnCooldown)
            {
                StartCoroutine(PerformSpecial());
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isUltimateOnCooldown)
            {
                StartCoroutine(PerformUltimate());
            }
        }
    }

    void PerformBasicAttack()
    {
        if (abilityVisuals != null)
            abilityVisuals.ShowAttackRange();

        if (audioManager != null) audioManager.PlayAttack();

        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        int hitCount = 0;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= attackAngle / 2)
                {
                    enemyHealth enemy = hit.GetComponent<enemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage);
                        hitCount++;
                    }
                }
            }
        }

        if (hitCount > 0)
            Debug.Log($"Ciupaga! Trafiono {hitCount} wrogów za {attackDamage} obrażeń");
    }

    IEnumerator PerformSpecial()
    {
        isSpecialOnCooldown = true;
        specialCooldownTimer = specialCooldown;

        if (abilityVisuals != null)
            abilityVisuals.ShowSpecialRange();

        if (audioManager != null) audioManager.PlaySpecialAbility();

        if (playerHealth != null)
        {
            playerHealth.Heal(healValue);
        }

        for (int i = 0; i < specialRotations; i++)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, specialRange);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null)
                    {
                        e.TakeDamage(specialDamage);
                    }
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator PerformUltimate()
    {
        isUltimateOnCooldown = true;
        ultimateCooldownTimer = ultimateDuration;

        if (abilityVisuals != null)
            abilityVisuals.ShowUltimateRange();

        if (audioManager != null) audioManager.PlayUltimate();

        float elapsed = 0f;

        while (elapsed < ultimateDuration)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, ultimateRadius);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null)
                    {
                        e.TakeDamage(ultimateDamage * Time.deltaTime);
                    }
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}