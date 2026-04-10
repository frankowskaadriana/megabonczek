using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SimpleEnemy : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float stoppingDistance = 1.5f;
    public float updateInterval = 0.5f; // Czas miêdzy aktualizacjami pozycji

    private NavMeshAgent agent;
    private Transform player;
    private Coroutine updateDestinationCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        player = GameObject.FindWithTag("Player").transform;

        // Uruchom coroutine do odœwie¿ania pozycji
        if (player != null)
        {
            updateDestinationCoroutine = StartCoroutine(UpdateDestinationRoutine());
        }
    }

    IEnumerator UpdateDestinationRoutine()
    {
        while (true)
        {
            if (player != null && agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void OnDestroy()
    {
        // Zatrzymaj coroutine gdy obiekt jest niszczony
        if (updateDestinationCoroutine != null)
        {
            StopCoroutine(updateDestinationCoroutine);
        }
    }
}