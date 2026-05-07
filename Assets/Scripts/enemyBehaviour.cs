using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    public float updateInterval = 0.5f;

    private NavMeshAgent agent;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        // Sprawdü czy gracz istnieje
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Nie znaleziono gracza z tagiem 'Player'!");
        }
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }
}