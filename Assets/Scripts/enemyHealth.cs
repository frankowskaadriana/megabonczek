using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class enemyHealth : MonoBehaviour
{
    [Header("═══════════════ TYP PRZECIWNIKA ═══════════════")]
    public EnemyType enemyType = EnemyType.Polnocnica;

    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float damage = 20f;
    public int expReward = 10;

    [Header("═══════════════ ATAK WRĘCZ ═══════════════")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1f;

    [Header("═══════════════ ODEPCHNIĘCIE PRZECIWNIKA ═══════════════")]
    public float pushForce = 3f;
    public float pushUpForce = 0.5f;
    public float pushRadius = 1.5f;

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public LevelSystem levelSystem;
    public TextMeshPro healthText;

    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private bool isDead = false;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private Rigidbody enemyRigidbody;
    private AudioManager audioManager;
    private float stuckTimer = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        currentHealth = maxHealth;

        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
        agent.autoBraking = false;
        agent.autoRepath = true;
        agent.updateRotation = true;
        agent.updatePosition = true;
        agent.angularSpeed = 360f;
        agent.acceleration = 100f;

        enemyRigidbody = GetComponent<Rigidbody>();
        if (enemyRigidbody == null) enemyRigidbody = gameObject.AddComponent<Rigidbody>();
        enemyRigidbody.isKinematic = true;
        enemyRigidbody.useGravity = false;

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) originalColor = meshRenderer.material.color;

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        audioManager = AudioManager.Instance;
        lastPosition = transform.position;

        ApplyEnemyVisuals();
    }

    void ApplyEnemyVisuals()
    {
        if (meshRenderer == null) return;

        switch (enemyType)
        {
            case EnemyType.Polnocnica:
                meshRenderer.material.color = new Color(0.6f, 0.3f, 0.8f);
                expReward = 10;
                break;
            case EnemyType.Strzyga:
                meshRenderer.material.color = new Color(0.5f, 0.2f, 0.1f);
                expReward = 15;
                break;
            case EnemyType.Upior:
                meshRenderer.material.color = new Color(0.4f, 0.6f, 0.3f);
                expReward = 20;
                break;
            case EnemyType.Leszy:
                meshRenderer.material.color = new Color(0.2f, 0.7f, 0.2f);
                transform.localScale = Vector3.one * 1.8f;
                expReward = 100;
                break;
        }

        if (healthText != null)
            healthText.text = Mathf.Round(currentHealth).ToString();
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);

            if (distance > agent.stoppingDistance)
                agent.isStopped = false;
            else
                agent.isStopped = true;
        }

        if (distance <= attackRange)
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
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                if (audioManager != null) audioManager.PlayEnemyAttack();

                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < pushRadius)
                {
                    Rigidbody playerRb = player.GetComponent<Rigidbody>();
                    if (playerRb != null)
                    {
                        Vector3 direction = (player.position - transform.position).normalized;
                        direction.y = 0.5f;
                        playerRb.AddForce(direction * pushForce * 0.5f, ForceMode.Impulse);
                    }
                }
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        StartCoroutine(DamageFlash());
        if (audioManager != null) audioManager.PlayEnemyHit();

        if (currentHealth <= 0) Die();
    }

    IEnumerator DamageFlash()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            meshRenderer.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (audioManager != null) audioManager.PlayEnemyDeath();

        if (levelSystem != null) levelSystem.EnemyDied();

        Destroy(gameObject);
    }
}