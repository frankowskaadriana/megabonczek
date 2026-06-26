using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Leszy : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 1200f;
    public float moveSpeed = 2f;
    public float damage = 45f;
    public int expReward = 500;

    [Header("Atak wręcz")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;

    [Header("Laser")]
    public GameObject laserPrefab;
    public float laserRange = 15f;
    public float laserCooldown = 4f;
    public float laserDamage = 50f;
    public float laserChargeTime = 1.5f;
    public float laserSpreadAngle = 15f;

    [Header("Odrzut po obrażeniach")]
    public float hitPushForce = 10f;
    public float hitPushUpForce = 0.5f;
    public float hitStunDuration = 0.3f;

    [Header("Efekty")]
    public GameObject deathEffect;
    public GameObject hitEffect;
    public GameObject chargeEffect;
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
    private LineRenderer aimLine;
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
        rb.mass = 200f;
        rb.isKinematic = true;
        rb.useGravity = false;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            originalColor = mesh.material.color;
            mesh.material.color = new Color(0.2f, 0.7f, 0.2f);
        }

        transform.localScale = Vector3.one * 2f;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        CreateAimLine();
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    void CreateAimLine()
    {
        GameObject line = new GameObject("AimLine");
        line.transform.SetParent(transform);
        aimLine = line.AddComponent<LineRenderer>();
        aimLine.startWidth = 0.1f;
        aimLine.endWidth = 0.1f;
        aimLine.positionCount = 2;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = Color.red;
        aimLine.endColor = Color.red;
        aimLine.enabled = false;
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
        if (dist <= laserRange && laserTimer >= laserCooldown)
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

        if (aimLine != null) aimLine.enabled = true;
        if (chargeEffect != null) chargeEffect.SetActive(true);

        float timer = 0f;
        while (timer < laserChargeTime)
        {
            timer += Time.deltaTime;
            if (aimLine != null)
            {
                Vector3 start = transform.position + Vector3.up * 1.5f;
                Vector3 end = player.position + Vector3.up * 0.5f;
                aimLine.SetPosition(0, start);
                aimLine.SetPosition(1, end);
            }
            yield return null;
        }

        FireThreeLasers(dir);

        if (aimLine != null) aimLine.enabled = false;
        if (chargeEffect != null) chargeEffect.SetActive(false);
        isCharging = false;
    }

    void FireThreeLasers(Vector3 dir)
    {
        if (laserPrefab == null) return;
        Quaternion left = Quaternion.Euler(0, -laserSpreadAngle, 0);
        Quaternion right = Quaternion.Euler(0, laserSpreadAngle, 0);

        FireLaser(left * dir);
        FireLaser(dir);
        FireLaser(right * dir);

        if (Vector3.Distance(player.position, transform.position) < 2f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(laserDamage);
        }
    }

    void FireLaser(Vector3 dir)
    {
        GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1.5f, Quaternion.LookRotation(dir));
        RaycastHit hit;
        float dist = laserRange;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir, out hit, laserRange))
            dist = hit.distance;
        laser.transform.localScale = new Vector3(0.2f, 0.2f, dist);
        Destroy(laser, 0.3f);
        AudioManager.Instance?.PlayLaser();
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