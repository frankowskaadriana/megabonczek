using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShepherdAbilities : MonoBehaviour
{
    [Header("═══════════════ ATAK ═══════════════")]
    public float attackRange = 2.5f;
    public float attackDamage = 20f;
    public float attackRate = 1.2f;

    [Header("═══════════════ UMIEJĘTNOŚCI ═══════════════")]
    public float barkRange = 5f;
    public float barkFearDuration = 3f;
    public float barkCooldown = 2f;

    [Header("═══════════════ OWCE ═══════════════")]
    public GameObject sheepPrefab;
    public int maxSheep = 3;
    public float sheepSpeed = 5f;
    public float sheepAttackRange = 2f;
    public float sheepAttackDamage = 15f;
    public float sheepAttackCooldown = 1f;
    public float sheepSpawnCooldown = 10f;
    public float sheepDetectionRange = 10f;

    [Header("═══════════════ WYBUCH OWCY (SPECIAL - Q) ═══════════════")]
    public float explosionRadius = 3f;
    public float explosionDamage = 40f;
    public float explodeCooldown = 8f;

    [Header("═══════════════ PRZYWOŁANIE OWIEC (ULTIMATE - R) ═══════════════")]
    public float ultimateCooldown = 30f;
    public int ultimateSheepCount = 3;

    [Header("═══════════════ UI UMIEJĘTNOŚCI ═══════════════")]
    public Image specialIcon;
    public Image ultimateIcon;
    public TextMeshProUGUI specialCDText;
    public TextMeshProUGUI ultimateCDText;
    public Color readyColor = Color.white;
    public Color cooldownColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    [Header("═══════════════ EFEKTY 2D ═══════════════")]
    public GameObject spawnEffectPrefab;
    public GameObject explosionEffectPrefab;
    public float effectDuration = 1.5f;

    [Header("═══════════════ WIZUALIZACJE ═══════════════")]
    public Color visualColor = new Color(0f, 1f, 0f, 0.4f);
    public float visualDuration = 0.3f;
    public float visualLineWidth = 0.08f;

    [Header("═══════════════ INTERWAŁ KROKÓW ═══════════════")]
    public float footstepInterval = 0.45f;

    private AudioShepherd audioShepherd;

    private List<Sheep> sheep = new List<Sheep>();
    private float attackTimer = 0f;
    private float sheepSpawnTimer = 0f;
    private float explodeTimer = 0f;
    private float ultimateTimer = 0f;
    private float barkTimer = 0f;
    private Transform player;
    private Camera mainCamera;
    private Vector3 targetPosition;

    private float footstepTimer = 0f;
    private bool isMoving = false;
    private Rigidbody rb;

    private GameObject visualObj;
    private LineRenderer visualLine;

    private Vector3 sheepTargetPosition;
    private Transform sheepTargetEnemy;
    private bool hasTarget = false;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        audioShepherd = GetComponent<AudioShepherd>();
        if (audioShepherd == null)
        {
            Debug.LogWarning("⚠️ AudioShepherd nie znaleziony!");
        }

        rb = GetComponent<Rigidbody>();

        ultimateTimer = ultimateCooldown;
        explodeTimer = 0f;
        sheepSpawnTimer = 0f;
        barkTimer = 0f;

        visualObj = new GameObject("AttackVisual");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualLine = visualObj.AddComponent<LineRenderer>();
        visualLine.startWidth = visualLineWidth;
        visualLine.endWidth = visualLineWidth;
        visualLine.useWorldSpace = false;
        visualLine.loop = true;
        visualLine.material = new Material(Shader.Find("Sprites/Default"));
        visualLine.startColor = visualColor;
        visualLine.endColor = new Color(visualColor.r, visualColor.g, visualColor.b, 0f);
        visualObj.SetActive(false);

        UpdateAbilityUI();

        SpawnSheep();
        Debug.Log("🐕 Pasterz gotowy!");
    }

    void Update()
    {
        if (player == null) return;

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

        // Cooldowny
        if (explodeTimer > 0) explodeTimer -= Time.deltaTime;
        if (ultimateTimer > 0) ultimateTimer -= Time.deltaTime;
        if (sheepSpawnTimer > 0) sheepSpawnTimer -= Time.deltaTime;
        if (barkTimer > 0) barkTimer -= Time.deltaTime;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            RotateToMouse();
            MeleeAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CommandSheepMove();
        }

        if (Input.GetKeyDown(KeyCode.Q) && explodeTimer <= 0 && sheep.Count > 0)
        {
            ExplodeSheep();
        }

        if (Input.GetKeyDown(KeyCode.R) && ultimateTimer <= 0)
        {
            UltimateSheepSpawn();
        }

        if (Input.GetKeyDown(KeyCode.E) && barkTimer <= 0)
        {
            RotateToMouse();
            Bark();
            barkTimer = barkCooldown;
        }

        UpdateSheep();
        UpdateAbilityUI();
    }

    // ============================================================
    // DŹWIĘKI
    // ============================================================

    void PlayFootstep()
    {
        if (audioShepherd != null) audioShepherd.PlayFootstep();
    }

    void PlayAttackSound()
    {
        if (audioShepherd != null) audioShepherd.PlayAttack();
    }

    void PlayBarkSound()
    {
        if (audioShepherd != null) audioShepherd.PlayBark();
    }

    void PlaySpecialSound()
    {
        if (audioShepherd != null) audioShepherd.PlaySpecial();
    }

    void PlayUltimateSound()
    {
        if (audioShepherd != null) audioShepherd.PlayUltimate();
    }

    void PlaySheepSpawnSound()
    {
        if (audioShepherd != null) audioShepherd.PlaySheepSpawn();
    }

    // ============================================================
    // UI
    // ============================================================

    void UpdateAbilityUI()
    {
        if (specialIcon != null)
        {
            bool ready = explodeTimer <= 0 && sheep.Count > 0;
            specialIcon.color = ready ? readyColor : cooldownColor;
        }
        if (specialCDText != null)
        {
            if (explodeTimer > 0)
                specialCDText.text = Mathf.CeilToInt(explodeTimer).ToString();
            else if (sheep.Count == 0)
                specialCDText.text = "!";
            else
                specialCDText.text = "";
        }

        if (ultimateIcon != null)
        {
            bool ready = ultimateTimer <= 0;
            ultimateIcon.color = ready ? readyColor : cooldownColor;
        }
        if (ultimateCDText != null)
        {
            if (ultimateTimer > 0)
                ultimateCDText.text = Mathf.CeilToInt(ultimateTimer).ToString();
            else
                ultimateCDText.text = "";
        }
    }

    // ============================================================
    // EFEKTY
    // ============================================================

    void SpawnEffect(GameObject effectPrefab, Vector3 position, float scale = 1f)
    {
        if (effectPrefab == null) return;

        GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * scale;
        Destroy(effect, effectDuration);
    }

    // ============================================================
    // ROTACJA I WIZUALIZACJE
    // ============================================================

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

    // ============================================================
    // KOMENDY DLA OWIEC
    // ============================================================

    void CommandSheepMove()
    {
        if (sheep.Count == 0)
        {
            Debug.Log("🐑 Brak owiec do wysłania!");
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            targetPosition = ray.GetPoint(distance);
            targetPosition.y = 0f;

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
                if (baseEnemy != null)
                {
                    sheepTargetEnemy = baseEnemy.transform;
                    sheepTargetPosition = baseEnemy.transform.position;
                    hasTarget = true;

                    foreach (Sheep s in sheep)
                    {
                        if (s != null && !s.IsDead())
                        {
                            s.SetTarget(baseEnemy.transform);
                            s.SetState(SheepState.Attacking);
                        }
                    }
                    Debug.Log("🐑 Owce idą do wroga!");
                    return;
                }

                Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
                if (bazyliszek != null)
                {
                    sheepTargetEnemy = bazyliszek.transform;
                    sheepTargetPosition = bazyliszek.transform.position;
                    hasTarget = true;

                    foreach (Sheep s in sheep)
                    {
                        if (s != null && !s.IsDead())
                        {
                            s.SetTarget(bazyliszek.transform);
                            s.SetState(SheepState.Attacking);
                        }
                    }
                    Debug.Log("🐑 Owce idą do Bazyliszka!");
                    return;
                }

                Leszy leszy = hit.collider.GetComponent<Leszy>();
                if (leszy != null)
                {
                    sheepTargetEnemy = leszy.transform;
                    sheepTargetPosition = leszy.transform.position;
                    hasTarget = true;

                    foreach (Sheep s in sheep)
                    {
                        if (s != null && !s.IsDead())
                        {
                            s.SetTarget(leszy.transform);
                            s.SetState(SheepState.Attacking);
                        }
                    }
                    Debug.Log("🐑 Owce idą do Leszego!");
                    return;
                }
            }

            sheepTargetEnemy = null;
            sheepTargetPosition = targetPosition;
            hasTarget = true;

            foreach (Sheep s in sheep)
            {
                if (s != null && !s.IsDead())
                {
                    s.SetTargetPosition(targetPosition);
                    s.SetState(SheepState.Charging);
                }
            }
            Debug.Log($"🐑 Owce idą do {targetPosition}!");
        }
    }

    // ============================================================
    // WYBUCH OWCY (Q)
    // ============================================================

    void ExplodeSheep()
    {
        if (sheep.Count == 0)
        {
            Debug.Log("🐑 Brak owiec do wybuchu!");
            return;
        }

        Sheep closestSheep = null;
        float closestDist = Mathf.Infinity;

        foreach (Sheep s in sheep)
        {
            if (s == null || s.IsDead()) continue;

            float dist;
            if (sheepTargetEnemy != null)
            {
                dist = Vector3.Distance(s.transform.position, sheepTargetEnemy.position);
            }
            else
            {
                dist = Vector3.Distance(s.transform.position, sheepTargetPosition);
            }

            if (dist < closestDist)
            {
                closestDist = dist;
                closestSheep = s;
            }
        }

        if (closestSheep == null)
        {
            closestSheep = sheep.Find(s => s != null && !s.IsDead());
        }

        if (closestSheep != null)
        {
            Vector3 explosionPos = closestSheep.transform.position;

            SpawnEffect(explosionEffectPrefab, explosionPos, 1.5f);

            closestSheep.Explode(explosionRadius, explosionDamage);
            explodeTimer = explodeCooldown;
            PlaySpecialSound();
            ShowCircleVisual(explosionRadius);

            Debug.Log($"💥 Owca wybuchła! Obrażenia: {explosionDamage} w promieniu {explosionRadius}");

            sheep.Remove(closestSheep);

            if (sheep.Count == 0)
            {
                hasTarget = false;
            }
        }
    }

    // ============================================================
    // ULTIMATE (R)
    // ============================================================

    void UltimateSheepSpawn()
    {
        if (sheepPrefab == null) return;

        ultimateTimer = ultimateCooldown;
        PlayUltimateSound();
        ShowCircleVisual(3f);

        SpawnEffect(spawnEffectPrefab, transform.position, 2f);

        int spawned = 0;
        for (int i = 0; i < ultimateSheepCount; i++)
        {
            if (sheep.Count >= maxSheep + 3) break;

            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = 0f;

            SpawnEffect(spawnEffectPrefab, spawnPos, 1f);

            GameObject sheepObj = Instantiate(sheepPrefab, spawnPos, Quaternion.identity);
            Sheep sheepScript = sheepObj.GetComponent<Sheep>();
            if (sheepScript == null) sheepScript = sheepObj.AddComponent<Sheep>();

            sheepScript.SetStats(sheepSpeed, sheepAttackRange, sheepAttackDamage, sheepAttackCooldown);
            sheepScript.SetOwner(this);
            sheepScript.SetTargetPosition(transform.position);
            sheepScript.SetDetectionRange(sheepDetectionRange);
            sheepScript.SetFollowTarget(player);
            sheepScript.SetExplosionRadius(explosionRadius);
            sheepScript.SetExplosionDamage(explosionDamage);

            sheep.Add(sheepScript);
            spawned++;
        }

        Debug.Log($"🐑 Przywołano {spawned} owiec! (Ultimate)");
        PlaySheepSpawnSound();
    }

    // ============================================================
    // PRZYWOŁANIE OWCY (F)
    // ============================================================

    void SpawnSheep()
    {
        if (sheepPrefab == null) return;

        Vector3 spawnPos = transform.position + Random.insideUnitSphere * 2f;
        spawnPos.y = 0f;

        SpawnEffect(spawnEffectPrefab, spawnPos, 1f);

        GameObject sheepObj = Instantiate(sheepPrefab, spawnPos, Quaternion.identity);
        Sheep sheepScript = sheepObj.GetComponent<Sheep>();
        if (sheepScript == null) sheepScript = sheepObj.AddComponent<Sheep>();

        sheepScript.SetStats(sheepSpeed, sheepAttackRange, sheepAttackDamage, sheepAttackCooldown);
        sheepScript.SetOwner(this);
        sheepScript.SetTargetPosition(transform.position);
        sheepScript.SetDetectionRange(sheepDetectionRange);
        sheepScript.SetFollowTarget(player);
        sheepScript.SetExplosionRadius(explosionRadius);
        sheepScript.SetExplosionDamage(explosionDamage);

        sheep.Add(sheepScript);
        Debug.Log($"🐑 Owca przywołana! ({sheep.Count}/{maxSheep})");

        PlaySheepSpawnSound();
        ShowCircleVisual(3f);
    }

    // ============================================================
    // ATAK WRĘCZ (LPM)
    // ============================================================

    void MeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                PlayAttackSound();
                ShowCircleVisual(attackRange);
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(attackDamage);
                PlayAttackSound();
                ShowCircleVisual(attackRange);
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(attackDamage);
                PlayAttackSound();
                ShowCircleVisual(attackRange);
                continue;
            }
        }
    }

    // ============================================================
    // SZCZEKANIE (E)
    // ============================================================

    void Bark()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, barkRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                Rigidbody rb = enemy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (enemy.transform.position - transform.position).normalized;
                    direction.y = 1f;
                    rb.AddForce(direction * 15f, ForceMode.Impulse);
                }
                enemy.TakeDamage(5f);

                // Owce atakują odrzuconego wroga
                foreach (Sheep s in sheep)
                {
                    if (s != null && !s.IsDead())
                    {
                        s.SetTarget(enemy.transform);
                        s.SetState(SheepState.Attacking);
                    }
                }
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                Rigidbody rb = bazyliszek.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (bazyliszek.transform.position - transform.position).normalized;
                    direction.y = 1f;
                    rb.AddForce(direction * 15f, ForceMode.Impulse);
                }
                bazyliszek.TakeDamage(5f);

                foreach (Sheep s in sheep)
                {
                    if (s != null && !s.IsDead())
                    {
                        s.SetTarget(bazyliszek.transform);
                        s.SetState(SheepState.Attacking);
                    }
                }
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                Rigidbody rb = leszy.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (leszy.transform.position - transform.position).normalized;
                    direction.y = 1f;
                    rb.AddForce(direction * 15f, ForceMode.Impulse);
                }
                leszy.TakeDamage(5f);

                foreach (Sheep s in sheep)
                {
                    if (s != null && !s.IsDead())
                    {
                        s.SetTarget(leszy.transform);
                        s.SetState(SheepState.Attacking);
                    }
                }
                continue;
            }
        }
        Debug.Log("🐕 Shepherd: Bark!");

        PlayBarkSound();
        ShowCircleVisual(barkRange);
    }

    // ============================================================
    // AKTUALIZACJA OWIEC
    // ============================================================

    void UpdateSheep()
    {
        sheep.RemoveAll(s => s == null || s.IsDead());

        foreach (Sheep s in sheep)
        {
            if (s != null && !s.IsDead())
            {
                SheepState state = s.GetState();

                if (state == SheepState.Idle || state == SheepState.AutoAttacking)
                {
                    s.SetTargetPosition(transform.position);
                    if (state == SheepState.Idle)
                        s.SetState(SheepState.Following);
                }
            }
        }
    }

    // ============================================================
    // METODY PUBLICZNE
    // ============================================================

    public void OnSheepDied(Sheep sheep)
    {
        if (this.sheep.Contains(sheep))
        {
            this.sheep.Remove(sheep);
            Debug.Log($"🐑 Owca zginęła! Pozostało: {this.sheep.Count}");
        }
    }

    public int GetSheepCount() => sheep.Count;
    public int GetMaxSheep() => maxSheep;

    void OnDestroy()
    {
        if (visualObj != null) Destroy(visualObj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, barkRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}