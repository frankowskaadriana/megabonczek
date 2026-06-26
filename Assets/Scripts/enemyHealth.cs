using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public enum EnemyType { Polnocnica, Strzyga, Upior, Leszy, Bazyliszek }

public class EnemyHealth : MonoBehaviour
{
    [Header("Typ")]
    public EnemyType enemyType = EnemyType.Polnocnica;

    [Header("Statystyki")]
    public float maxHealth = 50f;
    public float moveSpeed = 3f;
    public float damage = 20f;
    public int expReward = 10;

    [Header("Atak")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1f;

    [Header("Odepchnięcie")]
    public float pushForce = 3f;
    public float pushRadius = 1.5f;

    [Header("Odrzut po obrażeniach")]
    public float hitPushForce = 5f;
    public float hitPushUpForce = 1f;
    public float hitStunDuration = 0.3f;

    [Header("Efekty wizualne")]
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
    private Coroutine pushbackCoroutine;
    private Coroutine flashCoroutine;
    private float searchTimer = 0f;
    private float searchInterval = 0.5f;

    void Start()
    {
        currentHealth = maxHealth;
        levelSystem = FindFirstObjectByType<LevelSystem>();
        FindPlayer();

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.angularSpeed = 360f;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null) originalColor = mesh.material.color;

        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
        ApplyVisuals();
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

    void ApplyVisuals()
    {
        if (mesh == null) return;
        switch (enemyType)
        {
            case EnemyType.Polnocnica: mesh.material.color = new Color(0.6f, 0.3f, 0.8f); expReward = 10; break;
            case EnemyType.Strzyga: mesh.material.color = new Color(0.5f, 0.2f, 0.1f); expReward = 15; break;
            case EnemyType.Upior: mesh.material.color = new Color(0.4f, 0.6f, 0.3f); expReward = 20; break;
            case EnemyType.Leszy: mesh.material.color = new Color(0.2f, 0.7f, 0.2f); transform.localScale = Vector3.one * 1.8f; expReward = 100; break;
            case EnemyType.Bazyliszek: mesh.material.color = new Color(0.9f, 0.7f, 0.2f); transform.localScale = Vector3.one * 1.2f; expReward = 50; break;
        }
        originalColor = mesh.material.color;
        if (healthText != null) healthText.text = Mathf.Round(currentHealth).ToString();
    }

    void Update()
    {
        if (isDead || isStunned) return;

        // Szukaj gracza co jakiś czas
        searchTimer += Time.deltaTime;
        if (searchTimer >= searchInterval)
        {
            searchTimer = 0f;
            if (player == null)
            {
                FindPlayer();
                if (player == null) return;
            }
        }

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Poruszanie się tylko jeśli jest NavMesh
        if (agent != null && agent.isOnNavMesh && agent.enabled)
        {
            if (dist > attackRange)
            {
                agent.SetDestination(player.position);
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;
                // Obróć się w stronę gracza
                Vector3 direction = (player.position - transform.position).normalized;
                direction.y = 0;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
        else if (agent != null && !agent.isOnNavMesh)
        {
            // Jeśli nie ma NavMesh, spróbuj odzyskać
            Debug.LogWarning($"⚠️ {gameObject.name} nie ma NavMesh! Próba naprawy...");
            agent.enabled = false;
            agent.enabled = true;
        }

        // Atak
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

        // Sprawdź czy gracz jest w zasięgu
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange * 1.2f) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            ph.TakeDamage(damage);
            AudioManager.Instance?.PlayEnemyAttack();
            Debug.Log($"⚔️ {gameObject.name} atakuje! {damage} obrażeń!");

            // Odepchnij gracza
            if (dist < pushRadius)
            {
                Rigidbody playerRb = player.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    Vector3 dir = (player.position - transform.position).normalized;
                    dir.y = 0.5f;
                    playerRb.AddForce(dir * pushForce * 0.5f, ForceMode.Impulse);
                }
            }
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

        if (agent != null && agent.isOnNavMesh)
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
        AudioManager.Instance?.OnEnemyDied();

        if (levelSystem != null)
        {
            levelSystem.EnemyDied();
        }

        WaveSpawner waveSpawner = FindFirstObjectByType<WaveSpawner>();
        if (waveSpawner != null)
            waveSpawner.EnemyDied();

        Destroy(gameObject, 0.5f);
    }

    void OnDestroy()
    {
        if (pushbackCoroutine != null)
            StopCoroutine(pushbackCoroutine);
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
}