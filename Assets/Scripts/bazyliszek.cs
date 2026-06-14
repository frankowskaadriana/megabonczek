using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Bazyliszek : MonoBehaviour
{
    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 120f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float damage = 25f;

    [Header("═══════════════ ATAK WRĘCZ ═══════════════")]
    public float attackRange = 2f;
    public float attackCooldown = 1.2f;

    [Header("═══════════════ ATAK LASEREM ═══════════════")]
    public GameObject laserPrefab;
    public float laserRange = 12f;
    public float laserCooldown = 3f;
    public float laserDamage = 35f;
    public float laserChargeTime = 0.8f;

    [Header("═══════════════ TRAJEKTORIA ═══════════════")]
    public LineRenderer trajectoryLine;
    public Color trajectoryColor = new Color(1f, 0.5f, 0f, 0.8f);

    [Header("═══════════════ EFEKTY ═══════════════")]
    public GameObject deathEffect;
    public GameObject hitEffect;

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
            meshRenderer.material.color = new Color(0.9f, 0.7f, 0.2f);

        transform.localScale = Vector3.one * 1.2f;

        CreateTrajectoryLine();
        Debug.Log($"🐉 Bazyliszek pojawił się!");
    }

    void CreateTrajectoryLine()
    {
        if (trajectoryLine == null)
        {
            GameObject trajObj = new GameObject("TrajectoryLine");
            trajObj.transform.SetParent(transform);
            trajectoryLine = trajObj.AddComponent<LineRenderer>();
        }
        trajectoryLine.startWidth = 0.1f;
        trajectoryLine.endWidth = 0.05f;
        trajectoryLine.positionCount = 2;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0.3f);
        trajectoryLine.enabled = false;
    }

    void ShowTrajectory(Vector3 direction)
    {
        if (trajectoryLine == null) return;
        Vector3 startPos = transform.position + Vector3.up * 1f;
        Vector3 endPos = startPos + direction * laserRange;
        trajectoryLine.SetPosition(0, startPos);
        trajectoryLine.SetPosition(1, endPos);
        trajectoryLine.enabled = true;
    }

    void HideTrajectory()
    {
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }

    void Update()
    {
        if (player == null || isDead || isCharging) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && distance > attackRange && !isAttacking)
            agent.SetDestination(player.position);

        if (distance <= attackRange && !isAttacking)
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

        float chargeTimer = 0f;
        while (chargeTimer < laserChargeTime)
        {
            chargeTimer += Time.deltaTime;
            yield return null;
        }

        HideTrajectory();
        FireLaser(direction);

        isCharging = false;
        isAttacking = false;
    }

    void FireLaser(Vector3 direction)
    {
        if (laserPrefab != null)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position + Vector3.up * 1f, Quaternion.LookRotation(direction));
            RaycastHit hit;
            float distance = laserRange;
            if (Physics.Raycast(transform.position + Vector3.up * 1f, direction, out hit, laserRange))
                distance = hit.distance;
            laser.transform.localScale = new Vector3(0.15f, 0.15f, distance);
            Destroy(laser, 0.3f);
        }

        if (Vector3.Distance(player.position, targetPosition) < 2f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(laserDamage);
        }
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
        if (trajectoryLine != null && trajectoryLine.gameObject != null)
            Destroy(trajectoryLine.gameObject);
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null) levelSystem.EnemyDied();
        Destroy(gameObject);
    }
}