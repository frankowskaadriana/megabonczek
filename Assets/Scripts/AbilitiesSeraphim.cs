using UnityEngine;
using System.Collections;

public class SeraphimAbilities : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 40f;
    public float moveSpeed = 6f;

    [Header("Atak")]
    public float attackDamage = 30f;
    public float attackRate = 0.6f;
    public float attackRange = 10f;
    public GameObject lightBeamPrefab;
    public Transform firePoint;

    [Header("Heavenly Charge (Q)")]
    public float chargeDamage = 60f;
    public float chargeCooldown = 15f;
    public float chargeRange = 7.5f;
    public float chargeDuration = 2f;

    [Header("Divine Judgment (R)")]
    public float judgmentRadius = 25f;
    public float judgmentStunDuration = 2f;

    [Header("References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;

    private float attackTimer = 0f;
    private bool isChargeOnCooldown = false;
    private bool isJudgmentOnCooldown = false;
    private float chargeCooldownTimer = 0f;
    private float judgmentCooldownTimer = 0f;
    private float originalMoveSpeed;
    private bool isCastingJudgment = false;

    void Start()
    {
        originalMoveSpeed = moveSpeed;

        if (playerHealth != null)
        {
            playerHealth.maxHealth = maxHealth;
            playerHealth.currentHealth = maxHealth;
            playerHealth.UpdateUI();
        }

        if (weaponUpgrade != null)
        {
            attackDamage = weaponUpgrade.currentDamage;
            chargeDamage = weaponUpgrade.currentSpecialDamage;
            chargeCooldown = weaponUpgrade.currentSpecialCooldown;
        }

        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (isCastingJudgment) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            PerformRangedAttack();
        }

        if (isChargeOnCooldown)
        {
            chargeCooldownTimer -= Time.deltaTime;
            if (chargeCooldownTimer <= 0) isChargeOnCooldown = false;
        }

        if (isJudgmentOnCooldown)
        {
            judgmentCooldownTimer -= Time.deltaTime;
            if (judgmentCooldownTimer <= 0) isJudgmentOnCooldown = false;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isChargeOnCooldown && !isCastingJudgment)
            StartCoroutine(PerformHeavenlyCharge());

        if (Input.GetKeyDown(KeyCode.R) && !isJudgmentOnCooldown && !isCastingJudgment)
            StartCoroutine(PerformDivineJudgment());
    }

    void PerformRangedAttack()
    {
        if (lightBeamPrefab == null) return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange);
        Transform closestEnemy = null;
        float closestDistance = attackRange;

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = enemy.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            Vector3 direction = (closestEnemy.position - firePoint.position).normalized;
            GameObject projectile = Instantiate(lightBeamPrefab, firePoint.position, Quaternion.LookRotation(direction));
            LightBeam beam = projectile.GetComponent<LightBeam>();
            if (beam != null) beam.damage = attackDamage;
        }
    }

    IEnumerator PerformHeavenlyCharge()
    {
        isChargeOnCooldown = true;
        chargeCooldownTimer = chargeCooldown;

        moveSpeed = originalMoveSpeed * 1.5f;

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * chargeRange;

        while (elapsed < chargeDuration)
        {
            float t = elapsed / chargeDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null) e.TakeDamage(chargeDamage);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        moveSpeed = originalMoveSpeed;
    }

    IEnumerator PerformDivineJudgment()
    {
        isJudgmentOnCooldown = true;
        isCastingJudgment = true;
        judgmentCooldownTimer = 60f;

        float castTimer = 0f;
        while (castTimer < 5f)
        {
            castTimer += Time.deltaTime;
            yield return null;
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, judgmentRadius);
        int hitCount = 0;

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemyHealth e = enemy.GetComponent<enemyHealth>();
                if (e != null)
                {
                    e.TakeDamage(999999f);
                    hitCount++;
                }
            }
        }

        float originalSpeed = moveSpeed;
        moveSpeed = 0f;
        yield return new WaitForSeconds(judgmentStunDuration);
        moveSpeed = originalSpeed;

        isCastingJudgment = false;
        Debug.Log("Boski Osad zabil " + hitCount + " wrogow");
    }
}