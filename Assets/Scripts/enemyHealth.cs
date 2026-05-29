using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    [Header("═══════════════ ENEMY STATS ═══════════════")]
    public float health = 50f;
    public float moveSpeed = 3f;
<<<<<<< Updated upstream
=======
<<<<<<< HEAD
<<<<<<< Updated upstream
=======
<<<<<<< Updated upstream
    public float damage = 20f;
    public float attackCooldown = 1f;
=======
>>>>>>> Stashed changes
>>>>>>> Stashed changes
=======
>>>>>>> origin/mati
>>>>>>> Stashed changes

    [Header("═══════════════ REFERENCES ═══════════════")]
    public LevelSystem levelSystem;
    public TextMeshPro healthText;

    private Transform player;
    private NavMeshAgent agent;

    void Start()
    {
        if (levelSystem == null) levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null) health = 50f + (levelSystem.currentLevel - 1) * 10f;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = 1.5f;

        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh)
<<<<<<< Updated upstream
            agent.SetDestination(player.position);

=======
<<<<<<< HEAD
<<<<<<< Updated upstream
            agent.SetDestination(player.position);

=======
<<<<<<< Updated upstream
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
=======
            agent.SetDestination(player.position);
>>>>>>> Stashed changes

>>>>>>> Stashed changes
=======
            agent.SetDestination(player.position);

>>>>>>> origin/mati
>>>>>>> Stashed changes
        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
        if (health <= 0) Die();
    }

    void Die()
    {
        if (levelSystem != null) levelSystem.EnemyDied();
        Destroy(gameObject);
    }
}