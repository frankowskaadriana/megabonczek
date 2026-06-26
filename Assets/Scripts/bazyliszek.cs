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

    [Header("Atak wręcz")]
    public float attackRange = 2f;
    public float attackCooldown = 1.2f;

    [Header("Laser")]
    public GameObject laserPrefab;
    public float laserRange = 12f;
    public float laserCooldown = 3f;
    public float laserDamage = 35f;
    public float laserChargeTime = 0.8f;

    [Header("Efekty")]
    public GameObject deathEffect;
    public GameObject hitEffect;

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
            mesh.material.color = new Color(0.9f, 0.7f, 0.2f);
        }

        transform.localScale = Vector3.one * 1.2f;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
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