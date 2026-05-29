using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class enemyHealth : MonoBehaviour
{
    [Header("═══════════════ TYP PRZECIWNIKA ═══════════════")]
    public EnemyType enemyType = EnemyType.Polnocnica;

    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float moveSpeed = 3f;
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
    public float damage = 20f;
    public int expReward = 10;

    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1f;
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
=======
>>>>>>> origin/mati
>>>>>>> Stashed changes

    [Header("═══════════════ ATAK DYSTANSOWY (Leszy) ═══════════════")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float rangedAttackRange = 10f;
    public float rangedCooldown = 3f;

    [Header("═══════════════ ATAK LASEREM (Bazyliszek) ═══════════════")]
    public GameObject laserPrefab;
    public float laserRange = 15f;
    public float laserCooldown = 4f;
    public float laserDuration = 1f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public TextMeshPro healthText;
    public GameObject deathEffect;

    private Transform player;
    private NavMeshAgent agent;
<<<<<<< HEAD
    private float attackTimer = 0f;
    private float rangedTimer = 0f;
    private float laserTimer = 0f;
    private bool isAttacking = false;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();
=======

    void Start()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null) health = 50f + (levelSystem.currentLevel - 1) * 10f;
>>>>>>> origin/igor

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

<<<<<<< HEAD
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        UpdateHealthByLevel();
        ApplyEnemyVisuals();

        Debug.Log($"{enemyType} pojawił się! HP: {currentHealth}, Obrażenia: {damage}");
    }

    void UpdateHealthByLevel()
    {
        if (levelSystem != null && enemyType != EnemyType.Leszy && enemyType != EnemyType.Bazyliszek)
        {
            int levelBonus = levelSystem.currentLevel - 1;
            maxHealth += levelBonus * 10f;
            currentHealth = maxHealth;
            damage += levelBonus * 2f;
            if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
        }
    }

    void ApplyEnemyVisuals()
    {
        if (meshRenderer == null) return;

        switch (enemyType)
        {
            case EnemyType.Polnocnica:
                meshRenderer.material.color = new Color(0.6f, 0.3f, 0.8f);
                break;
            case EnemyType.Strzyga:
                meshRenderer.material.color = new Color(0.5f, 0.2f, 0.1f);
                break;
            case EnemyType.Upior:
                meshRenderer.material.color = new Color(0.4f, 0.6f, 0.3f);
                break;
            case EnemyType.Leszy:
                meshRenderer.material.color = new Color(0.2f, 0.7f, 0.2f);
                transform.localScale = Vector3.one * 1.5f;
                break;
            case EnemyType.Bazyliszek:
                meshRenderer.material.color = new Color(0.8f, 0.6f, 0.1f);
                transform.localScale = Vector3.one * 1.8f;
                break;
        }
=======
        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
>>>>>>> origin/igor
    }

    void Update()
    {
<<<<<<< HEAD
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && distance > attackRange)
=======
        if (player != null && agent != null && agent.isOnNavMesh)
<<<<<<< Updated upstream
            agent.SetDestination(player.position);

=======
<<<<<<< HEAD
<<<<<<< Updated upstream
            agent.SetDestination(player.position);

=======
<<<<<<< Updated upstream
>>>>>>> origin/igor
        {
            agent.SetDestination(player.position);
        }
        else if (agent != null)
        {
            agent.ResetPath();
        }

        if (distance <= attackRange && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                MeleeAttack();
            }
        }
=======
            agent.SetDestination(player.position);
>>>>>>> Stashed changes

<<<<<<< HEAD
        if (enemyType == EnemyType.Leszy)
        {
            HandleLeszyAttacks(distance);
        }
        else if (enemyType == EnemyType.Bazyliszek)
        {
            HandleBazyliszekAttacks(distance);
        }

=======
>>>>>>> Stashed changes
=======
            agent.SetDestination(player.position);

>>>>>>> origin/mati
>>>>>>> Stashed changes
>>>>>>> origin/igor
        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

<<<<<<< HEAD
    void HandleLeszyAttacks(float distance)
    {
        if (isAttacking) return;

        rangedTimer += Time.deltaTime;
        if (distance <= rangedAttackRange && rangedTimer >= rangedCooldown)
        {
            rangedTimer = 0f;
            StartCoroutine(RangedAttack());
        }
    }

    void HandleBazyliszekAttacks(float distance)
    {
        if (isAttacking) return;

        laserTimer += Time.deltaTime;
        if (distance <= laserRange && laserTimer >= laserCooldown)
        {
            laserTimer = 0f;
            StartCoroutine(LaserAttack());
        }
    }

    void MeleeAttack()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float finalDamage = damage;

                if (enemyType == EnemyType.Strzyga && Random.Range(0, 100) < 20)
                {
                    finalDamage *= 2;
                    Debug.Log("Strzyga zadała podwójne obrażenia!");
                }

                playerHealth.TakeDamage(finalDamage);
                Debug.Log($"{enemyType} zadał {finalDamage} obrażeń!");

                if (enemyType == EnemyType.Upior)
                {
                    StartCoroutine(SlowPlayer());
                }
            }
        }
    }

    IEnumerator SlowPlayer()
    {
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                float originalPlayerSpeed = movement.maxSpeed;
                movement.maxSpeed = originalPlayerSpeed * 0.5f;
                Debug.Log("Upiór spowolnił gracza!");
                yield return new WaitForSeconds(2f);
                movement.maxSpeed = originalPlayerSpeed;
            }
        }
    }

    IEnumerator RangedAttack()
    {
        isAttacking = true;
        Debug.Log("Leszy rzuca pociskiem!");

        if (meshRenderer != null) meshRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.3f);

        if (projectilePrefab != null && player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            GameObject projectile = Instantiate(projectilePrefab, transform.position + Vector3.up * 1f, Quaternion.LookRotation(direction));

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * projectileSpeed;
            }

            Projectile projScript = projectile.GetComponent<Projectile>();
            if (projScript != null) projScript.damage = damage * 0.8f;

            Destroy(projectile, 5f);
        }

        if (meshRenderer != null) meshRenderer.material.color = originalColor;
        isAttacking = false;
    }

    IEnumerator LaserAttack()
    {
        isAttacking = true;
        Debug.Log("Bazyliszek używa wzroku kamiennego!");

        if (meshRenderer != null) meshRenderer.material.color = Color.yellow;

        float chargeTime = 0.5f;
        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage * 1.5f);
                Debug.Log($"Bazyliszek zadał {damage * 1.5f} obrażeń laserem!");
            }
        }

        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            Destroy(laser, laserDuration);
        }

        yield return new WaitForSeconds(0.5f);

        if (meshRenderer != null) meshRenderer.material.color = originalColor;
        isAttacking = false;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.material.color = originalColor;
        }
=======
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
        if (health <= 0) Die();
>>>>>>> origin/igor
    }

    void Die()
    {
<<<<<<< HEAD
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (levelSystem != null)
            levelSystem.EnemyDied();

        Debug.Log($"{enemyType} zginął!");
=======
        if (levelSystem != null) levelSystem.EnemyDied();
>>>>>>> origin/igor
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (enemyType == EnemyType.Leszy)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, rangedAttackRange);
        }

        if (enemyType == EnemyType.Bazyliszek)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, laserRange);
        }
    }
}