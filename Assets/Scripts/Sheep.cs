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
    [Header("═══════════════ STATYSTYKI ═══════════════")]
    public float speed = 5f;
    public float attackRange = 2f;
    public float attackDamage = 15f;
    public float attackCooldown = 1f;
    public float detectionRange = 10f;
    public float followDistance = 3f;

    [Header("═══════════════ ZDROWIE ═══════════════")]
    public float maxHealth = 30f;
    private float currentHealth;

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float pushbackForce = 4f;
    public float pushbackUpForce = 0.3f;
    public float pushbackRadius = 2.5f;

    [Header("═══════════════ ODEPCHNIĘCIE PO KOLIZJI ═══════════════")]
    public float collisionPushForce = 2f;
    public float collisionPushRadius = 1.5f;

    [Header("═══════════════ EFEKTY WIZUALNE ═══════════════")]
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.15f;
    public Color attackColor = Color.yellow;
    public float attackFlashDuration = 0.1f;

    [Header("═══════════════ DŹWIĘKI ═══════════════")]
    public AudioClip[] attackSounds;      // Dźwięki ataku owcy
    public AudioClip[] hitSounds;         // Dźwięki otrzymania obrażeń
    public AudioClip deathSound;          // Dźwięk śmierci
    public AudioClip[] footstepSounds;    // Dźwięki kroków

    private NavMeshAgent agent;
    private Transform targetEnemy;
    private Transform currentTarget;
    private Vector3 targetPosition;
    private SheepState state = SheepState.Idle;
    private float attackTimer = 0f;
    private bool isDead = false;
    private ShepherdAbilities owner;
    private MeshRenderer mesh;
    private Color originalColor;
    private float health = 30f;
    private float autoAttackInterval = 0.3f;
    private bool hasNavMesh = false;
    private Transform followTarget;
    private Coroutine flashCoroutine;
    private Rigidbody rb;
    private float pushCooldown = 0f;
    private float pushCooldownTime = 0.5f;

    // === AudioSource dla dźwięków ===
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) followTarget = playerObj.transform;

        if (playerObj != null)
        {
            Collider sheepCollider = GetComponent<Collider>();
            Collider playerCollider = playerObj.GetComponent<Collider>();
            if (sheepCollider != null && playerCollider != null)
            {
                Physics.IgnoreCollision(sheepCollider, playerCollider, true);
            }
        }

        // === AUDIO SOURCE ===
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
        audioSource.spatialBlend = 0.5f;

        NavMeshHit hit;
        hasNavMesh = NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas);

        if (!hasNavMesh)
        {
            Debug.LogWarning($"🐑 Owca nie ma NavMesh w pobliżu! Pozycja: {transform.position}");
            StartCoroutine(RetryNavMesh());
        }
        else
        {
            SetupNavMeshAgent();
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 10f;
        rb.isKinematic = true;
        rb.useGravity = false;

        mesh = GetComponent<MeshRenderer>();
        if (mesh != null)
        {
            originalColor = mesh.material.color;
            mesh.material.color = Color.white;
        }

        if (followTarget != null)
            targetPosition = followTarget.position;

        StartCoroutine(StateMachine());
        StartCoroutine(AutoFindEnemy());
    }

    IEnumerator RetryNavMesh()
    {
        yield return new WaitForSeconds(1f);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            SetupNavMeshAgent();
            Debug.Log($"🐑 Owca naprawiona! Nowa pozycja: {transform.position}");
        }
        else
        {
            Debug.LogError($"🐑 Owca nie może znaleźć NavMesh! Pozycja: {transform.position}");
        }
    }

    void SetupNavMeshAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.stoppingDistance = attackRange;
        agent.autoBraking = true;
        agent.enabled = true;
        hasNavMesh = true;
    }

    IEnumerator AutoFindEnemy()
    {
        while (!isDead)
        {
            if (state != SheepState.Attacking && state != SheepState.Charging)
            {
                FindNearestEnemy();
            }
            yield return new WaitForSeconds(autoAttackInterval);
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
                if (enemy == null) continue;
                if (enemy.gameObject == null) continue;

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                currentTarget = closest.transform;
                if (state != SheepState.Attacking && state != SheepState.Charging)
                {
                    state = SheepState.AutoAttacking;
                    targetEnemy = currentTarget;
                }
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
                if (enemy == null) continue;
                if (enemy.gameObject == null) continue;

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                currentTarget = closest.transform;
                if (state != SheepState.Attacking && state != SheepState.Charging)
                {
                    state = SheepState.AutoAttacking;
                    targetEnemy = currentTarget;
                }
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
                if (enemy == null) continue;
                if (enemy.gameObject == null) continue;

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < closestDist && dist <= detectionRange)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                currentTarget = closest.transform;
                if (state != SheepState.Attacking && state != SheepState.Charging)
                {
                    state = SheepState.AutoAttacking;
                    targetEnemy = currentTarget;
                }
                return;
            }
        }

        targetEnemy = null;
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
                case SheepState.AutoAttacking:
                    yield return StartCoroutine(AutoAttackingState());
                    break;
            }
            yield return null;
        }
    }

    IEnumerator IdleState()
    {
        while (state == SheepState.Idle && !isDead)
        {
            if (followTarget != null && Vector3.Distance(transform.position, followTarget.position) > followDistance)
            {
                targetPosition = followTarget.position;
                state = SheepState.Following;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator FollowingState()
    {
        while (state == SheepState.Following && !isDead)
        {
            if (followTarget != null)
            {
                targetPosition = followTarget.position;

                if (Vector3.Distance(transform.position, followTarget.position) <= followDistance)
                {
                    BaseEnemy[] baseEnemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
                    bool hasEnemy = false;
                    foreach (BaseEnemy enemy in baseEnemies)
                    {
                        if (enemy != null && Vector3.Distance(transform.position, enemy.transform.position) <= detectionRange)
                        {
                            hasEnemy = true;
                            break;
                        }
                    }

                    if (!hasEnemy)
                    {
                        state = SheepState.Idle;
                        if (agent != null) agent.isStopped = true;
                        yield break;
                    }
                }
            }

            if (agent != null && agent.isOnNavMesh && agent.enabled)
            {
                agent.SetDestination(targetPosition);
                agent.isStopped = false;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AutoAttackingState()
    {
        while (state == SheepState.AutoAttacking && !isDead)
        {
            if (targetEnemy == null || targetEnemy.GetComponent<EnemyHealth>() == null)
            {
                state = SheepState.Following;
                if (followTarget != null)
                    targetPosition = followTarget.position;
                break;
            }

            float dist = Vector3.Distance(transform.position, targetEnemy.position);

            if (dist > attackRange)
            {
                if (agent != null && agent.isOnNavMesh && agent.enabled)
                {
                    agent.SetDestination(targetEnemy.position);
                    agent.isStopped = false;
                }
            }
            else
            {
                if (agent != null) agent.isStopped = true;
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    EnemyHealth enemy = targetEnemy.GetComponent<EnemyHealth>();
                    if (enemy != null && !enemy.gameObject.CompareTag("Dead"))
                    {
                        enemy.TakeDamage(attackDamage);
                        PushbackEnemy(enemy);
                        FlashAttack();

                        // === DŹWIĘK ATAKU OWCY ===
                        PlayAttackSound();
                    }
                }
            }

            if (dist > detectionRange)
            {
                state = SheepState.Following;
                if (followTarget != null)
                    targetPosition = followTarget.position;
                break;
            }

            if (state == SheepState.Charging || state == SheepState.Attacking)
                break;

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator AttackingState()
    {
        while (state == SheepState.Attacking && !isDead)
        {
            if (targetEnemy == null)
            {
                state = SheepState.Following;
                if (followTarget != null)
                    targetPosition = followTarget.position;
                break;
            }

            float dist = Vector3.Distance(transform.position, targetEnemy.position);

            if (dist > attackRange)
            {
                if (agent != null && agent.isOnNavMesh && agent.enabled)
                {
                    agent.SetDestination(targetEnemy.position);
                    agent.isStopped = false;
                }
            }
            else
            {
                if (agent != null) agent.isStopped = true;
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackCooldown)
                {
                    attackTimer = 0f;
                    EnemyHealth enemy = targetEnemy.GetComponent<EnemyHealth>();
                    if (enemy != null && !enemy.gameObject.CompareTag("Dead"))
                    {
                        enemy.TakeDamage(attackDamage);
                        PushbackEnemy(enemy);
                        FlashAttack();

                        // === DŹWIĘK ATAKU OWCY ===
                        PlayAttackSound();
                    }
                }
            }

            if (targetEnemy == null || targetEnemy.GetComponent<EnemyHealth>() == null)
            {
                state = SheepState.Following;
                if (followTarget != null)
                    targetPosition = followTarget.position;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ChargingState()
    {
        float chargeTime = 3f;
        float startSpeed = speed;
        if (agent != null) agent.speed = speed * 2f;

        if (mesh != null)
            mesh.material.color = Color.red;

        while (state == SheepState.Charging && !isDead && chargeTime > 0)
        {
            chargeTime -= Time.deltaTime;

            if (agent != null && agent.isOnNavMesh && agent.enabled)
            {
                agent.SetDestination(targetPosition);
                agent.isStopped = false;
            }

            if (Vector3.Distance(transform.position, targetPosition) < 1.5f)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, pushbackRadius);
                foreach (var hit in hitColliders)
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null && !enemy.gameObject.CompareTag("Dead"))
                    {
                        enemy.TakeDamage(attackDamage * 1.5f);
                        PushbackEnemy(enemy, 1.5f);
                        FlashAttack();

                        // === DŹWIĘK SZARŻY ===
                        PlayAttackSound();
                    }
                }

                state = SheepState.Following;
                if (agent != null) agent.speed = speed;
                if (followTarget != null)
                    targetPosition = followTarget.position;
                if (mesh != null)
                    mesh.material.color = originalColor;
                break;
            }

            yield return new WaitForSeconds(0.05f);
        }

        if (agent != null) agent.speed = speed;
        if (state == SheepState.Charging)
        {
            state = SheepState.Following;
            if (followTarget != null)
                targetPosition = followTarget.position;
        }
        if (mesh != null)
            mesh.material.color = originalColor;
    }

    void PushbackEnemy(EnemyHealth enemy, float forceMultiplier = 1f)
    {
        if (enemy == null) return;

        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            direction.y = pushbackUpForce;

            enemyRb.isKinematic = false;
            enemyRb.useGravity = true;
            enemyRb.AddForce(direction * pushbackForce * forceMultiplier, ForceMode.Impulse);
        }
    }

    // === DŹWIĘKI ===

    void PlayAttackSound()
    {
        if (audioSource == null) return;
        if (attackSounds.Length > 0)
        {
            AudioClip clip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(clip, 0.6f);
        }
    }

    void PlayHitSound()
    {
        if (audioSource == null) return;
        if (hitSounds.Length > 0)
        {
            AudioClip clip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(clip, 0.5f);
        }
    }

    void PlayDeathSound()
    {
        if (audioSource == null) return;
        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound, 0.7f);
        }
    }

    // === KOLIZJE I OBRAŻENIA ===

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // Sprawdź czy to przeciwnik (BaseEnemy)
        BaseEnemy baseEnemy = collision.gameObject.GetComponent<BaseEnemy>();
        if (baseEnemy != null)
        {
            // Owca otrzymuje obrażenia od przeciwnika
            TakeDamage(10f);

            // Odepchnij owcę od przeciwnika
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0.2f;

            if (rb != null && pushCooldown <= 0)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(pushDirection * collisionPushForce * 2f, ForceMode.Impulse);
                pushCooldown = pushCooldownTime;

                StartCoroutine(ResetKinematic());
            }
            return;
        }

        // Sprawdź czy to Bazyliszek
        Bazyliszek bazyliszek = collision.gameObject.GetComponent<Bazyliszek>();
        if (bazyliszek != null)
        {
            TakeDamage(15f);

            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0.2f;

            if (rb != null && pushCooldown <= 0)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(pushDirection * collisionPushForce * 2f, ForceMode.Impulse);
                pushCooldown = pushCooldownTime;

                StartCoroutine(ResetKinematic());
            }
            return;
        }

        // Sprawdź czy to Leszy
        Leszy leszy = collision.gameObject.GetComponent<Leszy>();
        if (leszy != null)
        {
            TakeDamage(25f);

            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            pushDirection.y = 0.2f;

            if (rb != null && pushCooldown <= 0)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.AddForce(pushDirection * collisionPushForce * 3f, ForceMode.Impulse);
                pushCooldown = pushCooldownTime;

                StartCoroutine(ResetKinematic());
            }
            return;
        }

        // Sprawdź czy to inna owca (unikaj kolizji z owcami)
        Sheep otherSheep = collision.gameObject.GetComponent<Sheep>();
        if (otherSheep != null && otherSheep != this)
        {
            // Lekko odepchnij owce od siebie
            Vector3 pushDirection = (transform.position - collision.transform.position).normalized;
            if (rb != null && pushCooldown <= 0)
            {
                rb.AddForce(pushDirection * 1f, ForceMode.Impulse);
                pushCooldown = pushCooldownTime;
                StartCoroutine(ResetKinematic());
            }
        }
    }

    // === ZDROWIE I ŚMIERĆ ===

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // === DŹWIĘK OBRAŻEŃ ===
        PlayHitSound();

        // === EFEKT MIGANIA ===
        FlashHit();

        Debug.Log($"🐑 Owca otrzymała {damage} obrażeń! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        state = SheepState.Dead;

        if (agent != null) agent.isStopped = true;
        if (mesh != null) mesh.material.color = Color.gray;

        // === DŹWIĘK ŚMIERCI ===
        PlayDeathSound();

        if (owner != null) owner.OnSheepDied(this);

        Destroy(gameObject, 1f);
        Debug.Log("🐑 Owca zginęła!");
    }

    IEnumerator ResetKinematic()
    {
        yield return new WaitForSeconds(0.3f);
        if (rb != null && !isDead)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (pushCooldown > 0)
            pushCooldown -= Time.deltaTime;

        if (followTarget != null && (state == SheepState.Following || state == SheepState.Idle))
        {
            targetPosition = followTarget.position;
        }

        if (mesh != null && flashCoroutine == null)
        {
            if (state == SheepState.Charging)
            {
                mesh.material.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 3f, 1f));
            }
            else if (state == SheepState.Attacking || state == SheepState.AutoAttacking)
            {
                mesh.material.color = Color.Lerp(originalColor, attackColor, Mathf.PingPong(Time.time * 2f, 1f));
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
            PushbackEnemy(enemy, 1.5f);
            FlashAttack();

            // === DŹWIĘK ATAKU ===
            PlayAttackSound();
        }
    }

    // === EFEKTY WIZUALNE ===

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

    void FlashAttack()
    {
        if (mesh != null)
        {
            StartCoroutine(FlashAttackCoroutine());
        }
    }

    IEnumerator FlashAttackCoroutine()
    {
        if (mesh != null)
        {
            Color tempColor = mesh.material.color;
            mesh.material.color = attackColor;
            yield return new WaitForSeconds(attackFlashDuration);
            mesh.material.color = tempColor;
        }
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
        if (target != null)
        {
            state = SheepState.Attacking;
        }
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
        currentHealth = health;
    }

    public void SetDetectionRange(float range)
    {
        detectionRange = range;
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
        if (target != null && (state == SheepState.Idle || state == SheepState.Following))
        {
            targetPosition = target.position;
            state = SheepState.Following;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, pushbackRadius);
        if (state == SheepState.Charging)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}