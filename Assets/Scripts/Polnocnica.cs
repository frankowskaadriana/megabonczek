using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class Polnocnica : MonoBehaviour
{
    [Header("Statystyki")]
    public float maxHealth = 30f;
    public float moveSpeed = 2.5f;
    public float damage = 10f;
    public int expReward = 10;

    [Header("Atak")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;

    [Header("Odrzut po obrażeniach")]
    public float hitPushForce = 4f;
    public float hitPushUpForce = 0.5f;
    public float hitStunDuration = 0.3f;

    [Header("Efekty")]
    public GameObject deathEffect;
    public Color hitColor = Color.white;
    public float hitFlashDuration = 0.1f;

    [Header("UI")]
    public TextMeshPro healthText;

    private float currentHealth;
    private Transform player;
    private NavMeshAgent agent;
    private float attackTimer = 0f;
    private bool isDead = false;
    private bool isStunned = false;
    private MeshRenderer mesh;
    private Color originalColor;
    private LevelSystem levelSystem;
    private Rigidbody rb;
    private Coroutine flashCoroutine;
    private Coroutine pushbackCoroutine;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        FindPlayer();

        // === IGNORUJ KOLIZJE Z INNYMI WROGAMI ===
        IgnoreEnemyCollisions();

        // NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.angularSpeed = 360f;
        agent.acceleration = 8f;
        agent.stoppingDistance = attackRange * 0.7f;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.enabled = true;

        // Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Mesh
        mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            originalColor = mesh.material.color;
            mesh.material.color = new Color(0.6f, 0.3f, 0.8f);
        }

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        Debug.Log($"✅ {gameObject.name} gotowy!");
    }

    void IgnoreEnemyCollisions()
    {
        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) return;

        // Znajdź wszystkich wrogów i zignoruj kolizje
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != gameObject)
            {
                Collider otherCollider = enemy.GetComponent<Collider>();
                if (otherCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, otherCollider, true);
                }
            }
        }

        // Znajdź też po tagu "Enemy"
        GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in taggedEnemies)
        {
            if (enemy != null && enemy != gameObject)
            {
                Collider otherCollider = enemy.GetComponent<Collider>();
                if (otherCollider != null)
                {
                    Physics.IgnoreCollision(myCollider, otherCollider, true);
                }
            }
        }

        Debug.Log($"🚫 {gameObject.name}: Ignorowanie kolizji z innymi wrogami");
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            Debug.Log($"🎯 {gameObject.name} znalazł gracza: {player.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {gameObject.name} nie znalazł gracza!");
        }
    }

    void Update()
    {
        if (isDead || isStunned) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (agent != null && agent.isOnNavMesh && agent.enabled)
        {
            agent.SetDestination(player.position);

            if (dist > attackRange)
            {
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
                }
            }
        }
        else if (agent != null && !agent.isOnNavMesh)
        {
            agent.enabled = false;
            agent.enabled = true;
        }

        if (dist <= attackRange)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0f;
                MeleeAttack();
            }
        }

        if (healthText != null && Camera.main != null)
        {
            healthText.transform.LookAt(Camera.main.transform);
            healthText.transform.Rotate(0, 180, 0);
        }
    }

    void MeleeAttack()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange * 1.2f) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            AudioManager.Instance?.PlayEnemyAttack();
            Debug.Log($"⚔️ {gameObject.name} atakuje! {damage} obrażeń!");
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();

        FlashHit();
        AudioManager.Instance?.PlayEnemyHit();

        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);
        pushbackCoroutine = StartCoroutine(HitPushback());

        if (currentHealth <= 0) Die();
    }

    public void FlashHit()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashHitCoroutine());
    }

    IEnumerator FlashHitCoroutine()
    {
        if (mesh != null)
        {
            mesh.material.color = hitColor;
            yield return new WaitForSeconds(hitFlashDuration);
            mesh.material.color = originalColor;
        }
        flashCoroutine = null;
    }

    IEnumerator HitPushback()
    {
        if (isDead) yield break;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) yield break;

        Vector3 direction = (transform.position - playerObj.transform.position).normalized;
        direction.y = hitPushUpForce;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * hitPushForce, ForceMode.Impulse);
        }

        isStunned = true;

        yield return new WaitForSeconds(hitStunDuration);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            if (agent.isOnNavMesh)
            {
                agent.Warp(transform.position);
            }
        }

        isStunned = false;
        pushbackCoroutine = null;
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        AudioManager.Instance?.PlayEnemyDeath();

        if (deathEffect != null) Instantiate(deathEffect, transform.position, Quaternion.identity);
        if (levelSystem != null) levelSystem.EnemyDied();

        WaveSpawner waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null) waveSpawner.EnemyDied();

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (pushbackCoroutine != null) StopCoroutine(pushbackCoroutine);
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}