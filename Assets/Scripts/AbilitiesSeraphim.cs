using UnityEngine;
using System.Collections;

public class AbilitiesSeraphim : MonoBehaviour
{
    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 10f;
    public float attackDamage = 15f;
    public float attackRate = 0.8f;
    public GameObject lightBeamPrefab;
    public Transform firePoint;

    [Header("═══════════════ ODRZUT LIGHTBEAM ═══════════════")]
    public float beamPushbackForce = 6f;
    public float beamPushbackUpForce = 1.5f;

    [Header("═══════════════ UMIEJĘTNOŚCI ═══════════════")]
    public float healAmount = 30f;
    public float healCooldown = 10f;
    public float shieldDuration = 5f;
    public float chargeRange = 8f;
    public float chargeDamage = 30f;
    public float chargeSpeed = 15f;
    public float chargeCooldown = 6f;

    [Header("═══════════════ ULTIMATE - OSĄD ═══════════════")]
    public float judgmentRadius = 12f;
    public float judgmentDamage = 80f;
    public float judgmentCooldown = 25f;
    public float judgmentDuration = 3f;
    public GameObject judgmentEffect;

    [Header("═══════════════ SPECIAL ═══════════════")]
    public float specialRange = 12f;
    public float specialDamage = 40f;
    public float specialCooldown = 8f;

    [Header("═══════════════ WIZUALIZACJE ═══════════════")]
    public Color visualColor = new Color(0f, 0.5f, 1f, 0.5f);
    public float visualDuration = 0.3f;
    public float visualLineWidth = 0.08f;

    [Header("═══════════════ DŹWIĘKI ═══════════════")]
    public AudioClip[] laserSounds;
    public AudioClip[] specialSounds;
    public AudioClip[] ultimateSounds;
    public AudioClip[] healSounds;
    public AudioClip[] chargeSounds;
    public AudioClip deathSound;
    public AudioClip[] footstepSounds;

    [Header("═══════════════ GŁOŚNOŚĆ DŹWIĘKÓW ═══════════════")]
    [Range(0f, 1f)] public float laserVolume = 0.7f;
    [Range(0f, 1f)] public float specialVolume = 0.8f;
    [Range(0f, 1f)] public float ultimateVolume = 0.9f;
    [Range(0f, 1f)] public float healVolume = 0.6f;
    [Range(0f, 1f)] public float chargeVolume = 0.7f;
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float footstepVolume = 0.2f;

    [Header("═══════════════ INTERWAŁ KROKÓW ═══════════════")]
    public float footstepInterval = 0.35f;

    private float attackTimer = 0f;
    private float healTimer = 0f;
    private float ultimateTimer = 0f;
    private float specialTimer = 0f;
    private float chargeTimer = 0f;
    private Transform player;
    private bool isShielded = false;
    private bool isCharging = false;
    private Vector3 chargeDirection;
    private Rigidbody rb;
    private Camera mainCamera;
    private bool canShoot = true;

    // Audio
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private bool isMoving = false;

    // Wizualizacje
    private GameObject visualObj;
    private LineRenderer visualLine;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        ultimateTimer = judgmentCooldown;
        specialTimer = specialCooldown;
        chargeTimer = chargeCooldown;

        if (firePoint == null)
        {
            Transform fp = transform.Find("FirePoint");
            if (fp != null) firePoint = fp;
            else firePoint = transform;
        }

        // Audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.5f;

