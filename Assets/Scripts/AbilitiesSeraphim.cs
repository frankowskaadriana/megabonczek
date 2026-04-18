using UnityEngine;
using System.Collections;

public class AngelAbilities : MonoBehaviour
{
    [Header("Statystyki Podstawowe")]
    public float maxHealth = 40f;
    public float moveSpeed = 6f;

    [Header("Bron: Wiazki Swiatla")]
    public float attackDamage = 30f;
    public float attackRate = 0.6f;
    public float attackRange = 10f;
    public int projectileCount = 1;
    public int pierceCount = 0;
    public bool canPierce = false;
    public GameObject lightBeamPrefab;
    public Transform firePoint;

    [Header("Heavenly Charge")]
    public float chargeDamage = 60f;
    public float chargeCooldown = 15f;
    public float chargeRange = 7.5f;
    public float chargeSpeedBoost = 0.5f;
    public float chargeDuration = 2f;

    [Header("Divine Judgment")]
    public float judgmentCastTime = 5f;
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
    private Coroutine chargeBoostCoroutine;
    private bool isCastingJudgment = false;

    void Start()
    {
        originalMoveSpeed = moveSpeed;

        if (playerHealth != null)
        {
            playerHealth.maxHealth = maxHealth;
            playerHealth.HeathValue = maxHealth;
            playerHealth.UpdateHealthUI();
        }

        if (weaponUpgrade != null)
        {
            attackDamage = weaponUpgrade.currentDamage;
            projectileCount = 1 + (weaponUpgrade.damageLevel / 3);
            if (projectileCount < 1) projectileCount = 1;

            pierceCount = weaponUpgrade.damageLevel / 2;
            canPierce = pierceCount > 0;
            chargeDamage = weaponUpgrade.currentSpecialDamage;
            chargeCooldown = weaponUpgrade.currentSpecialCooldown;
            chargeRange = weaponUpgrade.currentRange + 5f;
        }

        if (firePoint == null)
        {
            firePoint = transform;
        }
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
        {
            StartCoroutine(PerformHeavenlyCharge());
        }

        if (Input.GetKeyDown(KeyCode.R) && !isJudgmentOnCooldown && !isCastingJudgment)
        {
            StartCoroutine(PerformDivineJudgment());
        }
    }

    void PerformRangedAttack()
    {
        if (lightBeamPrefab == null) return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange);
        float closestDistance = attackRange;
        Transform closestEnemy = null;

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            Vector3 direction = (closestEnemy.position - firePoint.position).normalized;

            for (int i = 0; i < projectileCount; i++)
            {
                Vector3 spreadDirection = direction;
                if (projectileCount > 1)
                {
                    float spreadAngle = 10f;
                    float angleOffset = ((float)i / (projectileCount - 1) - 0.5f) * spreadAngle;
                    spreadDirection = Quaternion.Euler(0, angleOffset, 0) * direction;
                }

                GameObject projectile = Instantiate(lightBeamPrefab, firePoint.position, Quaternion.LookRotation(spreadDirection));
                LightBeam beam = projectile.GetComponent<LightBeam>();
                if (beam != null)
                {
                    beam.damage = attackDamage;
                    beam.pierceCount = pierceCount;
                    beam.canPierce = canPierce;
                }
            }
        }
    }

    IEnumerator PerformHeavenlyCharge()
    {
        isChargeOnCooldown = true;
        chargeCooldownTimer = chargeCooldown;

        if (chargeBoostCoroutine != null)
            StopCoroutine(chargeBoostCoroutine);
        chargeBoostCoroutine = StartCoroutine(SpeedBoost(chargeDuration));

        float elapsed = 0f;
        float dashDistance = chargeRange;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * dashDistance;

        while (elapsed < chargeDuration)
        {
            float t = elapsed / chargeDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                    if (enemyScript != null)
                        enemyScript.TakeDamage(chargeDamage);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        Debug.Log("Heavenly Charge zakonczony");
    }

    IEnumerator SpeedBoost(float duration)
    {
        moveSpeed = originalMoveSpeed * (1f + chargeSpeedBoost);
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }

    IEnumerator PerformDivineJudgment()
    {
        isJudgmentOnCooldown = true;
        isCastingJudgment = true;
        judgmentCooldownTimer = 60f;

        Debug.Log("Boski Osad: Rozkladanie... 5s");

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Color originalColor = renderer != null ? renderer.material.color : Color.white;
        if (renderer != null) renderer.material.color = Color.yellow;

        float castTimer = 0f;
        while (castTimer < judgmentCastTime)
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
                enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(999999f);
                    hitCount++;
                }
            }
        }

        Debug.Log("Boski Osad! Zabito: " + hitCount + " wrogow");

        float originalSpeed = moveSpeed;
        moveSpeed = 0f;

        if (renderer != null) renderer.material.color = Color.gray;

        yield return new WaitForSeconds(judgmentStunDuration);

        moveSpeed = originalSpeed;
        if (renderer != null) renderer.material.color = originalColor;

        isCastingJudgment = false;
        Debug.Log("Boski Osad zakonczony");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chargeRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, judgmentRadius);
    }
}