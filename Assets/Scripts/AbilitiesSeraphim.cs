using UnityEngine;
using System.Collections;

public class AbilitiesSeraphim : MonoBehaviour
{
    [Header("Atak")]
    public float attackRange = 10f;
    public float attackDamage = 15f;
    public float attackRate = 0.8f;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Umiejętności")]
    public float healAmount = 30f;
    public float healCooldown = 10f;
    public float shieldDuration = 5f;
    public float chargeRange = 8f;
    public float chargeDamage = 30f;
    public float chargeSpeed = 15f;
    public float chargeCooldown = 6f;

    [Header("Ultimate - Osąd")]
    public float judgmentRadius = 12f;
    public float judgmentDamage = 80f;
    public float judgmentCooldown = 25f;
    public float judgmentDuration = 3f;
    public GameObject judgmentEffect;

    [Header("Special")]
    public float specialRange = 12f;
    public float specialDamage = 40f;
    public float specialCooldown = 8f;

    private float attackTimer = 0f;
    private float healTimer = 0f;
    private float ultimateTimer = 0f;
    private float specialTimer = 0f;
    private float chargeTimer = 0f;
    private Transform player;
    private bool isShielded = false;
    private bool isCharging = false;
    private Vector3 chargeDirection;
    private Rigidbody rb;
    private Camera mainCamera;
    private bool canShoot = true;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        ultimateTimer = judgmentCooldown;
        specialTimer = specialCooldown;
        chargeTimer = chargeCooldown;

        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (player == null) return;

        // AUTOMATYCZNY STRZAŁ w kierunku myszki
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate && canShoot)
        {
            attackTimer = 0f;
            // Obróć w stronę myszki przed strzałem
            RotateToMouse();
            RangedAttack();
        }

        // Heal
        healTimer += Time.deltaTime;
        if (healTimer >= healCooldown && Input.GetKeyDown(KeyCode.Q))
        {
            healTimer = 0f;
            Heal();
        }

        // Ultimate
        ultimateTimer += Time.deltaTime;
        if (ultimateTimer >= judgmentCooldown && Input.GetKeyDown(KeyCode.R))
        {
            ultimateTimer = 0f;
            StartCoroutine(Judgment());
        }

        // Special
        specialTimer += Time.deltaTime;
        if (specialTimer >= specialCooldown && Input.GetKeyDown(KeyCode.E))
        {
            specialTimer = 0f;
            SpecialAttack();
        }

        // Charge
        chargeTimer += Time.deltaTime;
        if (chargeTimer >= chargeCooldown && Input.GetKeyDown(KeyCode.LeftShift))
        {
            chargeTimer = 0f;
            StartCoroutine(Charge());
        }

        if (isCharging && rb != null)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }
    }

    void RotateToMouse()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    void RangedAttack()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // Celowanie w kursor
        Vector3 targetPosition = GetMouseWorldPosition();
        if (targetPosition == Vector3.zero) return;

        Vector3 direction = (targetPosition - firePoint.position).normalized;
        direction.y = 0f;

        // Wystrzel pocisk w kierunku kursora
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));
        Bullet bullet = projectile.GetComponent<Bullet>();
        if (bullet != null) bullet.damage = attackDamage;
    }

    Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, firePoint.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void Heal()
    {
        if (player == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Debug.Log($"💚 Seraphim: Heal {healAmount} HP!");
        }
    }

    IEnumerator Judgment()
    {
        if (judgmentEffect != null)
        {
            GameObject effect = Instantiate(judgmentEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * judgmentRadius * 2f;
            Destroy(effect, judgmentDuration);
        }

        float timer = 0f;
        while (timer < judgmentDuration)
        {
            timer += Time.deltaTime;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, judgmentRadius);
            foreach (var hitCollider in hitColliders)
            {
                EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(judgmentDamage * Time.deltaTime);
                }

                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(5f * Time.deltaTime);
                }
            }

            yield return null;
        }

        Debug.Log($"⚖️ SERAPHIM OSĄD!");
    }

    void SpecialAttack()
    {
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, specialRange);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(specialDamage);
            }
        }
    }

    IEnumerator Charge()
    {
        if (isCharging) yield break;

        isCharging = true;
        chargeDirection = transform.forward;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, chargeRange);
        float closestDistance = Mathf.Infinity;
        Vector3 closestEnemy = transform.position + transform.forward * chargeRange;

        foreach (var hitCollider in hitColliders)
        {
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform.position;
                }
            }
        }

        if (closestDistance < chargeRange)
        {
            chargeDirection = (closestEnemy - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(chargeDirection);
        }

        float chargeTime = 0.3f;
        while (chargeTime > 0)
        {
            chargeTime -= Time.deltaTime;

            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (var enemyCollider in enemies)
            {
                EnemyHealth enemy = enemyCollider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(chargeDamage);
                    Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.AddForce(chargeDirection * 15f, ForceMode.Impulse);
                    }
                }
            }

            yield return null;
        }

        isCharging = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Debug.Log($"⚡ Seraphim Charge: {chargeDamage} obrażeń!");
    }

    public void ActivateShield()
    {
        if (!isShielded)
        {
            isShielded = true;
            StartCoroutine(ShieldDuration());
        }
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(shieldDuration);
        isShielded = false;
        Debug.Log("🛡️ Tarcza wygasła!");
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, judgmentRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * specialRange);
    }
}