using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Polnocnica : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 30f;
    public float moveSpeed = 2.5f;
    public float damage = 10f;

    [Header("Atak")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Efekty")]
    public GameObject deathEffect;

    [Header("UI")]
    public TextMeshPro healthText;

    private float currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private bool isDead = false;
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
            mesh.material.color = new Color(0.6f, 0.3f, 0.8f);
        }

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
    }

    void Update()
    {
        if (player == null || isDead) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
            agent.isStopped = dist <= agent.stoppingDistance;
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

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth -= amount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
        StartCoroutine(Flash());
        if (currentHealth <= 0) Die();
    }

    IEnumerator Flash()
    {
        if (mesh != null)
        {
            mesh.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            mesh.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null) levelSystem.EnemyDied();
        Destroy(gameObject);
    }
}