using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour
{
    [Header("═══════════════ ATAK ═══════════════")]
    public float damage = 10f;

    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1.5f;
    public float attackDelay = 0.5f;

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float hitPushForce = 5f;
    public float hitStunDuration = 0.2f;

    [Header("═══════════════ EFEKTY WIZUALNE ═══════════════")]
    public Color attackChargeColor = new Color(0f, 0.5f, 1f, 0.5f);
    public Color attackHitColor = new Color(1f, 0f, 0f, 0.6f);
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.15f;
    public float attackVisualDuration = 0.3f;
    public float visualHeight = 0.1f;

    [Header("═══════════════ NAWIGACJA ═══════════════")]
    public float pathUpdateInterval = 0.3f;
    public float stuckThreshold = 2f;
    public float angularSpeed = 720f;
    public float acceleration = 25f;

    private float currentHealth;
    private float maxHealth;
    private int expReward;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isAttacking = false;

    private List<MeshRenderer> allMeshes = new List<MeshRenderer>();
    private List<Color> originalColors = new List<Color>();

    private LevelSystem levelSystem;
    private Rigidbody rb;
    private Collider myCollider;

    private GameObject attackVisual;
    private LineRenderer visualLine;
    private Transform firePoint;

    private float pathUpdateTimer = 0f;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        gameObject.tag = "Enemy";
        myCollider = GetComponent<Collider>();
        IgnoreEnemyCollisions();

        // === POBIERZ STATYSTYKI Z LEVELSYSTEM ===
        levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            maxHealth = levelSystem.GetEnemyHealth();
            expReward = levelSystem.GetEnemyExpReward();
        }
        else
        {
            maxHealth = 30f;
            expReward = 10;
        }
        currentHealth = maxHealth;

        Debug.Log($"💀 {gameObject.name}: HP={maxHealth}, EXP={expReward}");

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        // === NAVMESHAGENT ===
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = 3f;
        agent.angularSpeed = angularSpeed;
        agent.acceleration = acceleration;
        agent.stoppingDistance = 0.3f;
        agent.autoBraking = false;
        agent.autoRepath = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.radius = 0.3f;
        agent.height = 1.8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.enabled = true;

        // === RIGIDBODY ===
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // === FIREPOINT ===
        Transform fp = transform.Find("FirePoint");
        if (fp != null) firePoint = fp;
        else
        {
            GameObject fpObj = new GameObject("FirePoint");
            fpObj.transform.SetParent(transform);
            fpObj.transform.localPosition = new Vector3(0, visualHeight, 0);
            firePoint = fpObj.transform;
        }

        CollectAllMeshRenderers();
        CreateAttackVisual();

        SetAllMeshesColor(Color.gray);

        lastPosition = transform.position;

        Debug.Log($"✅ {gameObject.name} gotowy! HP: {currentHealth}");
    }

    void CollectAllMeshRenderers()
    {
        allMeshes.Clear();
        originalColors.Clear();

        MeshRenderer mainMesh = GetComponent<MeshRenderer>();
        if (mainMesh != null)
        {
            allMeshes.Add(mainMesh);
            originalColors.Add(mainMesh.material.color);
        }

        MeshRenderer[] childrenMeshes = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer child in childrenMeshes)
        {
            if (child != mainMesh && !allMeshes.Contains(child))
            {
                allMeshes.Add(child);
                originalColors.Add(child.material.color);
            }
        }
    }

    void SetAllMeshesColor(Color color)
    {
        foreach (MeshRenderer mesh in allMeshes)
        {
            if (mesh != null) mesh.material.color = color;
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
        visualLine.sortingOrder = 10;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        if (mat == null) mat = new Material(Shader.Find("UI/Default"));
        mat.color = attackChargeColor;
        visualLine.material = mat;

        attackVisual.SetActive(false);
    }

    void ShowAttackVisual(float radius, Color color)
    {
        if (visualLine == null || firePoint == null) return;

        int points = 40;
        visualLine.positionCount = points;
        visualLine.loop = true;

        Vector3 center = firePoint.localPosition;

        for (int i = 0; i < points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            visualLine.SetPosition(i, new Vector3(x, center.y, z));
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
        if (myCollider == null) return;

        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        foreach (BaseEnemy enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != gameObject)
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

        if (agent != null && agent.isOnNavMesh && agent.enabled && !isAttacking)
        {
            pathUpdateTimer += Time.deltaTime;
            if (pathUpdateTimer >= pathUpdateInterval || agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                pathUpdateTimer = 0f;
                agent.SetDestination(player.position);
            }

            float speed = agent.velocity.magnitude;
            if (speed < 0.1f && dist > 2f)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckThreshold)
                {
                    agent.Warp(transform.position);
                    agent.SetDestination(player.position);
                    stuckTimer = 0f;
                    Debug.Log($"🔄 {gameObject.name}: Naprawa nawigacji!");
                }
            }
            else
            {
                stuckTimer = 0f;
            }

            if (dist <= attackRange * 0.5f && !isAttacking)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
            }

            if (dist > 0.5f)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }

            lastPosition = transform.position;
        }

        if (dist <= attackRange && !isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                StartCoroutine(AttackWithVisual());
            }
        }
        else if (dist > attackRange * 1.5f)
        {
            attackTimer = Mathf.Max(0, attackTimer - Time.deltaTime * 0.5f);
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
            Debug.Log($"⚔️ {gameObject.name} atakuje! {damage} obrażeń!");

            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 dir = (player.position - transform.position).normalized;
                dir.y = 0.5f;
                playerRb.AddForce(dir * 5f, ForceMode.Impulse);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"💥 {gameObject.name} otrzymał {amount} obrażeń! HP: {currentHealth}/{maxHealth}");

        StartCoroutine(FlashAllMeshes());
        StartCoroutine(HitPushback());

        if (currentHealth <= 0) Die();
    }

    IEnumerator FlashAllMeshes()
    {
        foreach (MeshRenderer mesh in allMeshes)
        {
            if (mesh != null) mesh.material.color = hitColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < allMeshes.Count && i < originalColors.Count; i++)
        {
            if (allMeshes[i] != null)
            {
                allMeshes[i].material.color = originalColors[i];
            }
        }
    }

    IEnumerator HitPushback()
    {
        if (isDead) yield break;

        if (player == null) yield break;

        Vector3 direction = (transform.position - player.position).normalized;
        direction.y = 0.5f;

        if (agent != null)
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
            if (agent.isOnNavMesh) agent.Warp(transform.position);
        }

        isStunned = false;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"💀 {gameObject.name} zginął! +{expReward} XP");

        if (levelSystem != null)
        {
            // Dodajemy EXP tyle razy ile wynosi nagroda (dzielone przez 10 dla balansu)
            for (int i = 0; i < expReward / 10; i++)
            {
                levelSystem.EnemyDied();
            }
        }

        WaveSpawner ws = FindFirstObjectByType<WaveSpawner>();
        if (ws != null) ws.EnemyDied();

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (attackVisual != null) Destroy(attackVisual);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}