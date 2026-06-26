using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum SheepState
{
    Idle,
    Following,
    Charging,
    Attacking,
    Dead
}

public class Sheep : MonoBehaviour
{
    [Header("Statystyki")]
    public float speed = 5f;
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 1f;

    private NavMeshAgent agent;
    private Transform targetEnemy;
    private Vector3 targetPosition;
    private SheepState state = SheepState.Idle;
    private float attackTimer = 0f;
    private bool isDead = false;
    private ShepherdAbilities owner;
    private MeshRenderer mesh;
    private Color originalColor;
    private float health = 30f;
    private float maxHealth = 30f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = attackRange;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            originalColor = mesh.material.color;
            mesh.material.color = Color.white;
        }

        StartCoroutine(StateMachine());
    }

    IEnumerator StateMachine()
    {
        while (!isDead)
        {
            switch (state)
            {
                case SheepState.Idle:
                    yield return StartCoroutine(IdleState());
                    break;
                case SheepState.Following:
                    yield return StartCoroutine(FollowingState());
                    break;
                case SheepState.Charging:
                    yield return StartCoroutine(ChargingState());
                    break;
                case SheepState.Attacking:
                    yield return StartCoroutine(AttackingState());
                    break;
            }
            yield return null;
        }
    }

    IEnumerator IdleState()
    {
        while (state == SheepState.Idle && !isDead)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator FollowingState()
    {
        while (state == SheepState.Following && !isDead)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(targetPosition);
                agent.isStopped = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ChargingState()
    {
        float chargeTime = 2f;
        float startSpeed = speed;
        agent.speed = speed * 2f;

        while (state == SheepState.Charging && !isDead && chargeTime > 0)
        {
            chargeTime -= Time.deltaTime;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(targetPosition);
                agent.isStopped = false;
            }

            if (Vector3.Distance(transform.position, targetPosition) < 1f)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange * 1.5f);
                foreach (var hit in hitColliders)
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage * 1.5f);
                        Rigidbody rb = enemy.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            Vector3 dir = (enemy.transform.position - transform.position).normalized;
                            dir.y = 1f;
                            rb.AddForce(dir * 10f, ForceMode.Impulse);
                        }
                    }
                }
                state = SheepState.Idle;
                agent.speed = speed;
                break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        agent.speed = speed;
        if (state == SheepState.Charging)
            state = SheepState.Idle;
    }

    IEnumerator AttackingState()
    {
        while (state == SheepState.Attacking && !isDead)
        {
            if (targetEnemy == null)
            {
                state = SheepState.Idle;
                break;
            }

            float dist = Vector3.Distance(transform.position, targetEnemy.position);

            if (dist > attackRange)
            {
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(targetEnemy.position);
                    agent.isStopped = false;
                }
            }
            else
            {
                agent.isStopped = true;
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    EnemyHealth enemy = targetEnemy.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage);
                        Debug.Log($"🐑 Owca atakuje! {attackDamage} obrażeń!");
                    }
                }
            }

            if (targetEnemy == null || targetEnemy.GetComponent<EnemyHealth>() == null)
            {
                state = SheepState.Idle;
                if (owner != null)
                    targetPosition = owner.transform.position;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update()
    {
        if (isDead) return;

        if (mesh != null)
        {
            if (state == SheepState.Charging)
            {
                mesh.material.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 2f, 1f));
            }
            else if (state == SheepState.Attacking)
            {
                mesh.material.color = Color.Lerp(Color.white, Color.yellow, Mathf.PingPong(Time.time * 1.5f, 1f));
            }
            else
            {
                mesh.material.color = Color.Lerp(mesh.material.color, originalColor, Time.deltaTime * 5f);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null && state == SheepState.Charging)
        {
            enemy.TakeDamage(attackDamage * 1.5f);
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (enemy.transform.position - transform.position).normalized;
                dir.y = 1f;
                rb.AddForce(dir * 15f, ForceMode.Impulse);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;
        if (health <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        state = SheepState.Dead;
        if (agent != null) agent.isStopped = true;
        if (mesh != null) mesh.material.color = Color.gray;

        if (owner != null) owner.OnSheepDied(this);

        Destroy(gameObject, 1f);
        Debug.Log("🐑 Owca zginęła!");
    }

    // ===== METODY PUBLICZNE =====

    public void SetStats(float speed, float attackRange, float damage, float cooldown)
    {
        this.speed = speed;
        this.attackRange = attackRange;
        this.attackDamage = damage;
        this.attackCooldown = cooldown;
        if (agent != null) agent.speed = speed;
    }

    public void SetOwner(ShepherdAbilities owner)
    {
        this.owner = owner;
    }

    public void SetState(SheepState newState)
    {
        if (isDead) return;
        state = newState;
    }

    public SheepState GetState()
    {
        return state;
    }

    public void SetTarget(Transform target)
    {
        targetEnemy = target;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        targetPosition.y = 0f;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void SetHealth(float health, float maxHealth)
    {
        this.health = health;
        this.maxHealth = maxHealth;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (state == SheepState.Charging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}