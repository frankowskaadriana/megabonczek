using UnityEngine;
using System.Collections;

public class SeraphimAbilities : MonoBehaviour
{
    [Header("═══════════════ PODSTAWOWY ATAK ═══════════════")]
    public float damage = 30f;
    public float fireRate = 0.6f;
    public float attackRange = 10f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("═══════════════ Heavenly Charge (Q) ═══════════════")]
    public float chargeDamage = 60f;
    public float chargeCooldown = 15f;
    public float chargeRange = 7.5f;
    public float chargeDuration = 2f;

    [Header("═══════════════ Divine Judgment (R) ═══════════════")]
    public float judgmentRadius = 25f;
    public float judgmentStunDuration = 2f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;
    public AbilityVisuals abilityVisuals;

    private float fireTimer = 0f;
    private bool isChargeOnCooldown = false;
    private bool isJudgmentOnCooldown = false;
    private float chargeCooldownTimer = 0f;
    private float judgmentCooldownTimer = 0f;
    private float originalMoveSpeed;
    private bool isCastingJudgment = false;
    private PlayerMovement playerMovement;
    private Camera mainCamera;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
            originalMoveSpeed = playerMovement.maxSpeed;

        mainCamera = Camera.main;

        if (playerHealth != null)
        {
            playerHealth.maxHealth = 40f;
            playerHealth.currentHealth = 40f;
            playerHealth.UpdateUI();
        }

        if (weaponUpgrade != null)
        {
            damage = weaponUpgrade.currentDamage;
            chargeDamage = weaponUpgrade.currentSpecialDamage;
            chargeCooldown = weaponUpgrade.currentSpecialCooldown;
        }

        if (firePoint == null)
            firePoint = transform;

        if (abilityVisuals == null)
            abilityVisuals = GetComponent<AbilityVisuals>();

        if (bulletPrefab == null)
        {
            Debug.LogError("❌ BULLET PREFAB NIE JEST PRZYPISANY! Stwórz prostą kulę i przeciągnij!");
        }

        Debug.Log("Seraphim gotowy! Atak co " + fireRate + "s");
    }

    void Update()
    {
        if (isCastingJudgment) return;

        // Automatyczny atak
        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot();
        }

        // Cooldown dla Heavenly Charge
        if (isChargeOnCooldown)
        {
            chargeCooldownTimer -= Time.deltaTime;
            if (chargeCooldownTimer <= 0)
            {
                isChargeOnCooldown = false;
                Debug.Log("Heavenly Charge gotowe!");
            }
        }

        // Cooldown dla Divine Judgment
        if (isJudgmentOnCooldown)
        {
            judgmentCooldownTimer -= Time.deltaTime;
            if (judgmentCooldownTimer <= 0)
            {
                isJudgmentOnCooldown = false;
                Debug.Log("Divine Judgment gotowe!");
            }
        }

        // Heavenly Charge na Q
        if (Input.GetKeyDown(KeyCode.Q) && !isChargeOnCooldown && !isCastingJudgment)
            StartCoroutine(HeavenlyCharge());

        // Divine Judgment na R
        if (Input.GetKeyDown(KeyCode.R) && !isJudgmentOnCooldown && !isCastingJudgment)
            StartCoroutine(DivineJudgment());
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        // Pokaż linię trajektorii
        if (abilityVisuals != null)
            abilityVisuals.ShowAttackRange();

        // Oblicz kierunek do kursora
        Vector3 direction = GetAimDirection();

        // Stwórz pocisk
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction));

        // Ustaw obrażenia
        LightBeam beam = bullet.GetComponent<LightBeam>();
        if (beam != null)
            beam.damage = damage;

        Destroy(bullet, 3f);
    }

    Vector3 GetAimDirection()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            return (hitPoint - firePoint.position).normalized;
        }

        return transform.forward;
    }

    IEnumerator HeavenlyCharge()
    {
        isChargeOnCooldown = true;
        chargeCooldownTimer = chargeCooldown;

        // Pokaż wizualizację
        if (abilityVisuals != null)
            abilityVisuals.ShowSpecialRange();

        // Zwiększ prędkość
        if (playerMovement != null)
            playerMovement.maxSpeed = originalMoveSpeed * 1.5f;

        // Szarża
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + transform.forward * chargeRange;

        while (elapsed < chargeDuration)
        {
            float t = elapsed / chargeDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);

            // Zadaj obrażenia wrogom w zasięgu
            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth e = enemy.GetComponent<enemyHealth>();
                    if (e != null)
                        e.TakeDamage(chargeDamage);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Przywróć prędkość
        if (playerMovement != null)
            playerMovement.maxSpeed = originalMoveSpeed;

        Debug.Log("Heavenly Charge zakończone!");
    }

    IEnumerator DivineJudgment()
    {
        isJudgmentOnCooldown = true;
        isCastingJudgment = true;
        judgmentCooldownTimer = 60f;

        // Pokaż wizualizację
        if (abilityVisuals != null)
            abilityVisuals.ShowUltimateRange();

        // Czas ładowania 5 sekund
        float castTimer = 0f;
        while (castTimer < 5f)
        {
            castTimer += Time.deltaTime;
            yield return null;
        }

        // One-shot w promieniu
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

        // Unieruchomienie na 2 sekundy
        if (playerMovement != null)
        {
            float originalSpeed = playerMovement.maxSpeed;
            playerMovement.maxSpeed = 0f;
            yield return new WaitForSeconds(judgmentStunDuration);
            playerMovement.maxSpeed = originalSpeed;
        }

        isCastingJudgment = false;
        Debug.Log($"Boski Osad zabił {hitCount} wrogów!");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, chargeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, judgmentRadius);
    }
}