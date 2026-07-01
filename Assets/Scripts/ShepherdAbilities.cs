using UnityEngine;
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

    [Header("═══════════════ OWCE ═══════════════")]
    public GameObject sheepPrefab;
    public int maxSheep = 3;
    public float sheepSpeed = 5f;
    public float sheepAttackRange = 2f;
    public float sheepAttackDamage = 15f;
    public float sheepAttackCooldown = 1f;
    public float sheepSpawnCooldown = 10f;
    public float sheepDetectionRange = 10f;

    [Header("═══════════════ WIZUALIZACJE ═══════════════")]
    public Color visualColor = new Color(0f, 1f, 0f, 0.4f);
    public float visualDuration = 0.3f;
    public float visualLineWidth = 0.08f;

    private List<Sheep> sheep = new List<Sheep>();
    private float attackTimer = 0f;
    private float sheepSpawnTimer = 0f;
    private Transform player;
    private Camera mainCamera;
    private Vector3 targetPosition;

    // Wizualizacje
    private GameObject visualObj;
    private LineRenderer visualLine;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        // Stwórz obiekt do wizualizacji
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

        SpawnSheep();
        Debug.Log("🐕 Pasterz gotowy!");
    }

    void Update()
    {
        if (player == null) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            RotateToMouse();
            MeleeAttack();
        }

        if (Input.GetMouseButtonDown(1))
        {
            CommandSheep();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateToMouse();
            Bark();
        }

        sheepSpawnTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E) && sheepSpawnTimer >= sheepSpawnCooldown && sheep.Count < maxSheep)
        {
            sheepSpawnTimer = 0f;
            SpawnSheep();
        }

        UpdateSheep();
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

    void CommandSheep()
    {
        if (sheep.Count == 0) return;

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
                BaseEnemy enemy = hit.collider.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    CommandSheepAttack(enemy.transform);
                    return;
                }

                Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
                if (bazyliszek != null)
                {
                    CommandSheepAttack(bazyliszek.transform);
                    return;
                }

                Leszy leszy = hit.collider.GetComponent<Leszy>();
                if (leszy != null)
                {
                    CommandSheepAttack(leszy.transform);
                    return;
                }
            }

            CommandSheepCharge(targetPosition);
        }
    }

    void CommandSheepAttack(Transform target)
    {
        foreach (Sheep sheep in sheep)
        {
            if (sheep != null && !sheep.IsDead())
            {
                sheep.SetTarget(target);
                sheep.SetState(SheepState.Attacking);
                AudioManager.Instance?.PlayAttack();
            }
        }
        Debug.Log("🐑 Owce atakują wroga!");
    }

    void CommandSheepCharge(Vector3 position)
    {
        foreach (Sheep sheep in sheep)
        {
            if (sheep != null && !sheep.IsDead())
            {
                sheep.SetTargetPosition(position);
                sheep.SetState(SheepState.Charging);
                AudioManager.Instance?.PlayCharge();
            }
        }
        Debug.Log($"🐑 Owce szarżują do {position}!");
    }

    void UpdateSheep()
    {
        sheep.RemoveAll(s => s == null || s.IsDead());

        foreach (Sheep sheep in sheep)
        {
            if (sheep != null && !sheep.IsDead())
            {
                SheepState state = sheep.GetState();
                if (state == SheepState.Idle || state == SheepState.AutoAttacking)
                {
                    sheep.SetTargetPosition(transform.position);
                    if (state == SheepState.Idle)
                        sheep.SetState(SheepState.Following);
                }
            }
        }
    }

    void SpawnSheep()
    {
        if (sheepPrefab == null) return;

        Vector3 spawnPos = transform.position + Random.insideUnitSphere * 2f;
        spawnPos.y = 0f;

        GameObject sheepObj = Instantiate(sheepPrefab, spawnPos, Quaternion.identity);
        Sheep sheepScript = sheepObj.GetComponent<Sheep>();
        if (sheepScript == null) sheepScript = sheepObj.AddComponent<Sheep>();

        sheepScript.SetStats(sheepSpeed, sheepAttackRange, sheepAttackDamage, sheepAttackCooldown);
        sheepScript.SetOwner(this);
        sheepScript.SetTargetPosition(transform.position);
        sheepScript.SetDetectionRange(sheepDetectionRange);
        sheepScript.SetFollowTarget(player);

        sheep.Add(sheepScript);
        Debug.Log($"🐑 Owca przywołana! ({sheep.Count}/{maxSheep})");

        AudioManager.Instance?.PlaySheepSpawn();
        ShowCircleVisual(3f);
    }

    void MeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy enemy = hitCollider.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                AudioManager.Instance?.PlayAttack();
                ShowCircleVisual(attackRange);
                continue;
            }

            Bazyliszek bazyliszek = hitCollider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(attackDamage);
                AudioManager.Instance?.PlayAttack();
                ShowCircleVisual(attackRange);
                continue;
            }

            Leszy leszy = hitCollider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(attackDamage);
                AudioManager.Instance?.PlayAttack();
                ShowCircleVisual(attackRange);
                continue;
            }
        }
    }

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

                foreach (Sheep sheep in sheep)
                {
                    if (sheep != null && !sheep.IsDead())
                    {
                        sheep.SetTarget(enemy.transform);
                        sheep.SetState(SheepState.Attacking);
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

                foreach (Sheep sheep in sheep)
                {
                    if (sheep != null && !sheep.IsDead())
                    {
                        sheep.SetTarget(bazyliszek.transform);
                        sheep.SetState(SheepState.Attacking);
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

                foreach (Sheep sheep in sheep)
                {
                    if (sheep != null && !sheep.IsDead())
                    {
                        sheep.SetTarget(leszy.transform);
                        sheep.SetState(SheepState.Attacking);
                    }
                }
                continue;
            }
        }
        Debug.Log("🐕 Shepherd: Bark!");
        AudioManager.Instance?.PlayBark();
        ShowCircleVisual(barkRange);
    }

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