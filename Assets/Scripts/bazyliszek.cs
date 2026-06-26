using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Bazyliszek : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 120f;
    public float moveSpeed = 2f;
    public float damage = 25f;
    public int expReward = 50;

    [Header("Atak wręcz")]
    public float attackRange = 2f;
    public float attackCooldown = 1.2f;

    [Header("Laser")]
    public GameObject laserPrefab;
    public float laserRange = 12f;
    public float laserCooldown = 3f;
    public float laserDamage = 35f;
    public float laserChargeTime = 0.8f;

    [Header("Odrzut po obrażeniach")]
    public float hitPushForce = 8f;
    public float hitPushUpForce = 0.5f;
    public float hitStunDuration = 0.3f;

    [Header("Efekty")]
    public GameObject deathEffect;
    public GameObject hitEffect;
    public Color hitColor = Color.white;
    public float hitFlashDuration = 0.1f;

    [Header("UI")]
    public TextMeshPro healthText;

    private float currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private float laserTimer = 0f;
    private bool isDead = false;
    private bool isCharging = false;
    private bool isStunned = false;
    private MeshRenderer mesh;
    private Color originalColor;
    private LevelSystem levelSystem;
    private Rigidbody rb;
    private Coroutine flashCoroutine;
    private Coroutine pushbackCoroutine;
    private float searchTimer = 0f;
    private float searchInterval = 0.5f;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        FindPlayer();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = 360f;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 80f;
        rb.isKinematic = true;
        rb.useGravity = false;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            originalColor = mesh.material.color;
            mesh.material.color = new Color(0.9f, 0.7f, 0.2f);
        }

        transform.localScale = Vector3.one * 1.2f;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null || isDead || isCharging || isStunned) return;

        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            if (player == null) FindPlayer();
            if (player == null) return;
        }

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && agent.enabled)
        {
            if (dist > attackRange)
            {
                agent.SetDestination(player.position);
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                }
            }
        }

        if (dist <= attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                MeleeAttack();
            }
        }

        laserTimer += Time.deltaTime;
        if (dist <= laserRange && laserTimer >= laserCooldown && !isCharging)
        {
            laserTimer = 0f;
            StartCoroutine(LaserAttack());
        }

        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    void MeleeAttack()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange * 1.2f) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(damage);
    }

    IEnumerator LaserAttack()
    {
        isCharging = true;
        Vector3 target = player.position;
        Vector3 dir = (target - transform.position).normalized;

        yield return new WaitForSeconds(laserChargeTime);

        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1f, Quaternion.LookRotation(dir));
            RaycastHit hit;
            float dist = laserRange;
            if (Physics.Raycast(transform.position + Vector3.up * 1f, dir, out hit, laserRange))
                dist = hit.distance;
            laser.transform.localScale = new Vector3(0.15f, 0.15f, dist);
            Destroy(laser, 0.3f);
            AudioManager.Instance?.PlayLaser();
        }

        if (Vector3.Distance(player.position, target) < 2f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(laserDamage);
        }

        isCharging = false;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        FlashHit();
        AudioManager.Instance?.PlayEnemyHit();
        if (hitEffect != null) Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);

        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);
        pushbackCoroutine = StartCoroutine(HitPushback());

        if (currentHealth <= 0) Die();
    }

    public void FlashHit()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashHitCoroutine());
    }

    IEnumerator FlashHitCoroutine()
    {
        if (mesh != null)
        {
            mesh.material.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            mesh.material.color = originalColor;
        }
        flashCoroutine = null;
    }

    IEnumerator HitPushback()
    {
        if (isDead) yield break;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) yield break;

        Vector3 direction = (transform.position - playerObj.transform.position).normalized;
        direction.y = hitPushUpForce;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * hitPushForce, ForceMode.Impulse);
        }

        isStunned = true;

        yield return new WaitForSeconds(hitStunDuration);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
            }
        }

        isStunned = false;
        pushbackCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        AudioManager.Instance?.PlayEnemyDeath();

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null) levelSystem.EnemyDied();

        WaveSpawner waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null) waveSpawner.EnemyDied();

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
    }
}