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

    [Header("═══════════════ CZAS UMIEJĘTNOŚCI ═══════════════")]
    public float stompDuration = 0.5f;
    public float specialDuration = 0.5f;
    public float ultimateTime = 3f;

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float pushbackForce = 10f;
    public float pushbackUpForce = 2f;

    [Header("═══════════════ FIRE POINT ═══════════════")]
    public Transform firePoint;

    [Header("═══════════════ WSKAŹNIK ZASIĘGU (PREFAB) ═══════════════")]
    public GameObject rangeIndicatorPrefab;
    public Vector3 indicatorOffset = new Vector3(0f, 0.1f, 0f);
    public Vector3 indicatorRotation = Vector3.zero;
    public float indicatorScale = 1f;
    public Color indicatorColor = new Color(0f, 1f, 0f, 0.3f);

    [Header("═══════════════ EFEKTY WIZUALNE ═══════════════")]
    public GameObject attackEffectPrefab;
    public GameObject specialEffectPrefab;
    public GameObject ultimateEffectPrefab;
    public GameObject hitEffectPrefab;

    [Header("═══════════════ USTAWIENIA EFEKTÓW ═══════════════")]
    public float effectScale = 1f;
    public float effectDestroyTime = 1.5f;

    [Header("═══════════════ USTAWIENIA ROTACJI PARTICLE 2D ═══════════════")]
    public float particleRotationOffset = 90f;

    [Header("═══════════════ WIZUALIZACJE ═══════════════")]
    public Color visualColor = new Color(1f, 0f, 0f, 0.5f);
    public float visualDuration = 0.3f;
    public float visualLineWidth = 0.08f;

    private float attackTimer = 0f;
    private float stompTimer = 0f;
    private float ultimateTimer = 0f;
    private float specialTimer = 0f;
    private Transform player;
    private bool canAttack = true;

    private PlayerMovement playerMovement;

    private GameObject visualObj;
    private LineRenderer visualLine;

    private GameObject rangeIndicator;
    private SpriteRenderer rangeSprite;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerMovement = playerObj.GetComponent<PlayerMovement>();
        }

        ultimateTimer = ultimateCooldown;
        specialTimer = specialCooldown;

        if (firePoint == null)
        {
            firePoint = transform;
        }

        CreateRangeIndicator();

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

    void CreateRangeIndicator()
    {
        if (rangeIndicatorPrefab == null)
        {
            rangeIndicator = new GameObject("RangeIndicator");
            rangeIndicator.transform.SetParent(transform);
            rangeIndicator.transform.localPosition = indicatorOffset;
            rangeIndicator.transform.localRotation = Quaternion.Euler(indicatorRotation);
            rangeIndicator.transform.localScale = Vector3.one * indicatorScale;

            rangeSprite = rangeIndicator.AddComponent<SpriteRenderer>();
            rangeSprite.sprite = CreateCircleSprite();
            rangeSprite.color = indicatorColor;
            rangeSprite.sortingOrder = -1;
        }
        else
        {
            rangeIndicator = Instantiate(rangeIndicatorPrefab, transform);
            rangeIndicator.transform.localPosition = indicatorOffset;
            rangeIndicator.transform.localRotation = Quaternion.Euler(indicatorRotation);
            rangeIndicator.transform.localScale = Vector3.one * indicatorScale;

            rangeSprite = rangeIndicator.GetComponent<SpriteRenderer>();
            if (rangeSprite == null)
            {
                rangeSprite = rangeIndicator.AddComponent<SpriteRenderer>();
                rangeSprite.sprite = CreateCircleSprite();
                rangeSprite.color = indicatorColor;
            }
        }

        UpdateRangeIndicator(attackRange);
    }

    Sprite CreateCircleSprite()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pixel = new Vector2(x, y);
                float dist = Vector2.Distance(pixel, center);

                if (dist <= radius - 2f)
                {
                    float alpha = 0.5f - (dist / radius) * 0.4f;
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else if (dist <= radius)
                {
                    colors[y * size + x] = new Color(1f, 1f, 1f, 0.8f);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void UpdateRangeIndicator(float range)
    {
        if (rangeIndicator == null) return;

        float scale = range * indicatorScale;
        rangeIndicator.transform.localScale = new Vector3(scale, scale, 1f);

        if (rangeSprite != null)
        {
            if (range <= attackRange)
                rangeSprite.color = new Color(0f, 1f, 0f, 0.3f);
            else if (range <= specialRange)
                rangeSprite.color = new Color(0f, 0.5f, 1f, 0.3f);
            else
                rangeSprite.color = new Color(1f, 0f, 0f, 0.3f);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (rangeIndicator != null)
        {
            rangeIndicator.transform.rotation = transform.rotation * Quaternion.Euler(indicatorRotation);
        }

        float distance = Vector3.Distance(transform.position, player.position);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate && canAttack)
        {
            attackTimer = 0f;
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
            Ultimate();
        }

        specialTimer += Time.deltaTime;
        if (specialTimer >= specialCooldown && Input.GetKeyDown(KeyCode.E))
        {
            specialTimer = 0f;
            SpecialAttack();
        }

        if (canAttack)
        {
            UpdateRangeIndicator(attackRange);
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

    private void SpawnEffect(GameObject effectPrefab, Vector3 position, float scale = 1f)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;

            main.loop = false;
            main.prewarm = false;
            main.playOnAwake = false;
            main.duration = 0.5f;
            main.startLifetime = 0.3f;

            float playerAngle = transform.eulerAngles.y;
            float rotationZ = playerAngle + particleRotationOffset;

            main.startRotation3D = false;
            main.startRotation = new ParticleSystem.MinMaxCurve(rotationZ * Mathf.Deg2Rad);

            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        Destroy(effect, effectDestroyTime);
    }

    void MeleeAttack()
    {
        UpdateRangeIndicator(attackRange);

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
                    AudioManager.Instance?.PlayEnemyHit();
                    SpawnEffect(hitEffectPrefab, baseEnemy.transform.position + Vector3.up, effectScale);
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
                    AudioManager.Instance?.PlayEnemyHit();
                    SpawnEffect(hitEffectPrefab, bazyliszek.transform.position + Vector3.up, effectScale);
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
                    AudioManager.Instance?.PlayEnemyHit();
                    SpawnEffect(hitEffectPrefab, leszy.transform.position + Vector3.up, effectScale);
                }
                continue;
            }
        }

        SpawnEffect(attackEffectPrefab, firePoint.position, effectScale);
        ShowAttackVisual(attackRange, attackAngle);
    }

    void Stomp()
    {
        UpdateRangeIndicator(stompRange);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stompRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(stompDamage);
                PushbackEnemy(baseEnemy, 1.5f);
                AudioManager.Instance?.PlayPerkSelect();
                SpawnEffect(hitEffectPrefab, baseEnemy.transform.position + Vector3.up, effectScale * 1.2f);
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(stompDamage);
                PushbackEnemy(bazyliszek, 1.5f);
                AudioManager.Instance?.PlayPerkSelect();
                SpawnEffect(hitEffectPrefab, bazyliszek.transform.position + Vector3.up, effectScale * 1.2f);
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(stompDamage);
                PushbackEnemy(leszy, 1.5f);
                AudioManager.Instance?.PlayPerkSelect();
                SpawnEffect(hitEffectPrefab, leszy.transform.position + Vector3.up, effectScale * 1.2f);
                continue;
            }
        }

        SpawnEffect(specialEffectPrefab, firePoint.position, effectScale * 1.5f);
        ShowCircleVisual(stompRange);
        Invoke(nameof(ResetRangeIndicator), 0.5f);
    }

    void Ultimate()
    {
        UpdateRangeIndicator(ultimateRadius);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRadius);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy baseEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(ultimateDamage);
                PushbackEnemy(baseEnemy, 2f);
                AudioManager.Instance?.PlayVictory();
                SpawnEffect(hitEffectPrefab, baseEnemy.transform.position + Vector3.up, effectScale * 1.5f);
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(ultimateDamage);
                PushbackEnemy(bazyliszek, 2f);
                AudioManager.Instance?.PlayVictory();
                SpawnEffect(hitEffectPrefab, bazyliszek.transform.position + Vector3.up, effectScale * 1.5f);
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(ultimateDamage);
                PushbackEnemy(leszy, 2f);
                AudioManager.Instance?.PlayVictory();
                SpawnEffect(hitEffectPrefab, leszy.transform.position + Vector3.up, effectScale * 1.5f);
                continue;
            }
        }

        SpawnEffect(ultimateEffectPrefab, firePoint.position, effectScale * 2f);
        ShowCircleVisual(ultimateRadius);
        Invoke(nameof(ResetRangeIndicator), 0.5f);
    }

    void SpecialAttack()
    {
        UpdateRangeIndicator(specialRange);

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.5f, transform.forward, specialRange);
        foreach (var hit in hits)
        {
            BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(specialDamage);
                PushbackEnemy(baseEnemy);
                AudioManager.Instance?.PlayLaser();
                SpawnEffect(hitEffectPrefab, baseEnemy.transform.position + Vector3.up, effectScale);
                continue;
            }

            Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(specialDamage);
                PushbackEnemy(bazyliszek);
                AudioManager.Instance?.PlayLaser();
                SpawnEffect(hitEffectPrefab, bazyliszek.transform.position + Vector3.up, effectScale);
                continue;
            }

            Leszy leszy = hit.collider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(specialDamage);
                PushbackEnemy(leszy);
                AudioManager.Instance?.PlayLaser();
                SpawnEffect(hitEffectPrefab, leszy.transform.position + Vector3.up, effectScale);
                continue;
            }
        }

        SpawnEffect(specialEffectPrefab, firePoint.position, effectScale * 1.2f);
        ShowAttackVisual(specialRange, 30f);
        Invoke(nameof(ResetRangeIndicator), 0.5f);
    }

    void ResetRangeIndicator()
    {
        if (canAttack)
        {
            UpdateRangeIndicator(attackRange);
        }
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

    public void SetCanAttack(bool value)
    {
        canAttack = value;
        if (canAttack)
        {
            UpdateRangeIndicator(attackRange);
        }
        else
        {
            if (rangeIndicator != null)
                rangeIndicator.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (visualObj != null) Destroy(visualObj);
        if (rangeIndicator != null) Destroy(rangeIndicator);
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

        if (firePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
            Gizmos.DrawLine(transform.position, firePoint.position);
        }
    }
}