using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class enemyHealth : MonoBehaviour
{
    public float health = 50f;
    public LevelSystem levelSystem;
    public TextMeshPro healthText;
    public float moveSpeed = 3f;

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
        agent.autoBraking = true;
        agent.autoRepath = true;

        // Ustaw agenta na ziemiê jeœli jest nad navmesh
        if (!agent.isOnNavMesh)
        {
            // Spróbuj przykleiæ do navmesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                Debug.Log(gameObject.name + " przyklejony do NavMesh");
            }
            else
            {
                Debug.LogWarning(gameObject.name + " NIE jest na NavMesh! Sprawdz czy NavMesh jest wygenerowany.");
            }
        }
        else
        {
            Debug.Log(gameObject.name + " jest na NavMesh");
        }

        if (healthText != null) healthText.text = Mathf.Round(health).ToString();
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh && agent.enabled)
        {
            agent.SetDestination(player.position);
        }

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