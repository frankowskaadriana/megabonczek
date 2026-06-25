using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Sheep : MonoBehaviour
{
    [Header("═══════════════ SHEEP STATS ═══════════════")]
    private ShepherdAbilities shepherd;
    private float damage;
    private bool canTakeDamage;
    private float maxHealth;
    private float currentHealth;

    [Header("═══════════════ COMPONENTS ═══════════════")]
    private NavMeshAgent agent;
    private Transform currentTarget;
    private bool isInFormation = false;
    private float attackCooldown = 0f;
    private float attackRange = 1.5f;

    [Header("═══════════════ AVOIDANCE SETTINGS ═══════════════")]
    public float avoidanceRadius = 1.2f;
    public float avoidanceForce = 3f;
    public float shepherdAvoidanceRadius = 2f;
    public float shepherdAvoidanceForce = 5f;

    [Header("═══════════════ HEALTH TEXT 3D ═══════════════")]
    public TextMeshPro healthText3D;
    public float textHeight = 1.2f;
    public Vector3 textOffset = new Vector3(0, 1.2f, 0);

    void Awake()
    {
        CreateHealthText();
    }

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
        agent.stoppingDistance = attackRange;
        agent.radius = avoidanceRadius * 0.5f;

        gameObject.tag = "Sheep";

        if (GetComponent<Collider>() == null)
        {
            CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
            col.radius = 0.5f;
            col.height = 1f;
        }

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        CreateVisual();
        UpdateHealthText();

        Debug.Log("Owca stworzona! Obrazenia: " + damage + ", HP: " + currentHealth + "/" + maxHealth);
    }

    void CreateVisual()
    {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        Destroy(visual.GetComponent<Collider>());

        Renderer rend = visual.GetComponent<Renderer>();
        rend.material.color = Color.white;
    }

    void CreateHealthText()
    {
        if (healthText3D != null) return;

        GameObject textObj = new GameObject("HealthText3D");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = textOffset;

        healthText3D = textObj.AddComponent<TextMeshPro>();
        healthText3D.fontSize = 0.5f;
        healthText3D.alignment = TextAlignmentOptions.Center;
        healthText3D.color = Color.green;
        healthText3D.text = "?";

        healthText3D.fontMaterial.SetFloat("_OutlineWidth", 0.1f);
        healthText3D.fontMaterial.SetColor("_OutlineColor", Color.black);
    }

    void Update()
    {
        if (healthText3D != null)
        {
            healthText3D.transform.localPosition = textOffset;
            if (Camera.main != null)
            {
                healthText3D.transform.LookAt(Camera.main.transform);
                healthText3D.transform.Rotate(0, 180, 0);
            }
        }

        if (attackCooldown > 0)
            attackCooldown -= Time.deltaTime;

        if (isInFormation)
        {
            if (agent != null) agent.isStopped = true;
            return;
        }

        if (currentTarget == null || (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > 15f))
        {
            FindClosestEnemy();
        }

        if (currentTarget != null && agent != null && agent.isOnNavMesh)
        {
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 avoidance = CalculateAvoidance();
            Vector3 finalDirection = (directionToTarget + avoidance).normalized;

            Vector3 newDestination = transform.position + finalDirection * 5f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(newDestination, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                agent.SetDestination(currentTarget.position);
            }

            agent.isStopped = false;

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= attackRange && attackCooldown <= 0)
            {
                Attack();
            }
        }
    }

    Vector3 CalculateAvoidance()
    {
        Vector3 avoidanceForceVector = Vector3.zero;

        Collider[] nearbySheep = Physics.OverlapSphere(transform.position, avoidanceRadius);
        foreach (Collider sheep in nearbySheep)
        {
            if (sheep.CompareTag("Sheep") && sheep.gameObject != gameObject)
            {
                Vector3 directionAway = transform.position - sheep.transform.position;
                float distance = directionAway.magnitude;
                if (distance < avoidanceRadius)
                {
                    float strength = (1f - distance / avoidanceRadius) * avoidanceForce;
                    avoidanceForceVector += directionAway.normalized * strength;
                }
            }
        }

        if (shepherd != null)
        {
            Transform shepherdTransform = shepherd.transform;
            float distanceToShepherd = Vector3.Distance(transform.position, shepherdTransform.position);
            if (distanceToShepherd < shepherdAvoidanceRadius)
            {
                Vector3 directionAway = transform.position - shepherdTransform.position;
                float strength = (1f - distanceToShepherd / shepherdAvoidanceRadius) * shepherdAvoidanceForce;
                avoidanceForceVector += directionAway.normalized * strength;
            }
        }

        RaycastHit hit;
        Vector3 rayDirection = transform.forward;
        if (Physics.Raycast(transform.position, rayDirection, out hit, 2f))
        {
            if (!hit.collider.CompareTag("Enemy"))
            {
                Vector3 wallAvoidance = Vector3.Cross(Vector3.up, hit.normal);
                avoidanceForceVector += wallAvoidance * 2f;
            }
        }

        return avoidanceForceVector;
    }

    void FindClosestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, 20f);
        float closestDistance = 20f;
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

    void Attack()
    {
        if (currentTarget != null)
        {
            enemyHealth enemy = currentTarget.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                attackCooldown = 1f;
                StartCoroutine(FlashRed());
            }
        }
    }

    IEnumerator FlashRed()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Color original = rend.material.color;
            rend.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            rend.material.color = original;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!canTakeDamage) return;

        currentHealth -= amount;
        UpdateHealthText();
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    void UpdateHealthText()
    {
        if (healthText3D == null)
        {
            CreateHealthText();
            return;
        }

        healthText3D.text = Mathf.Round(currentHealth).ToString();

        float healthPercent = currentHealth / maxHealth;
        if (healthPercent > 0.6f)
            healthText3D.color = Color.green;
        else if (healthPercent > 0.3f)
            healthText3D.color = Color.yellow;
        else
            healthText3D.color = Color.red;
    }

    IEnumerator DamageFlash()
    {
        if (healthText3D != null)
        {
            Color originalColor = healthText3D.color;
            healthText3D.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            UpdateHealthText();
        }
    }

    void Die()
    {
        if (shepherd != null) shepherd.SheepDied(gameObject);
        if (healthText3D != null) Destroy(healthText3D.gameObject);
        Destroy(gameObject);
    }

    public void Resurrect()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        currentTarget = null;
        isInFormation = false;
        if (agent != null) agent.isStopped = false;
        UpdateHealthText();
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