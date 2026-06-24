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

    [Header("═══════════════ INACCURACY (NIECELNOŚĆ) ═══════════════")]
    public float inaccuracyAngle = 5f;
    public float maxInaccuracy = 15f;
    public float inaccuracyRecoveryRate = 2f;

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
    public SeraphimAnimationController animController;

    private float fireTimer = 0f;
    private bool isChargeOnCooldown = false;
    private bool isJudgmentOnCooldown = false;
    private float chargeCooldownTimer = 0f;
    private float judgmentCooldownTimer = 0f;
    private float originalMoveSpeed;
    private bool isCastingJudgment = false;
    private PlayerMovement playerMovement;
    private Camera mainCamera;
    private AudioManager audioManager;
    private float currentInaccuracy = 0f;
    private bool isShooting = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
            originalMoveSpeed = playerMovement.maxSpeed;

        mainCamera = Camera.main;
        audioManager = AudioManager.Instance;

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

        // Znajdź kontroler animacji
        if (animController == null)
            animController = GetComponent<SeraphimAnimationController>();
        if (animController == null)
            animController = GetComponentInChildren<SeraphimAnimationController>();
        if (animController == null)
            animController = gameObject.AddComponent<SeraphimAnimationController>();

        if (bulletPrefab == null)
        {
            Debug.LogError("❌ BULLET PREFAB NIE JEST PRZYPISANY!");
        }

        Debug.Log("Seraphim gotowy!");
    }

    void Update()
    {
        if (isCastingJudgment) return;

        currentInaccuracy = Mathf.Max(0, currentInaccuracy - inaccuracyRecoveryRate * Time.deltaTime);

        fireTimer += Time.deltaTime;
        if (fireTimer >= fireRate)
        {
            fireTimer = 0f;
            Shoot();
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
            StartCoroutine(HeavenlyCharge());

        if (Input.GetKeyDown(KeyCode.R) && !isJudgmentOnCooldown && !isCastingJudgment)
            StartCoroutine(DivineJudgment());
    }

    void Shoot()
    {
        if (bulletPrefab == null) return;

        currentInaccuracy = Mathf.Min(maxInaccuracy, currentInaccuracy + inaccuracyAngle);

        if (abilityVisuals != null)
            abilityVisuals.ShowAttackRange();

        if (audioManager != null)
            audioManager.PlayAttack();

        // ANIMACJA STRZAŁU
        if (animController != null)
            animController.TriggerShoot();

        Vector3 direction = GetAimDirection();
        Vector3 finalDirection = ApplyInaccuracy(direction);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(finalDirection));

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

    Vector3 ApplyInaccuracy(Vector3 direction)
    {
        if (currentInaccuracy <= 0) return direction;

        float randomAngle = Random.Range(0f, 360f);
        float randomOffset = Random.Range(0f, currentInaccuracy);

        Vector3 horizontalOffset = Quaternion.Euler(0, randomAngle, 0) * Vector3.right * randomOffset;
        Vector3 finalDirection = direction + (horizontalOffset * 0.01f);

        return finalDirection.normalized;
    }

    IEnumerator HeavenlyCharge()
    {
        isChargeOnCooldown = true;
        chargeCooldownTimer = chargeCooldown;

        if (abilityVisuals != null)
            abilityVisuals.ShowSpecialRange();

        if (audioManager != null)
        {
            audioManager.PlayCharge();
            audioManager.PlaySpecialAbility();
        }

        // ANIMACJA UMIEJĘTNOŚCI
        if (animController != null)
            animController.TriggerAbility();

        if (playerMovement != null)
            playerMovement.maxSpeed = originalMoveSpeed * 1.5f;

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

        if (playerMovement != null)
            playerMovement.maxSpeed = originalMoveSpeed;

        Debug.Log("Heavenly Charge zakończone!");
    }

    IEnumerator DivineJudgment()
    {
        isJudgmentOnCooldown = true;
        isCastingJudgment = true;
        judgmentCooldownTimer = 60f;

        if (abilityVisuals != null)
            abilityVisuals.ShowUltimateRange();

        if (audioManager != null)
            audioManager.PlayUltimate();

        // ANIMACJA ULTIMATE
        if (animController != null)
            animController.TriggerUltimate();

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
}