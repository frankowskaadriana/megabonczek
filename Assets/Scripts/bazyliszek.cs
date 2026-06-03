using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Bazyliszek : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 800f;
    public float currentHealth;
    public float moveSpeed = 1.5f;
    public float damage = 50f;
    public int expReward = 200;

    [Header("═══════════════ ATAK WRĘCZ ═══════════════")]
    public float attackRange = 3f;
    public float attackCooldown = 2f;

    [Header("═══════════════ ATAK LASEREM ═══════════════")]
    public GameObject laserPrefab;
    public float laserRange = 15f;
    public float laserCooldown = 5f;
    public float laserDamage = 60f;
    public float laserChargeTime = 1.5f;

    [Header("═══════════════ EFEKTY WIZUALNE ═══════════════")]
    public GameObject deathEffect;
    public GameObject hitEffect;
    public GameObject chargeEffect;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public TextMeshPro healthText;

    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private float laserTimer = 0f;
    private bool isAttacking = false;
    private bool isDead = false;
    private bool isCharging = false;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    private Vector3 aimDirection;
    private Vector3 targetPosition;
    private LineRenderer aimLine;

    void Start()
    {
        currentHealth = maxHealth;

        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;

        if (healthText != null)
        {
            healthText.text = Mathf.Round(currentHealth).ToString();
            healthText.fontSize = 0.8f;
        }

        if (meshRenderer != null)
            meshRenderer.material.color = new Color(0.9f, 0.7f, 0.2f);

        transform.localScale = Vector3.one * 2f;

        CreateAimLine();

        Debug.Log("!!! BAZYLISZEK POJAWIŁ SIĘ !!! HP: " + currentHealth);
    }

    void CreateAimLine()
    {
        GameObject lineObj = new GameObject("AimLine");
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        aimLine = lineObj.AddComponent<LineRenderer>();

        aimLine.startWidth = 0.1f;
        aimLine.endWidth = 0.1f;
        aimLine.positionCount = 2;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = Color.red;
        aimLine.endColor = Color.red;
        aimLine.enabled = false;
    }

    void UpdateAimLine()
    {
        if (aimLine == null || player == null) return;

        Vector3 startPos = transform.position + Vector3.up * 1.5f;
        Vector3 endPos = player.position + Vector3.up * 0.5f;

        aimLine.SetPosition(0, startPos);
        aimLine.SetPosition(1, endPos);

        float progress = laserTimer / laserCooldown;
        aimLine.startColor = Color.Lerp(Color.yellow, Color.red, progress);
        aimLine.endColor = Color.Lerp(Color.yellow, Color.red, progress);
    }

    void Update()
    {
        if (player == null || isDead || isCharging) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && distance > attackRange && !isAttacking)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
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

        if (!isAttacking)
        {
            laserTimer += Time.deltaTime;
            if (distance <= laserRange && laserTimer >= laserCooldown)
            {
                laserTimer = 0f;
                StartCoroutine(LaserAttack());
            }
        }

        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    void MeleeAttack()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Bazyliszek zadał {damage} obrażeń!");
            }
        }
    }

    IEnumerator LaserAttack()
    {
        isCharging = true;
        isAttacking = true;

        targetPosition = player.position;
        aimDirection = (targetPosition - transform.position).normalized;

        Debug.Log("Bazyliszek celuje...");

        if (aimLine != null)
        {
            aimLine.enabled = true;
            UpdateAimLine();
        }

        if (chargeEffect != null)
            chargeEffect.SetActive(true);

        if (meshRenderer != null)
            meshRenderer.material.color = Color.Lerp(Color.yellow, Color.red, 0.5f);

        float chargeTimer = 0f;
        while (chargeTimer < laserChargeTime)
        {
            chargeTimer += Time.deltaTime;

            if (aimLine != null) UpdateAimLine();

            if (meshRenderer != null)
            {
                float intensity = Mathf.PingPong(chargeTimer * 3f, 0.5f) + 0.5f;
                meshRenderer.material.color = Color.Lerp(Color.yellow, Color.red, intensity);
            }

            yield return null;
        }

        FireLaser();

        if (aimLine != null) aimLine.enabled = false;
        if (chargeEffect != null) chargeEffect.SetActive(false);
        if (meshRenderer != null) meshRenderer.material.color = originalColor;

        isCharging = false;
        isAttacking = false;
    }

    void FireLaser()
    {
        Debug.Log("!!! BAZYLISZEK STRZELA LASEREM !!!");

        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1.5f, Quaternion.LookRotation(aimDirection));
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
            laser.transform.localScale = new Vector3(0.3f, 0.3f, distanceToTarget);
            Destroy(laser, 0.3f);
        }

        float distanceToShot = Vector3.Distance(player.position, targetPosition);

        if (distanceToShot < 2f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(laserDamage);
                Debug.Log($"BAZYLISZEK TRAFIŁ gracza! Obrażenia: {laserDamage}");
            }
        }
        else
        {
            Debug.Log("Gracz uniknął lasera!");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        StartCoroutine(DamageFlash());

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position + Vector3.up * 1f, Quaternion.identity);

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        isDead = true;

        if (aimLine != null && aimLine.gameObject != null)
            Destroy(aimLine.gameObject);

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (levelSystem != null)
        {
            levelSystem.EnemyDied();
            for (int i = 0; i < 5; i++)
                levelSystem.EnemyDied();
        }

        Debug.Log("!!! BAZYLISZEK ZGINĄŁ !!!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, laserRange);
    }
}