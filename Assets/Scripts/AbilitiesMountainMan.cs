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
            ultimateDuration = weaponUpgrade.currentUltimateDuration;
            ultimateRadius = weaponUpgrade.currentUltimateRadius;
            ultimateDamage = weaponUpgrade.currentUltimateDamage;
        }

        Debug.Log("AbilitiesMountainMan zainicjalizowany!");
    }

    void Update()
    {
        // Automatyczny atak
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            PerformBasicAttack();
        }

        // Cooldown dla Gniewu Tatr (Q)
        if (isSpecialOnCooldown)
        {
            specialCooldownTimer -= Time.deltaTime;
            if (specialCooldownTimer <= 0)
            {
                isSpecialOnCooldown = false;
                Debug.Log("Gniew Tatr gotowy do użycia!");
            }
        }

        // Cooldown dla Orlego Gromu (R)
        if (isUltimateOnCooldown)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0)
            {
                isUltimateOnCooldown = false;
                Debug.Log("Orli Grom gotowy do użycia!");
            }
        }

        // Gniew Tatr na Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isSpecialOnCooldown)
            {
                StartCoroutine(PerformSpecial());
            }
            else
            {
                Debug.Log($"Gniew Tatr na cooldownie! Pozostało: {specialCooldownTimer:F1}s");
            }
        }

        // Orli Grom na R
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isUltimateOnCooldown)
            {
                StartCoroutine(PerformUltimate());
            }
            else
            {
                Debug.Log($"Orli Grom na cooldownie! Pozostało: {ultimateCooldownTimer:F1}s");
            }
        }
    }

    void PerformBasicAttack()
    {
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

        float elapsed = 0f;
        float tickTime = 0.5f;

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

    // ============ GIZMO - WIDOCZNE W EDYTORZE ============
    void OnDrawGizmos()
    {
        // Zasięg ataku ciupagi (czerwony stożek)
        DrawAttackCone();

        // Zasięg Gniewu Tatr (żółte koło)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, specialRange);

        // Zasięg Orlego Gromu (niebieskie koło)
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
    }

    void DrawAttackCone()
    {
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        // Kolor stożka (czerwony)
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

        // Lewa i prawa krawędź
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(center, leftDir * attackRange);
        Gizmos.DrawRay(center, rightDir * attackRange);

        // Rysuj łuk (linia łącząca końce)
        int segments = 30;
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

        // Rysuj wypełnienie stożka (tylko w edytorze - linie promieniowe)
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
        prevPoint = center + leftDir * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + dir * attackRange;
            Gizmos.DrawLine(center, point);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Rysuj linię środkową
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawRay(center, forward * attackRange);
    }

    // Rysowanie tylko gdy obiekt zaznaczony (bardziej szczegółowe)
    void OnDrawGizmosSelected()
    {
        // Grubsze linie gdy zaznaczony
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        Gizmos.color = new Color(1f, 0f, 0f, 0.8f);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(center, leftDir * attackRange);
        Gizmos.DrawRay(center, rightDir * attackRange);
        Gizmos.DrawRay(center, forward * attackRange);

        // Rysuj łuk z większą liczbą segmentów
        int segments = 40;
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

        // Informacja tekstowa (przybliżona pozycja)
        Gizmos.color = Color.white;

        // Zasięg Gniewu Tatr
        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        Gizmos.DrawWireSphere(center, specialRange);

        // Zasięg Orlego Gromu
        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.6f);
        Gizmos.DrawWireSphere(center, ultimateRadius);

        // Dodatkowe oznaczenie kąta na ziemi (półkole)
        DrawGroundArc();
    }

    void DrawGroundArc()
    {
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        // Rysuj łuk na ziemi (Y=0)
        center.y = 0.05f;
        int segments = 30;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);

        Vector3 prevPoint = center + Quaternion.Euler(0, -halfAngle, 0) * forward * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 point = center + Quaternion.Euler(0, angle, 0) * forward * attackRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}