using UnityEngine;
using UnityEngine.AI;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;

    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (player != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
}