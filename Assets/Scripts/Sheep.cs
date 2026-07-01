using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum SheepState
{
    Idle,
    Following,
    Charging,
    Attacking,
    AutoAttacking,
    Dead
}

public class Sheep : MonoBehaviour
{
    [Header("Statystyki")]
    public float speed = 5f;
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;

    private NavMeshAgent agent;
    private Transform targetEnemy;
    private Transform player;
    private float attackTimer = 0f;
    private bool isDead = false;
    private MeshRenderer mesh;
    private Color originalColor;
    private float health = 30f;
    private ShepherdAbilities owner;
    private SheepState state = SheepState.Idle;

    void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = attackRange;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null) originalColor = mesh.material.color;

        StartCoroutine(AutoFindEnemy());
    }

    IEnumerator AutoFindEnemy()
    {
        while (!isDead)
        {
            FindNearestEnemy();
            yield return new WaitForSeconds(0.5f);
        }
    }

    void FindNearestEnemy()
    {
        if (isDead) return;

        BaseEnemy[] baseEnemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        if (baseEnemies.Length > 0)
        {
            BaseEnemy closest = null;
            float closestDist = Mathf.Infinity;

            foreach (BaseEnemy enemy in baseEnemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                targetEnemy = closest.transform;
                return;
            }
        }

        Bazyliszek[] bazyliszki = FindObjectsByType<Bazyliszek>(FindObjectsSortMode.None);
        if (bazyliszki.Length > 0)
        {
            Bazyliszek closest = null;
            float closestDist = Mathf.Infinity;

            foreach (Bazyliszek enemy in bazyliszki)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                targetEnemy = closest.transform;
                return;
            }
        }

        Leszy[] lesze = FindObjectsByType<Leszy>(FindObjectsSortMode.None);
        if (lesze.Length > 0)
        {
            Leszy closest = null;
            float closestDist = Mathf.Infinity;

            foreach (Leszy enemy in lesze)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                targetEnemy = closest.transform;
                return;
            }
        }

        targetEnemy = null;
    }

    void Update()
    {
        if (isDead) return;

        if (targetEnemy != null)
        {
            float dist = Vector3.Distance(transform.position, targetEnemy.position);

            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(targetEnemy.position);
                agent.isStopped = dist <= attackRange;
            }

            if (dist <= attackRange)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    Attack();
                }
            }
        }
        else if (player != null)
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(player.position);
            }
        }
    }

    void Attack()
    {
        if (targetEnemy == null) return;

        BaseEnemy baseEnemy = targetEnemy.GetComponent<BaseEnemy>();
        if (baseEnemy != null)
        {
            baseEnemy.TakeDamage(attackDamage);
            AudioManager.Instance?.PlayEnemyHit();
            return;
        }

        Bazyliszek bazyliszek = targetEnemy.GetComponent<Bazyliszek>();
        if (bazyliszek != null)
        {
            bazyliszek.TakeDamage(attackDamage);
            AudioManager.Instance?.PlayEnemyHit();
            return;
        }

        Leszy leszy = targetEnemy.GetComponent<Leszy>();
        if (leszy != null)
        {
            leszy.TakeDamage(attackDamage);
            AudioManager.Instance?.PlayEnemyHit();
            return;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        health -= damage;
        StartCoroutine(FlashHit());
        if (health <= 0) Die();
    }

    IEnumerator FlashHit()
    {
        if (mesh != null)
        {
            mesh.material.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            mesh.material.color = originalColor;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        state = SheepState.Dead;
        if (agent != null) agent.isStopped = true;
        if (mesh != null) mesh.material.color = Color.gray;
        if (owner != null) owner.OnSheepDied(this);
        Destroy(gameObject, 1f);
        Debug.Log("🐑 Owca zginęła!");
    }

    // ===== METODY PUBLICZNE =====

    public void SetOwner(ShepherdAbilities owner)
    {
        this.owner = owner;
    }

    public void SetTarget(Transform target)
    {
        targetEnemy = target;
    }

    public void SetTargetPosition(Vector3 position)
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(position);
        }
    }

    public void SetState(SheepState newState)
    {
        state = newState;
    }

    public SheepState GetState()
    {
        return state;
    }

    public bool IsDead() => isDead;

    public void SetStats(float speed, float range, float damage, float cooldown)
    {
        this.speed = speed;
        this.attackRange = range;
        this.attackDamage = damage;
        this.attackCooldown = cooldown;
        if (agent != null) agent.speed = speed;
    }

    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }

    public void SetFollowTarget(Transform target)
    {
        player = target;
    }
}