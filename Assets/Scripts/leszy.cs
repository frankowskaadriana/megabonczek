using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Leszy : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 1200f;
    public float moveSpeed = 2f;
    public float damage = 45f;
    public int expReward = 500;

    [Header("═══════════════ ATAK WRĘCZ ═══════════════")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.8f;
    public float attackDelay = 0.6f;

    [Header("═══════════════ LASER ═══════════════")]
    public GameObject laserPrefab;
    public float laserRange = 15f;
    public float laserCooldown = 4f;
    public float laserDamage = 50f;
    public float laserChargeTime = 1.5f;
    public float laserSpreadAngle = 15f;

    [Header("═══════════════ EFEKTY WIZUALNE ═══════════════")]
    public Color attackChargeColor = new Color(0f, 0.5f, 1f, 0.3f);
    public Color attackHitColor = new Color(1f, 0f, 0f, 0.4f);
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.15f;
    public float attackVisualDuration = 0.4f;

    private float currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private float laserTimer = 0f;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isAttacking = false;
    private bool isChargingLaser = false;

    private MeshRenderer mainMesh;
    private List<MeshRenderer> childMeshes = new List<MeshRenderer>();
    private List<Color> originalColors = new List<Color>();

    private LevelSystem levelSystem;
    private Rigidbody rb;
    private GameObject attackVisual;
    private LineRenderer visualLine;

    // ============================================================
    // !!! NOWE: Flaga informująca AudioManager że Leszy żyje !!!
    // ============================================================
    private bool bossMusicStarted = false;

    void Start()
    {
        gameObject.tag = "Enemy";
        IgnoreEnemyCollisions();

        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.5f;
        agent.autoBraking = true;
        agent.enabled = true;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 200f;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        CollectAllMeshRenderers();
        transform.localScale = Vector3.one * 1.8f;
        CreateAttackVisual();

        // ============================================================
        // !!! NOWE: Włącz muzykę bossa gdy Leszy się pojawi !!!
        // ============================================================
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnBossSpawn();
            bossMusicStarted = true;
            Debug.Log("🎵 Muzyka bossa włączona!");
        }

        Debug.Log($"🌲 Leszy (BOSS) gotowy! HP: {currentHealth}, Znaleziono {childMeshes.Count} meshów");
    }

    void CollectAllMeshRenderers()
    {
        childMeshes.Clear();
        originalColors.Clear();

        mainMesh = GetComponent<MeshRenderer>();
        if (mainMesh != null)
        {
            childMeshes.Add(mainMesh);
            originalColors.Add(mainMesh.material.color);
        }

        MeshRenderer[] children = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer child in children)
        {
            if (child != mainMesh)
            {
                childMeshes.Add(child);
                originalColors.Add(child.material.color);
            }
        }
    }

    void CreateAttackVisual()
    {
        attackVisual = new GameObject("AttackVisual");
        attackVisual.transform.SetParent(transform);
        attackVisual.transform.localPosition = Vector3.zero;
        attackVisual.transform.localRotation = Quaternion.identity;

        visualLine = attackVisual.AddComponent<LineRenderer>();
        visualLine.startWidth = 0.1f;
        visualLine.endWidth = 0.1f;
        visualLine.useWorldSpace = false;
        visualLine.loop = true;
        visualLine.sortingOrder = -1;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = attackChargeColor;
        visualLine.material = mat;
        visualLine.startColor = attackChargeColor;
        visualLine.endColor = attackChargeColor;

        attackVisual.SetActive(false);
    }

    void ShowAttackVisual(float radius, Color color)
    {
        if (visualLine == null) return;

        int points = 40;
        visualLine.positionCount = points;
        visualLine.loop = true;

        for (int i = 0; i < points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            visualLine.SetPosition(i, new Vector3(x, 0.02f, z));
        }

        visualLine.startColor = color;
        visualLine.endColor = color;
        visualLine.material.color = color;

        attackVisual.SetActive(true);
    }

    void HideAttackVisual()
    {
        if (attackVisual != null) attackVisual.SetActive(false);
    }

    void IgnoreEnemyCollisions()
    {
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in taggedEnemies)
        {
            if (enemy != null && enemy != gameObject)
            {
                Collider otherCollider = enemy.GetComponent<Collider>();
                if (otherCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, otherCollider, true);
                }
            }
        }
    }

    void Update()
    {
        if (isDead || isStunned || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && agent.enabled && !isAttacking && !isChargingLaser)
        {
            agent.SetDestination(player.position);

            if (dist > attackRange * 0.8f)
            {
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

        if (dist <= attackRange && !isAttacking && !isChargingLaser)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                StartCoroutine(AttackWithVisual());
            }
        }

        laserTimer += Time.deltaTime;
        if (dist <= laserRange && laserTimer >= laserCooldown && !isChargingLaser && !isAttacking)
        {
            laserTimer = 0f;
            StartCoroutine(LaserAttack());
        }
    }

    IEnumerator AttackWithVisual()
    {
        isAttacking = true;

        if (agent != null) agent.isStopped = true;

        ShowAttackVisual(attackRange, attackChargeColor);
        yield return new WaitForSeconds(attackDelay);

        ShowAttackVisual(attackRange, attackHitColor);
        MeleeAttack();

        yield return new WaitForSeconds(attackVisualDuration);
        HideAttackVisual();

        isAttacking = false;
        if (agent != null) agent.isStopped = false;
    }

    void MeleeAttack()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange * 1.2f) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            Debug.Log($"🌲 Leszy atakuje wręcz! {damage} obrażeń!");
        }
    }

    IEnumerator LaserAttack()
    {
        isChargingLaser = true;

        ShowAttackVisual(laserRange * 0.25f, attackChargeColor);

        yield return new WaitForSeconds(laserChargeTime);

        Vector3 target = player.position;
        Vector3 dir = (target - transform.position).normalized;

        ShowAttackVisual(laserRange * 0.25f, attackHitColor);

        Quaternion leftRot = Quaternion.Euler(0, -laserSpreadAngle, 0);
        Quaternion rightRot = Quaternion.Euler(0, laserSpreadAngle, 0);

        FireLaser(leftRot * dir);
        FireLaser(dir);
        FireLaser(rightRot * dir);

        AudioManager.Instance?.PlayLaser();

        if (Vector3.Distance(player.position, target) < 2f)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(laserDamage);
            Debug.Log($"🌲 Leszy laser! {laserDamage} obrażeń!");
        }

        yield return new WaitForSeconds(attackVisualDuration);
        HideAttackVisual();

        isChargingLaser = false;
    }

    void FireLaser(Vector3 direction)
    {
        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1.5f, Quaternion.LookRotation(direction));
            Destroy(laser, 0.3f);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"💥 Leszy otrzymał {amount} obrażeń! HP: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashAllMeshes());

        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashAllMeshes()
    {
        foreach (MeshRenderer mesh in childMeshes)
        {
            if (mesh != null) mesh.material.color = hitColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < childMeshes.Count && i < originalColors.Count; i++)
        {
            if (childMeshes[i] != null)
            {
                childMeshes[i].material.color = originalColors[i];
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 Leszy (BOSS) zginął!");

        // ============================================================
        // !!! NOWE: Wyłącz muzykę bossa gdy Leszy umiera !!!
        // ============================================================
        if (AudioManager.Instance != null && bossMusicStarted)
        {
            AudioManager.Instance.OnBossDeath();
            bossMusicStarted = false;
            Debug.Log("🎵 Muzyka bossa wyłączona!");
        }

        if (levelSystem != null) levelSystem.EnemyDied();

        WaveSpawner ws = FindFirstObjectByType<WaveSpawner>();
        if (ws != null) ws.EnemyDied();

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (attackVisual != null) Destroy(attackVisual);
    }
}