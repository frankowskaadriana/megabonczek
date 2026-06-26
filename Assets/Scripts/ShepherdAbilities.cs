using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShepherdAbilities : MonoBehaviour
{
    [Header("Atak")]
    public float attackRange = 2.5f;
    public float attackDamage = 20f;
    public float attackRate = 1.2f;

    [Header("Umiejętności")]
    public float barkRange = 5f;
    public float barkFearDuration = 3f;

    [Header("Owce")]
    public GameObject sheepPrefab;
    public int maxSheep = 3;
    public float sheepSpeed = 5f;
    public float sheepAttackRange = 2f;
    public float sheepAttackDamage = 15f;
    public float sheepAttackCooldown = 1f;
    public float sheepSpawnCooldown = 10f;
    public float sheepDetectionRange = 10f;

    private List<Sheep> sheep = new List<Sheep>();
    private float attackTimer = 0f;
    private float sheepSpawnTimer = 0f;
    private Transform player;
    private Camera mainCamera;
    private Vector3 targetPosition;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        SpawnSheep();
    }

    void Update()
    {
        if (player == null) return;

        // Automatyczny atak pasterza
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            RotateToMouse();
            MeleeAttack();
        }

        // SZARŻA OWIEC - PPM
        if (Input.GetMouseButtonDown(1))
        {
            CommandSheep();
        }

        // Szczekanie
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateToMouse();
            Bark();
        }

        // Przywołanie owcy
        sheepSpawnTimer += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.E) && sheepSpawnTimer >= sheepSpawnCooldown && sheep.Count < maxSheep)
        {
            sheepSpawnTimer = 0f;
            SpawnSheep();
        }

        // Aktualizuj owce
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
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    CommandSheepAttack(enemy.transform);
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

        sheep.Add(sheepScript);
        Debug.Log($"🐑 Owca przywołana! ({sheep.Count}/{maxSheep})");

        AudioManager.Instance?.PlayHeal();
    }

    void MeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                AudioManager.Instance?.PlayAttack();
            }
        }
    }

    void Bark()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, barkRange);
        foreach (var hitCollider in hitColliders)
        {
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
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
            }
        }
        Debug.Log("🐕 Shepherd: Bark!");
        AudioManager.Instance?.PlaySpecialAbility();
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