using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("Atak")]
    public float attackDamage = 50f;
    public float attackRange = 1.5f;
    public float attackAngle = 90f;
    public float attackRate = 0.8f;

    [Header("Gniew Tatr (Q)")]
    public float specialDamage = 80f;
    public float specialCooldown = 20f;
    public int specialRotations = 1;
    public float healValue = 30f;

    [Header("Orli Grom (R)")]
    public float ultimateDuration = 10f;
    public float ultimateRadius = 1.25f;
    public float ultimateDamage = 50f;

    [Header("References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;

    private float attackTimer = 0f;
    private bool isSpecialOnCooldown = false;
    private bool isUltimateOnCooldown = false;
    private float specialCooldownTimer = 0f;
    private float ultimateCooldownTimer = 0f;

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
        }
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
            if (specialCooldownTimer <= 0) isSpecialOnCooldown = false;
        }

        if (isUltimateOnCooldown)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0) isUltimateOnCooldown = false;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isSpecialOnCooldown)
            StartCoroutine(PerformSpecial());

        if (Input.GetKeyDown(KeyCode.R) && !isUltimateOnCooldown)
            StartCoroutine(PerformUltimate());
    }

    void PerformBasicAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= attackAngle / 2)
                {
                    enemyHealth enemy = hit.GetComponent<enemyHealth>();
                    if (enemy != null) enemy.TakeDamage(attackDamage);
                }
            }
        }
    }

    IEnumerator PerformSpecial()
    {
        isSpecialOnCooldown = true;
        specialCooldownTimer = specialCooldown;

        if (playerHealth != null) playerHealth.Heal(healValue);

        for (int i = 0; i < specialRotations; i++)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, 3f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null) e.TakeDamage(specialDamage);
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator PerformUltimate()
    {
        isUltimateOnCooldown = true;
        ultimateCooldownTimer = ultimateDuration;

        float elapsed = 0f;
        while (elapsed < ultimateDuration)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, ultimateRadius);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null) e.TakeDamage(ultimateDamage * Time.deltaTime);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Tylko gdy obiekt zaznaczony
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(center, leftDir * attackRange);
        Gizmos.DrawRay(center, rightDir * attackRange);

        int segments = 20;
        Vector3 prevPoint = center + leftDir * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + dir * attackRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center, 3f);

        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
        Gizmos.DrawWireSphere(center, ultimateRadius);
    }
}