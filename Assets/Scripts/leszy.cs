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

    [Header("Efekty")]
    public GameObject deathEffect;
    public GameObject hitEffect;
    public GameObject chargeEffect;

    [Header("UI")]
    public TextMeshPro healthText;

    private float currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private float laserTimer = 0f;
    private bool isDead = false;
    private bool isCharging = false;
    private MeshRenderer mesh;
    private Color originalColor;
    private LevelSystem levelSystem;
    private LineRenderer aimLine;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        player = GameObject.FindWithTag("Player")?.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

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
        if (player == null || isDead || isCharging) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (agent != null && agent.isOnNavMesh && dist > attackRange)
            agent.SetDestination(player.position);

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
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
        if (hitEffect != null) Instantiate(hitEffect, transform.position + Vector3.up, Quaternion.identity);
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null)
        {
            for (int i = 0; i < 5; i++) levelSystem.EnemyDied();
        }
        Destroy(gameObject);
    }
}