using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    private ShepherdAbilities shepherd;
    private float damage;
    private bool canTakeDamage;
    private float maxHealth;
    private float currentHealth;
    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isInFormation = false;
    private float attackCooldown = 0f;

    public void Initialize(ShepherdAbilities owner, float sheepDamage, bool takeDamage, float maxHP)
    {
        shepherd = owner;
        damage = sheepDamage;
        canTakeDamage = takeDamage;
        maxHealth = maxHP;
        currentHealth = maxHP;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = 5f;
        agent.stoppingDistance = 1.5f;

        gameObject.tag = "Sheep";

        // Dodaj kolider
        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 1f;
        }

        // Dodaj renderer (widoczna kula)
        if (GetComponent<MeshRenderer>() == null)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            Destroy(visual.GetComponent<Collider>());

            Renderer rend = visual.GetComponent<Renderer>();
            rend.material.color = Color.white;
        }
    }

    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (isInFormation)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        // Szukaj najblizszego wroga
        if (currentTarget == null || (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > 15f))
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, 15f);
            float closestDistance = 15f;
            currentTarget = null;

            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    float dist = Vector3.Distance(transform.position, enemy.transform.position);
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        currentTarget = enemy.transform;
                    }
                }
            }
        }

        if (currentTarget != null && agent != null && agent.isOnNavMesh)
        {
            agent.SetDestination(currentTarget.position);
            agent.isStopped = false;

            if (Vector3.Distance(transform.position, currentTarget.position) < 1.5f && attackCooldown <= 0)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        if (currentTarget != null)
        {
            enemyHealth enemy = currentTarget.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("Owca atakuje! Obrazenia: " + damage);
                attackCooldown = 1f;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!canTakeDamage) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (shepherd != null)
        {
            shepherd.SheepDied(gameObject);
        }
        Destroy(gameObject);
    }

    public void Resurrect()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        currentTarget = null;
        isInFormation = false;
        if (agent != null) agent.isStopped = false;
    }

    public void SetFormationMode(bool inFormation)
    {
        isInFormation = inFormation;
        if (agent != null) agent.isStopped = inFormation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && canTakeDamage)
        {
            TakeDamage(10f);
        }
    }
}