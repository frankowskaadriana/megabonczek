using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("Atak")]
    public float attackRange = 3f;
    public float attackDamage = 25f;
    public float attackRate = 1f;
    public float attackAngle = 60f;

    [Header("Umiejętności")]
    public float stompRange = 5f;
    public float stompDamage = 40f;
    public float stompCooldown = 8f;

    [Header("Ultimate")]
    public float ultimateRadius = 10f;
    public float ultimateDamage = 100f;
    public float ultimateCooldown = 30f;

    [Header("Special")]
    public float specialRange = 8f;
    public float specialDamage = 60f;
    public float specialCooldown = 12f;

    [Header("Odrzut")]
    public float pushbackForce = 10f;
    public float pushbackUpForce = 2f;

    private float attackTimer = 0f;
    private float stompTimer = 0f;
    private float ultimateTimer = 0f;
    private float specialTimer = 0f;
    private Transform player;
    private Camera mainCamera;
    private bool canAttack = true;

    void Start()
    {
        mainCamera = Camera.main;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        ultimateTimer = ultimateCooldown;
        specialTimer = specialCooldown;
    }

    void Update()
    {
        if (player == null) return;

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

    void MeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            // Szukaj EnemyHealth - na obiekcie lub rodzicu
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                Vector3 directionToEnemy = (enemy.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);

                if (angle <= attackAngle / 2)
                {
                    enemy.TakeDamage(attackDamage);
                    PushbackEnemy(enemy);
                    AudioManager.Instance?.PlayAttack();
                    Debug.Log($"⚔️ Góral: {attackDamage} obrażeń dla {enemy.name}!");
                }
            }
        }
    }

    void Stomp()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, stompRange);
        foreach (var hitCollider in hitColliders)
        {
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(stompDamage);
                PushbackEnemy(enemy, 1.5f);
                AudioManager.Instance?.PlayStomp();
                Debug.Log($"💥 Stomp: {stompDamage} obrażeń dla {enemy.name}!");
            }
        }
    }

    void Ultimate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ultimateRadius);
        foreach (var hitCollider in hitColliders)
        {
            EnemyHealth enemy = hitCollider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hitCollider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(ultimateDamage);
                PushbackEnemy(enemy, 2f);
                AudioManager.Instance?.PlayUltimate();
                Debug.Log($"🔥 ULTIMATE: {ultimateDamage} obrażeń dla {enemy.name}!");
            }
        }
    }

    void SpecialAttack()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 0.5f, transform.forward, specialRange);
        foreach (var hit in hits)
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemy != null)
            {
                enemy.TakeDamage(specialDamage);
                PushbackEnemy(enemy);
                AudioManager.Instance?.PlaySpecialAbility();
                Debug.Log($"⚡ Special: {specialDamage} obrażeń dla {enemy.name}!");
            }
        }
    }

    void PushbackEnemy(EnemyHealth enemy, float forceMultiplier = 1f)
    {
        if (enemy == null) return;

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            direction.y = pushbackUpForce;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * pushbackForce * forceMultiplier, ForceMode.Impulse);
        }
    }

    public void SetCanAttack(bool value)
    {
        canAttack = value;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stompRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ultimateRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, specialRange);

        Vector3 forward = transform.forward;
        Quaternion leftRotation = Quaternion.Euler(0, -attackAngle / 2, 0);
        Quaternion rightRotation = Quaternion.Euler(0, attackAngle / 2, 0);
        Vector3 leftDirection = leftRotation * forward * attackRange;
        Vector3 rightDirection = rightRotation * forward * attackRange;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftDirection);
        Gizmos.DrawLine(transform.position, transform.position + rightDirection);
    }
}