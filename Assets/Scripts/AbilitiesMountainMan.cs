using UnityEngine;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 3f;
    public float attackDamage = 25f;
    public float attackRate = 1f;
    public float attackAngle = 60f;

    [Header("═══════════════ UMIEJĘTNOŚCI ═══════════════")]
    public float stompRange = 5f;
    public float stompDamage = 40f;
    public float stompCooldown = 8f;

    [Header("═══════════════ SPECIAL ═══════════════")]
    public float specialRange = 8f;
    public float specialDamage = 60f;
    public float specialCooldown = 12f;

    [Header("═══════════════ ULTIMATE ═══════════════")]
    public float ultimateRadius = 10f;
    public float ultimateDamage = 100f;
    public float ultimateCooldown = 30f;

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float pushbackForce = 10f;
    public float pushbackUpForce = 2f;

    [Header("═══════════════ WIZUALIZACJE ═══════════════")]
    public Color visualColor = new Color(1f, 0f, 0f, 0.5f);
    public float visualDuration = 0.3f;
    public float visualLineWidth = 0.08f;

    [Header("═══════════════ DŹWIĘKI ═══════════════")]
    public AudioClip[] attackSounds;
    public AudioClip[] specialSounds;
    public AudioClip[] ultimateSounds;
    public AudioClip[] hitSounds;
    public AudioClip deathSound;
    public AudioClip[] footstepSounds;

    [Header("═══════════════ GŁOŚNOŚĆ DŹWIĘKÓW ═══════════════")]
    [Range(0f, 1f)] public float attackVolume = 0.7f;
    [Range(0f, 1f)] public float specialVolume = 0.8f;
    [Range(0f, 1f)] public float ultimateVolume = 0.9f;
    [Range(0f, 1f)] public float hitVolume = 0.5f;
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float footstepVolume = 0.3f;

    [Header("═══════════════ INTERWAŁ KROKÓW ═══════════════")]
    public float footstepInterval = 0.5f;

    private float attackTimer = 0f;
    private float stompTimer = 0f;
    private float ultimateTimer = 0f;
    private float specialTimer = 0f;
    private Transform player;
    private Camera mainCamera;
    private bool canAttack = true;

    // Audio
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private bool isMoving = false;
    private Rigidbody rb;

    // Wizualizacje
    private GameObject visualObj;
    private LineRenderer visualLine;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        ultimateTimer = ultimateCooldown;
        specialTimer = specialCooldown;

        // Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.5f;
        rb = GetComponent<Rigidbody>();

        // Wizualizacje
        visualObj = new GameObject("AttackVisual");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualLine = visualObj.AddComponent<LineRenderer>();
        visualLine.startWidth = visualLineWidth;
        visualLine.endWidth = visualLineWidth;
        visualLine.useWorldSpace = false;
        visualLine.material = new Material(Shader.Find("Sprites/Default"));
        visualLine.startColor = visualColor;
        visualLine.endColor = new Color(visualColor.r, visualColor.g, visualColor.b, 0f);
        visualObj.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // Kroki
        if (rb != null)
        {
            isMoving = rb.linearVelocity.magnitude > 0.5f;
        }

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate && canAttack)
        {
            attackTimer = 0f;
            RotateToMouse();
            MeleeAttack();
        }

        stompTimer += Time.deltaTime;
        if (stompTimer >= stompCooldown && distance <= stompRange && Input.GetKeyDown(KeyCode.Q))
        {
            stompTimer = 0f;
            Stomp();
        }

        ultimateTimer += Time.deltaTime;
        if (ultimateTimer >= ultimateCooldown && Input.GetKeyDown(KeyCode.R))
        {
            ultimateTimer = 0f;
            RotateToMouse();
            Ultimate();
        }

        specialTimer += Time.deltaTime;
        if (specialTimer >= specialCooldown && Input.GetKeyDown(KeyCode.E))
        {
            specialTimer = 0f;
            RotateToMouse();
            SpecialAttack();
        }
    }

    void RotateToMouse()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }

    void ShowAttackVisual(float range, float angle)
    {
        if (visualLine == null) return;

        float halfAngle = angle / 2f;
        int points = 30;
        visualLine.positionCount = points + 3;

        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;

        visualLine.SetPosition(0, center);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        visualLine.SetPosition(1, center + leftDir * range);

        int pointIndex = 2;
        for (int i = 1; i <= points; i++)
        {
            float t = (float)i / points;
            float currentAngle = -halfAngle + (angle * t);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 point = center + dir * range;
            visualLine.SetPosition(pointIndex++, point);
        }

        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        visualLine.SetPosition(visualLine.positionCount - 1, center + rightDir * range);

        visualObj.SetActive(true);
        CancelInvoke(nameof(HideVisual));
        Invoke(nameof(HideVisual), visualDuration);
    }

    void ShowCircleVisual(float radius)
    {
        if (visualLine == null) return;

        int points = 40;
        visualLine.positionCount = points;
        visualLine.loop = true;

        for (int i = 0; i < points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            visualLine.SetPosition(i, new Vector3(x, 0.02f, z));
        }

        visualObj.SetActive(true);
        CancelInvoke(nameof(HideVisual));
        Invoke(nameof(HideVisual), visualDuration);
    }

    void HideVisual()
    {
        if (visualObj != null) visualObj.SetActive(false);
    }

    void MeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                Vector3 dir = (baseEnemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= attackAngle / 2)
                {
                    baseEnemy.TakeDamage(attackDamage);
                    PushbackEnemy(baseEnemy);
                    PlayAttackSound();
                }
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                Vector3 dir = (bazyliszek.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= attackAngle / 2)
                {
                    bazyliszek.TakeDamage(attackDamage);
                    PushbackEnemy(bazyliszek);
                    PlayAttackSound();
                }
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                Vector3 dir = (leszy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, dir);
                if (angle <= attackAngle / 2)
                {
                    leszy.TakeDamage(attackDamage);
                    PushbackEnemy(leszy);
                    PlayAttackSound();
                }
                continue;
            }
        }

        ShowAttackVisual(attackRange, attackAngle);
    }

    void Stomp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stompRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(stompDamage);
                PushbackEnemy(baseEnemy, 1.5f);
                PlaySpecialSound();
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(stompDamage);
                PushbackEnemy(bazyliszek, 1.5f);
                PlaySpecialSound();
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(stompDamage);
                PushbackEnemy(leszy, 1.5f);
                PlaySpecialSound();
                continue;
            }
        }

        ShowCircleVisual(stompRange);
    }

    void Ultimate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRadius);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(ultimateDamage);
                PushbackEnemy(baseEnemy, 2f);
                PlayUltimateSound();
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(ultimateDamage);
                PushbackEnemy(bazyliszek, 2f);
                PlayUltimateSound();
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(ultimateDamage);
                PushbackEnemy(leszy, 2f);
                PlayUltimateSound();
                continue;
            }
        }

        ShowCircleVisual(ultimateRadius);
    }

    void SpecialAttack()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.5f, transform.forward, specialRange);
        foreach (var hit in hits)
        {
            BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(specialDamage);
                PushbackEnemy(baseEnemy);
                PlaySpecialSound();
                continue;
            }

            Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(specialDamage);
                PushbackEnemy(bazyliszek);
                PlaySpecialSound();
                continue;
            }

            Leszy leszy = hit.collider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(specialDamage);
                PushbackEnemy(leszy);
                PlaySpecialSound();
                continue;
            }
        }

        ShowAttackVisual(specialRange, 30f);
    }

    void PushbackEnemy(object enemy, float forceMultiplier = 1f)
    {
        if (enemy == null) return;

        Rigidbody rb = null;
        Transform enemyTransform = null;

        if (enemy is BaseEnemy baseEnemy)
        {
            rb = baseEnemy.GetComponent<Rigidbody>();
            enemyTransform = baseEnemy.transform;
        }
        else if (enemy is Bazyliszek bazyliszek)
        {
            rb = bazyliszek.GetComponent<Rigidbody>();
            enemyTransform = bazyliszek.transform;
        }
        else if (enemy is Leszy leszy)
        {
            rb = leszy.GetComponent<Rigidbody>();
            enemyTransform = leszy.transform;
        }

        if (rb != null && enemyTransform != null)
        {
            Vector3 direction = (enemyTransform.position - transform.position).normalized;
            direction.y = pushbackUpForce;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * pushbackForce * forceMultiplier, ForceMode.Impulse);
        }
    }

    // ============================================================
    // DŹWIĘKI
    // ============================================================

    public void PlayAttackSound() => PlayRandomClip(attackSounds, attackVolume);
    public void PlaySpecialSound() => PlayRandomClip(specialSounds, specialVolume);
    public void PlayUltimateSound() => PlayRandomClip(ultimateSounds, ultimateVolume);
    public void PlayHitSound() => PlayRandomClip(hitSounds, hitVolume);
    public void PlayDeathSound() => PlayClip(deathSound, deathVolume);
    public void PlayFootstep() => PlayRandomClip(footstepSounds, footstepVolume);

    private void PlayRandomClip(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayClip(clip, volume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
    }

    void OnDestroy()
    {
        if (visualObj != null) Destroy(visualObj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Vector3 forward = transform.forward;
        Quaternion leftRot = Quaternion.Euler(0, -attackAngle / 2, 0);
        Quaternion rightRot = Quaternion.Euler(0, attackAngle / 2, 0);
        Vector3 leftDir = leftRot * forward * attackRange;
        Vector3 rightDir = rightRot * forward * attackRange;

        Gizmos.DrawLine(transform.position, transform.position + leftDir);
        Gizmos.DrawLine(transform.position, transform.position + rightDir);

        int points = 20;
        for (int i = 0; i <= points; i++)
        {
            float t = (float)i / points;
            float angle = -attackAngle / 2 + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = transform.position + dir * attackRange;

            if (i > 0)
            {
                float prevAngle = -attackAngle / 2 + (attackAngle * ((float)(i - 1) / points));
                Vector3 prevDir = Quaternion.Euler(0, prevAngle, 0) * forward;
                Vector3 prevPoint = transform.position + prevDir * attackRange;
                Gizmos.DrawLine(prevPoint, point);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stompRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, specialRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
    }
}