        CreateVisualLine();
    }

    void CreateVisualLine()
    {
        visualObj = new GameObject("TrajectoryVisual");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = Vector3.zero;

        visualLine = visualObj.AddComponent<LineRenderer>();
        visualLine.startWidth = visualLineWidth;
        visualLine.endWidth = visualLineWidth * 0.3f;
        visualLine.useWorldSpace = true;
        visualLine.positionCount = 30;
        visualLine.loop = false;
        visualLine.sortingOrder = 10;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = visualColor;
        visualLine.material = mat;
        visualLine.startColor = visualColor;
        visualLine.endColor = new Color(visualColor.r, visualColor.g, visualColor.b, 0f);

        visualObj.SetActive(false);
    }

    void ShowTrajectoryVisual()
    {
        if (visualLine == null || firePoint == null) return;

        Vector3 startPoint = firePoint.position;
        Vector3 direction = transform.forward;

        float range = attackRange;
        visualLine.positionCount = 30;
        visualLine.loop = false;
        visualLine.useWorldSpace = true;

        for (int i = 0; i < 30; i++)
        {
            float t = (float)i / 29f;
            Vector3 point = startPoint + direction * (t * range);
            visualLine.SetPosition(i, point);
        }

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
        visualLine.useWorldSpace = false;

        Vector3 center = firePoint != null ? firePoint.localPosition : Vector3.zero;

        for (int i = 0; i < points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            visualLine.SetPosition(i, new Vector3(x, center.y, z));
        }

        visualObj.SetActive(true);
        CancelInvoke(nameof(HideVisual));
        Invoke(nameof(HideVisual), visualDuration);
    }

    void HideVisual()
    {
        if (visualObj != null) visualObj.SetActive(false);
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

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate && canShoot)
        {
            attackTimer = 0f;
            RotateToMouse();
            RangedAttack();
        }

        healTimer += Time.deltaTime;
        if (healTimer >= healCooldown && Input.GetKeyDown(KeyCode.Q))
        {
            healTimer = 0f;
            Heal();
        }

        ultimateTimer += Time.deltaTime;
        if (ultimateTimer >= judgmentCooldown && Input.GetKeyDown(KeyCode.R))
        {
            ultimateTimer = 0f;
            StartCoroutine(Judgment());
        }

        specialTimer += Time.deltaTime;
        if (specialTimer >= specialCooldown && Input.GetKeyDown(KeyCode.E))
        {
            specialTimer = 0f;
            SpecialAttack();
        }

        chargeTimer += Time.deltaTime;
        if (chargeTimer >= chargeCooldown && Input.GetKeyDown(KeyCode.LeftShift))
        {
            chargeTimer = 0f;
            StartCoroutine(Charge());
        }

        if (isCharging && rb != null)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
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

    void RangedAttack()
    {
        if (lightBeamPrefab == null || firePoint == null) return;

        Vector3 targetPosition = GetMouseWorldPosition();
        if (targetPosition == Vector3.zero) return;

        Vector3 direction = (targetPosition - firePoint.position).normalized;
        direction.y = 0f;

        GameObject beam = Instantiate(lightBeamPrefab, firePoint.position, Quaternion.LookRotation(direction));
        LightBeam lightBeam = beam.GetComponent<LightBeam>();
        if (lightBeam != null)
        {
            lightBeam.SetBeam(attackDamage, attackRange, 0.5f, 25f);
            lightBeam.SetPushback(beamPushbackForce, beamPushbackUpForce);
        }

        PlayLaserSound();
        ShowTrajectoryVisual();
    }

    Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, firePoint.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    void Heal()
    {
        if (player == null) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            PlayHealSound();
        }
    }

    IEnumerator Judgment()
    {
        PlayUltimateSound();
        ShowCircleVisual(judgmentRadius);

        if (judgmentEffect != null)
        {
            GameObject effect = Instantiate(judgmentEffect, transform.position, Quaternion.identity);
            effect.transform.localScale = Vector3.one * judgmentRadius * 2f;
            Destroy(effect, judgmentDuration);
        }

        float timer = 0f;
        while (timer < judgmentDuration)
        {
            timer += Time.deltaTime;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, judgmentRadius);
            foreach (var hitCollider in hitColliders)
            {
                BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
                if (baseEnemy != null)
                {
                    baseEnemy.TakeDamage(judgmentDamage * Time.deltaTime);

                    Rigidbody rb = baseEnemy.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (baseEnemy.transform.position - transform.position).normalized;
                        dir.y = 2f;
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.AddForce(dir * 5f * Time.deltaTime, ForceMode.Impulse);
                    }
                    continue;
                }

                Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
                if (bazyliszek != null)
                {
                    bazyliszek.TakeDamage(judgmentDamage * Time.deltaTime);

                    Rigidbody rb = bazyliszek.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (bazyliszek.transform.position - transform.position).normalized;
                        dir.y = 2f;
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.AddForce(dir * 5f * Time.deltaTime, ForceMode.Impulse);
                    }
                    continue;
                }

                Leszy leszy = hitCollider.GetComponent<Leszy>();
                if (leszy != null)
                {
                    leszy.TakeDamage(judgmentDamage * Time.deltaTime);

                    Rigidbody rb = leszy.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 dir = (leszy.transform.position - transform.position).normalized;
                        dir.y = 2f;
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        rb.AddForce(dir * 5f * Time.deltaTime, ForceMode.Impulse);
                    }
                    continue;
                }

                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(5f * Time.deltaTime);
                }
            }

            yield return null;
        }

        Debug.Log($"⚖️ SERAPHIM OSĄD!");
    }

    void SpecialAttack()
    {
        PlaySpecialSound();
        ShowCircleVisual(specialRange);

        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, specialRange);
        foreach (var hit in hits)
        {
            BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(specialDamage);

                Rigidbody rb = baseEnemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (baseEnemy.transform.position - transform.position).normalized;
                    dir.y = 1.5f;
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(dir * beamPushbackForce * 1.5f, ForceMode.Impulse);
                }
                continue;
            }

            Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(specialDamage);

                Rigidbody rb = bazyliszek.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (bazyliszek.transform.position - transform.position).normalized;
                    dir.y = 1.5f;
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(dir * beamPushbackForce * 1.5f, ForceMode.Impulse);
                }
                continue;
            }

            Leszy leszy = hit.collider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(specialDamage);

                Rigidbody rb = leszy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 dir = (leszy.transform.position - transform.position).normalized;
                    dir.y = 1.5f;
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.AddForce(dir * beamPushbackForce * 1.5f, ForceMode.Impulse);
                }
                continue;
            }
        }
    }

    IEnumerator Charge()
    {
        if (isCharging) yield break;

        PlayChargeSound();
        ShowCircleVisual(chargeRange);

        isCharging = true;
        chargeDirection = transform.forward;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, chargeRange);
        float closestDistance = Mathf.Infinity;
        Vector3 closestEnemy = transform.position + transform.forward * chargeRange;

        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                float distance = Vector3.Distance(transform.position, baseEnemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = baseEnemy.transform.position;
                }
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                float distance = Vector3.Distance(transform.position, bazyliszek.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = bazyliszek.transform.position;
                }
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                float distance = Vector3.Distance(transform.position, leszy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = leszy.transform.position;
                }
                continue;
            }
        }

        if (closestDistance < chargeRange)
        {
            chargeDirection = (closestEnemy - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(chargeDirection);
        }

        float chargeTime = 0.3f;
        while (chargeTime > 0)
        {
            chargeTime -= Time.deltaTime;

            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);
            foreach (var enemyCollider in enemies)
            {
                BaseEnemy baseEnemy = enemyCollider.GetComponent<BaseEnemy>();
                if (baseEnemy != null)
                {
                    baseEnemy.TakeDamage(chargeDamage);
                    Rigidbody enemyRb = baseEnemy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.isKinematic = false;
                        enemyRb.useGravity = true;
                        enemyRb.AddForce(chargeDirection * 15f, ForceMode.Impulse);
                    }
                    continue;
                }

                Bazyliszek bazyliszek = enemyCollider.GetComponent<Bazyliszek>();
                if (bazyliszek != null)
                {
                    bazyliszek.TakeDamage(chargeDamage);
                    Rigidbody enemyRb = bazyliszek.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.isKinematic = false;
                        enemyRb.useGravity = true;
                        enemyRb.AddForce(chargeDirection * 15f, ForceMode.Impulse);
                    }
                    continue;
                }

                Leszy leszy = enemyCollider.GetComponent<Leszy>();
                if (leszy != null)
                {
                    leszy.TakeDamage(chargeDamage);
                    Rigidbody enemyRb = leszy.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.isKinematic = false;
                        enemyRb.useGravity = true;
                        enemyRb.AddForce(chargeDirection * 15f, ForceMode.Impulse);
                    }
                    continue;
                }
            }

            yield return null;
        }

        isCharging = false;
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Debug.Log($"⚡ Seraphim Charge: {chargeDamage} obrażeń!");
    }

    // ============================================================
    // DŹWIĘKI
    // ============================================================

    public void PlayLaserSound() => PlayRandomClip(laserSounds, laserVolume);
    public void PlaySpecialSound() => PlayRandomClip(specialSounds, specialVolume);
    public void PlayUltimateSound() => PlayRandomClip(ultimateSounds, ultimateVolume);
    public void PlayHealSound() => PlayRandomClip(healSounds, healVolume);
    public void PlayChargeSound() => PlayRandomClip(chargeSounds, chargeVolume);
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

    public void ActivateShield()
    {
        if (!isShielded)
        {
            isShielded = true;
            StartCoroutine(ShieldDuration());
        }
    }

    IEnumerator ShieldDuration()
    {
        yield return new WaitForSeconds(shieldDuration);
        isShielded = false;
        Debug.Log("🛡️ Tarcza wygasła!");
    }

    public void SetCanShoot(bool value)
    {
        canShoot = value;
    }

    void OnDestroy()
    {
        if (visualObj != null) Destroy(visualObj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, judgmentRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chargeRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * specialRange);

        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, firePoint.position);
        }
    }
}