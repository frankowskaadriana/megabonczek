using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class enemyBehaviour : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 5f;

    [Header("Push Settings")]
    public float pushForce = 10f;
    public float timeToPush = 2f;

    [Header("NavMesh Settings")]
    public float stoppingDistance = 1.5f;

    private NavMeshAgent agent;
    private Transform player;
    private float collisionTimer = 0f;
    private GameObject currentCollidingEnemy = null;

    // Cache dla wydajnoœci
    private WaitForSeconds updateDelay;
    private bool isPushing = false;

    void Awake()
    {
        // Inicjalizacja w Awake dla lepszej wydajnoœci
        updateDelay = new WaitForSeconds(0.25f); // Rzadziej aktualizuj cel
    }

    void Start()
    {
        InitializeNavMeshAgent();
        FindPlayer();

        // Rozpocznij coroutine do aktualizacji celu
        StartCoroutine(UpdateTargetRoutine());
    }

    void InitializeNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
        }

        agent.speed = moveSpeed;
        agent.angularSpeed = rotationSpeed * 20f;
        agent.stoppingDistance = stoppingDistance;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updateRotation = false; // Wy³¹cz automatyczn¹ rotacjê NavMeshAgent
        agent.updateUpAxis = false;
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure Player has tag 'Player'");
        }
    }

    void Update()
    {
        if (!IsValid()) return;

        UpdateDestination();
        RotateTowardsPlayer();
    }

    bool IsValid()
    {
        return agent != null && player != null && agent.isActiveAndEnabled && !isPushing;
    }

    void UpdateDestination()
    {
        // Ustaw cel tylko jeœli agent jest na NavMesh
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(player.position);
        }
    }

    void RotateTowardsPlayer()
    {
        // Obracanie w stronê gracza (tylko oœ Y)
        Vector3 direction = player.position - transform.position;
        direction.y = 0; // Ignoruj oœ Y, obracaj tylko w poziomie

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    IEnumerator UpdateTargetRoutine()
    {
        while (true)
        {
            if (agent != null && player != null && agent.isActiveAndEnabled && !isPushing)
            {
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(player.position);
                }
            }
            yield return updateDelay;
        }
    }

    // Kolizja zoptymalizowana
    void OnCollisionEnter(Collision collision)
    {
        if (!isPushing && collision.gameObject.CompareTag("Enemy"))
        {
            currentCollidingEnemy = collision.gameObject;
            collisionTimer = 0f;
            StartCoroutine(PushCoroutine());
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (!isPushing && collision.gameObject.CompareTag("Enemy") && currentCollidingEnemy != null)
        {
            collisionTimer += Time.deltaTime;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ResetCollisionState();
        }
    }

    void ResetCollisionState()
    {
        currentCollidingEnemy = null;
        collisionTimer = 0f;
        isPushing = false;
        StopCoroutine(PushCoroutine());
    }

    IEnumerator PushCoroutine()
    {
        while (collisionTimer < timeToPush && currentCollidingEnemy != null)
        {
            yield return null;
        }

        if (collisionTimer >= timeToPush && currentCollidingEnemy != null && !isPushing)
        {
            PushAway(currentCollidingEnemy);
        }
    }

    void PushAway(GameObject otherEnemy)
    {
        isPushing = true;

        Vector3 randomDirection = GetRandomDirection();

        // Odepchnij obu wrogów
        PushEnemy(otherEnemy, randomDirection);
        PushEnemy(gameObject, -randomDirection);

        ResetCollisionState();

        // Przywróæ agentów po odepchniêciu
        StartCoroutine(RestoreAgentAfterPush(otherEnemy));
    }

    Vector3 GetRandomDirection()
    {
        return new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
    }

    void PushEnemy(GameObject enemy, Vector3 direction)
    {
        NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>();
        if (enemyAgent != null)
        {
            enemyAgent.enabled = false;
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = enemy.AddComponent<Rigidbody>();
                rb.useGravity = true;
            }
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(direction * pushForce, ForceMode.Impulse);
        }
    }

    IEnumerator RestoreAgentAfterPush(GameObject otherEnemy)
    {
        yield return new WaitForSeconds(0.5f);

        RestoreAgentComponent(gameObject);
        RestoreAgentComponent(otherEnemy);

        isPushing = false;
    }

    void RestoreAgentComponent(GameObject enemy)
    {
        if (enemy == null) return;

        NavMeshAgent navAgent = enemy.GetComponent<NavMeshAgent>();
        Rigidbody rb = enemy.GetComponent<Rigidbody>();

        if (navAgent != null)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Destroy(rb); // Usuñ Rigidbody po odepchniêciu
            }
            navAgent.enabled = true;
            navAgent.ResetPath();
        }
    }

    // Metoda do wizualizacji w edytorze
    void OnDrawGizmosSelected()
    {
        if (agent != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, agent.stoppingDistance);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}