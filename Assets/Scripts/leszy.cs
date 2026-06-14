using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Leszy : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI BOSSA ═══════════════")]
    public float maxHealth = 1200f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float damage = 45f;

    [Header("═══════════════ ATAK WRĘCZ ═══════════════")]
    public float attackRange = 2.5f;
    public float attackCooldown = 1.5f;

    [Header("═══════════════ ATAK LASERAMI ═══════════════")]
    public GameObject laserPrefab;
    public float laserRange = 15f;
    public float laserCooldown = 4f;
    public float laserDamage = 50f;
    public float laserChargeTime = 1.5f;
    public float laserSpreadAngle = 15f;

    [Header("═══════════════ TRAJEKTORIA ═══════════════")]
    public LineRenderer trajectoryLine;
    public Color trajectoryColor = new Color(1f, 0f, 0f, 0.8f);

    [Header("═══════════════ EFEKTY ═══════════════")]
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
    private Vector3 targetPosition;
    private AudioManager audioManager;
    private LineRenderer aimLine;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        audioManager = AudioManager.Instance;

        if (meshRenderer != null)
            meshRenderer.material.color = new Color(0.2f, 0.7f, 0.2f);

        transform.localScale = Vector3.one * 2f;

        CreateAimLine();
        CreateTrajectoryLine();

        Debug.Log($"🌲 LESZY GOTOWY!");
    }

    void CreateAimLine()
    {
        GameObject lineObj = new GameObject("AimLine");
        lineObj.transform.SetParent(transform);
        aimLine = lineObj.AddComponent<LineRenderer>();
        aimLine.startWidth = 0.1f;
        aimLine.endWidth = 0.1f;
        aimLine.positionCount = 2;
        aimLine.material = new Material(Shader.Find("Sprites/Default"));
        aimLine.startColor = Color.red;
        aimLine.endColor = Color.red;
        aimLine.enabled = false;
    }

    void CreateTrajectoryLine()
    {
        if (trajectoryLine == null)
        {
            GameObject trajObj = new GameObject("TrajectoryLine");
            trajObj.transform.SetParent(transform);
            trajectoryLine = trajObj.AddComponent<LineRenderer>();
        }
        trajectoryLine.startWidth = 0.15f;
        trajectoryLine.endWidth = 0.08f;
        trajectoryLine.positionCount = 2;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0.3f);
        trajectoryLine.enabled = false;
    }

    void ShowTrajectory(Vector3 direction)
    {
        if (trajectoryLine == null) return;
        Vector3 startPos = transform.position + Vector3.up * 1.5f;
        Vector3 endPos = startPos + direction * laserRange;
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
        trajectoryLine.enabled = true;
    }

    void HideTrajectory()
    {
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }

    void UpdateAimLine()
    {
        if (aimLine == null || player == null) return;
        Vector3 startPos = transform.position + Vector3.up * 1.5f;
        Vector3 endPos = player.position + Vector3.up * 0.5f;
        aimLine.SetPosition(0, startPos);
        aimLine.SetPosition(1, endPos);
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && distance > attackRange && !isAttacking && !isCharging)
        {
            agent.SetDestination(player.position);
        }

        if (distance <= attackRange && !isAttacking && !isCharging)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                MeleeAttack();
            }
        }

        if (!isAttacking && !isCharging && !isDead)
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
            if (playerHealth != null) playerHealth.TakeDamage(damage);
        }
    }

    IEnumerator LaserAttack()
    {
        isCharging = true;
        isAttacking = true;

        targetPosition = player.position;
        Vector3 direction = (targetPosition - transform.position).normalized;

        ShowTrajectory(direction);

        if (aimLine != null) aimLine.enabled = true;
        if (chargeEffect != null) chargeEffect.SetActive(true);

        float chargeTimer = 0f;
        while (chargeTimer < laserChargeTime)
        {
            chargeTimer += Time.deltaTime;
            if (aimLine != null) UpdateAimLine();
            yield return null;
        }

        HideTrajectory();
        FireThreeLasers();

        if (aimLine != null) aimLine.enabled = false;
        if (chargeEffect != null) chargeEffect.SetActive(false);

        isCharging = false;
        isAttacking = false;
    }

    void FireThreeLasers()
    {
        if (laserPrefab == null) return;

        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion leftRot = Quaternion.Euler(0, -laserSpreadAngle, 0);
        Quaternion rightRot = Quaternion.Euler(0, laserSpreadAngle, 0);

        FireSingleLaser(leftRot * direction);
        FireSingleLaser(direction);
        FireSingleLaser(rightRot * direction);

        if (Vector3.Distance(player.position, targetPosition) < 2f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(laserDamage);
        }
    }

    void FireSingleLaser(Vector3 direction)
    {
        GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1.5f, Quaternion.LookRotation(direction));
        RaycastHit hit;
        float distance = laserRange;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out hit, laserRange))
            distance = hit.distance;
        laser.transform.localScale = new Vector3(0.2f, 0.2f, distance);
        Destroy(laser, 0.3f);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null)
        {
            for (int i = 0; i < 5; i++)
                levelSystem.EnemyDied();
        }
        Destroy(gameObject);
    }
}