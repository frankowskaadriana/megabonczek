using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    [Header("═══════════════ ENEMY STATS ═══════════════")]
    public float health = 50f;
    public float moveSpeed = 3f;

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
            agent.SetDestination(player.position);

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