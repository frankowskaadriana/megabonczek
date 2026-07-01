using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 30f;
    public float moveSpeed = 3f;
    public float damage = 10f;
    public int expReward = 10;

    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 2f;           // ZWIĘKSZONE
    public float attackCooldown = 0.8f;       // SZYBSZE (z 1.5 na 0.8)
    public float attackDelay = 0.2f;          // SZYBSZE (z 0.5 na 0.2)
    public float attackAngle = 90f;           // KĄT ATAKU (stożek)

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float hitPushForce = 5f;
    public float hitStunDuration = 0.2f;

    [Header("═══════════════ EFEKTY WIZUALNE ATAKU ═══════════════")]
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
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isStunned = false;
    private bool isAttacking = false;

    private List<MeshRenderer> allMeshes = new List<MeshRenderer>();
    private List<Color> originalColors = new List<Color>();
    private List<bool> hasColorProperty = new List<bool>();

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

        levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            maxHealth = levelSystem.GetEnemyHealth();
            expReward = levelSystem.GetEnemyExpReward();
        }
        currentHealth = maxHealth;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
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

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

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

        Debug.Log($"✅ {gameObject.name} gotowy! HP: {currentHealth}, Atak co {attackCooldown}s");
    }

    void CollectAllMeshRenderers()
    {
        allMeshes.Clear();
        originalColors.Clear();
        hasColorProperty.Clear();

        MeshRenderer mainMesh = GetComponent<MeshRenderer>();
        if (mainMesh != null) AddMeshRenderer(mainMesh);

        MeshRenderer[] childrenMeshes = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer child in childrenMeshes)
        {
            if (child != mainMesh && !allMeshes.Contains(child))
            {
                AddMeshRenderer(child);
            }
        }
    }

    void AddMeshRenderer(MeshRenderer mesh)
    {
        if (mesh == null) return;
        allMeshes.Add(mesh);

        bool hasColor = false;
        Color color = Color.white;

        try
        {
            if (mesh.material != null && mesh.material.HasProperty("_Color"))
            {
                color = mesh.material.color;
                hasColor = true;
            }
            else if (mesh.material != null && mesh.material.HasProperty("_BaseColor"))
            {
                color = mesh.material.GetColor("_BaseColor");
                hasColor = true;
            }
            else if (mesh.material != null && mesh.material.HasProperty("_MainColor"))
            {
                color = mesh.material.GetColor("_MainColor");
                hasColor = true;
            }
            else
            {
                color = Color.white;
                hasColor = false;
            }
        }
        catch { color = Color.white; hasColor = false; }

        originalColors.Add(color);
        hasColorProperty.Add(hasColor);
    }

    void SetAllMeshesColor(Color color)
    {
        for (int i = 0; i < allMeshes.Count; i++)
        {
            MeshRenderer mesh = allMeshes[i];
            if (mesh == null || mesh.material == null) continue;

            try
            {
                if (mesh.material.HasProperty("_Color"))
                    mesh.material.color = color;
                else if (mesh.material.HasProperty("_BaseColor"))
                    mesh.material.SetColor("_BaseColor", color);
                else if (mesh.material.HasProperty("_MainColor"))
                    mesh.material.SetColor("_MainColor", color);
            }
            catch { }
        }
    }

    void CreateAttackVisual()
    {
        attackVisual = new GameObject("AttackVisual");
        attackVisual.transform.SetParent(transform);
        attackVisual.transform.localPosition = Vector3.zero;
        attackVisual.transform.localRotation = Quaternion.identity;

        visualLine = attackVisual.AddComponent<LineRenderer>();
        visualLine.useWorldSpace = false;
        visualLine.loop = false;
        visualLine.sortingOrder = 10;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        if (mat == null) mat = new Material(Shader.Find("UI/Default"));
        mat.color = attackChargeColor;
        visualLine.material = mat;

        attackVisual.SetActive(false);
    }

    void ShowAttackCone(float range, float angle, Color color)
    {
        if (visualLine == null || firePoint == null) return;

        float halfAngle = angle / 2f;
        int points = 30;
        visualLine.positionCount = points + 3;
        visualLine.loop = false;

        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;

        visualLine.SetPosition(0, center);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        visualLine.SetPosition(1, center + leftDir * range);

        int pointIndex = 2;
        for (int i = 1; i <= points; i++)
        {
            float t = (float)i / points;
            float currentAngle = -halfAngle + (angle * t);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 point = center + dir * range;
            visualLine.SetPosition(pointIndex++, point);
        }

        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        visualLine.SetPosition(visualLine.positionCount - 1, center + rightDir * range);

        visualLine.startColor = color;
        visualLine.endColor = new Color(color.r, color.g, color.b, 0.3f);
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

        // Pokaż NIEBIESKI stożek (przygotowanie)
        ShowAttackCone(attackRange, attackAngle, attackChargeColor);
        yield return new WaitForSeconds(attackDelay);

        // Zmień na CZERWONY stożek (atak)
        ShowAttackCone(attackRange, attackAngle, attackHitColor);
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

        // Sprawdź czy gracz jest w stożku ataku
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle <= attackAngle / 2)
        {
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
        for (int i = 0; i < allMeshes.Count; i++)
        {
            MeshRenderer mesh = allMeshes[i];
            if (mesh == null || mesh.material == null) continue;

            try
            {
                if (hasColorProperty[i])
                {
                    if (mesh.material.HasProperty("_Color"))
                        mesh.material.color = hitColor;
                    else if (mesh.material.HasProperty("_BaseColor"))
                        mesh.material.SetColor("_BaseColor", hitColor);
                }
            }
            catch { }
        }

        yield return new WaitForSeconds(hitFlashDuration);

        for (int i = 0; i < allMeshes.Count; i++)
        {
            MeshRenderer mesh = allMeshes[i];
            if (mesh == null || mesh.material == null) continue;

            try
            {
                if (hasColorProperty[i] && i < originalColors.Count)
                {
                    if (mesh.material.HasProperty("_Color"))
                        mesh.material.color = originalColors[i];
                    else if (mesh.material.HasProperty("_BaseColor"))
                        mesh.material.SetColor("_BaseColor", originalColors[i]);
                }
            }
            catch { }
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
        // Zasięg ataku
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Stożek ataku
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * attackRange);

        // Łuk
        int points = 20;
        for (int i = 0; i <= points; i++)
        {
            float t = (float)i / points;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = transform.position + dir * attackRange;

            if (i > 0)
            {
                float prevAngle = -halfAngle + (attackAngle * ((float)(i - 1) / points));
                Vector3 prevDir = Quaternion.Euler(0, prevAngle, 0) * forward;
                Vector3 prevPoint = transform.position + prevDir * attackRange;
                Gizmos.DrawLine(prevPoint, point);
            }
        }

        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}