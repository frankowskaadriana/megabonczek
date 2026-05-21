using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    [Header("═══════════════ ENEMY STATS ═══════════════")]
    public float health = 50f;
    public float moveSpeed = 3f;
    public float damage = 20f;
    public float attackCooldown = 1f;

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public TextMeshPro healthText;

    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;

    void Start()
    {
        if (levelSystem == null)
            levelSystem = FindFirstObjectByType<LevelSystem>();

        if (levelSystem != null)
            health = 50f + (levelSystem.currentLevel - 1) * 10f;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = 1.5f;

        if (healthText != null) healthText.text = Mathf.Round(health).ToString();

        Debug.Log($"Enemy spawned! Health: {health}, Damage: {damage}");
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);

            // Atakuj gracza gdy blisko
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= 1.8f)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    AttackPlayer();
                }
            }
        }

        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    void AttackPlayer()
    {
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log($"Enemy atakuje! Obrażenia: {damage}");
                playerHealth.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
        Debug.Log($"Enemy otrzymał {damage} obrażeń. Pozostałe HP: {health}");

        if (health <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        if (levelSystem != null) levelSystem.EnemyDied();
        Destroy(gameObject);
    }
}