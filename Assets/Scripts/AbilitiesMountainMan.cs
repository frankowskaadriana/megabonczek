using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("Podstawowy Atak (Automatyczna Ciupaga)")]
    public float attackDamage = 50f;
    public float attackRange = 1.5f;
    public float attackAngle = 90f;
    public float attackRate = 0.8f;

    [Header("Gniew Tatr (Q)")]
    public float specialDamage = 80f;
    public float specialCooldown = 20f;
    public int specialRotations = 1;
    public float healValue = 30f;
    public float specialRange = 3f;

    [Header("Orli Grom (R)")]
    public float ultimateDuration = 10f;
    public float ultimateRadius = 1.25f;
    public float ultimateDamage = 50f;

    [Header("Wizualizacje")]
    public Material indicatorMaterial;
    public Texture2D gradientTexture; // Opcjonalna tekstura dla gradientu

    [Header("References")]
    public PlayerHealth playerHealth;
    public WeaponUpgradeSystem weaponUpgrade;

    private float attackTimer = 0f;
    private bool isSpecialOnCooldown = false;
    private bool isUltimateOnCooldown = false;
    private float specialCooldownTimer = 0f;
    private float ultimateCooldownTimer = 0f;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;

    // LineRenderer dla wskaźników
    private LineRenderer attackLine;
    private LineRenderer specialLine;
    private LineRenderer ultimateLine;
    private MeshRenderer attackMesh; // Do wypełnienia stożka

    void Start()
    {
        if (weaponUpgrade != null)
        {
            attackDamage = weaponUpgrade.currentDamage;
            attackRange = weaponUpgrade.currentRange;
            attackAngle = weaponUpgrade.currentSwingAngle;
            specialDamage = weaponUpgrade.currentSpecialDamage;
            specialCooldown = weaponUpgrade.currentSpecialCooldown;
            specialRotations = weaponUpgrade.currentSpecialRotations;
            ultimateDuration = weaponUpgrade.currentUltimateDuration;
            ultimateRadius = weaponUpgrade.currentUltimateRadius;
            ultimateDamage = weaponUpgrade.currentUltimateDamage;
        }

        CreateIndicators();
    }

    void CreateIndicators()
    {
        // === WSKAŹNIK ATAKU (stożek z wypełnieniem) ===
        GameObject attackObj = new GameObject("AttackIndicator");
        attackObj.transform.SetParent(transform);
        attackObj.transform.localPosition = new Vector3(0, 0.02f, 0);
        attackObj.transform.localRotation = Quaternion.identity;

        // Dodaj MeshRenderer dla wypełnienia
        MeshFilter mf = attackObj.AddComponent<MeshFilter>();
        attackMesh = attackObj.AddComponent<MeshRenderer>();
        attackMesh.material = new Material(Shader.Find("Sprites/Default"));
        attackMesh.material.color = new Color(1f, 0f, 0f, 0f); // Przezroczysty na start

        // Stwórz mesh dla wypełnienia stożka
        CreateFillMesh(mf);

        // Dodaj LineRenderer dla krawędzi (ładniejszy)
        attackLine = attackObj.AddComponent<LineRenderer>();
        SetupLineRenderer(attackLine, new Color(1f, 0.2f, 0.2f, 0.9f), 0.05f);
        UpdateAttackLine();

        // === WSKAŹNIK SPECJALNY (koło) ===
        GameObject specialObj = new GameObject("SpecialIndicator");
        specialObj.transform.SetParent(transform);
        specialObj.transform.localPosition = new Vector3(0, 0.02f, 0);
        specialLine = specialObj.AddComponent<LineRenderer>();
        SetupLineRenderer(specialLine, new Color(1f, 0.8f, 0f, 0.7f), 0.08f);
        specialLine.loop = true;
        UpdateCircleLine(specialLine, specialRange);
        specialObj.SetActive(false);

        // === WSKAŹNIK ULTIMATE (koło z wypełnieniem) ===
        GameObject ultimateObj = new GameObject("UltimateIndicator");
        ultimateObj.transform.SetParent(transform);
        ultimateObj.transform.localPosition = new Vector3(0, 0.01f, 0);
        ultimateLine = ultimateObj.AddComponent<LineRenderer>();
        SetupLineRenderer(ultimateLine, new Color(0.3f, 0.6f, 1f, 0.7f), 0.08f);
        ultimateLine.loop = true;
        UpdateCircleLine(ultimateLine, ultimateRadius);
        ultimateObj.SetActive(false);
    }

    void CreateFillMesh(MeshFilter mf)
    {
        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;
        float halfAngle = attackAngle / 2f;

        int segments = 30;
        Vector3[] vertices = new Vector3[segments + 2];
        int[] triangles = new int[segments * 3];

        vertices[0] = center;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            vertices[i + 1] = dir * attackRange;
        }

        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;
    }

    void SetupLineRenderer(LineRenderer lr, Color color, float width)
    {
        lr.startWidth = width;
        lr.endWidth = width;
        lr.useWorldSpace = false;

        // Ładniejszy materiał dla linii
        if (indicatorMaterial != null)
            lr.material = indicatorMaterial;
        else
            lr.material = new Material(Shader.Find("Sprites/Default"));

        lr.startColor = color;
        lr.endColor = color;
    }

    void UpdateAttackLine()
    {
        if (attackLine == null) return;

        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;
        float halfAngle = attackAngle / 2f;

        int segments = 30;
        int totalPoints = segments + 3;
        attackLine.positionCount = totalPoints;

        int pointIndex = 0;

        // Lewa krawędź
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        attackLine.SetPosition(pointIndex++, center);
        attackLine.SetPosition(pointIndex++, center + leftDir * attackRange);

        // Łuk
        Vector3 prevPoint = center + leftDir * attackRange;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + dir * attackRange;
            attackLine.SetPosition(pointIndex++, point);
            attackLine.SetPosition(pointIndex++, prevPoint);
            prevPoint = point;
        }

        // Prawa krawędź
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        attackLine.SetPosition(pointIndex++, center);
        attackLine.SetPosition(pointIndex++, center + rightDir * attackRange);
    }

    void UpdateCircleLine(LineRenderer lr, float radius)
    {
        if (lr == null) return;

        int segments = 60;
        lr.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            lr.SetPosition(i, new Vector3(x, 0f, z));
        }
    }

    void Update()
    {
        // Automatyczny atak
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackRate)
        {
            attackTimer = 0f;
            PerformBasicAttack();
            lastAttackTime = Time.time;
            StartCoroutine(AttackFlash());
        }

        // Aktualizuj wizualizację stożka podczas ataku
        if (attackMesh != null)
        {
            if (Time.time - lastAttackTime < 0.2f)
            {
                // Wypełnienie na czerwono podczas ataku
                attackMesh.material.color = new Color(1f, 0f, 0f, 0.5f);

                // Grubsza linia podczas ataku
                if (attackLine != null)
                {
                    attackLine.startWidth = 0.1f;
                    attackLine.endWidth = 0.1f;
                    attackLine.startColor = new Color(1f, 0.5f, 0.5f, 1f);
                    attackLine.endColor = new Color(1f, 0.5f, 0.5f, 1f);
                }
            }
            else
            {
                // Normalny stan - delikatne wypełnienie
                attackMesh.material.color = new Color(1f, 0f, 0f, 0.15f);

                if (attackLine != null)
                {
                    attackLine.startWidth = 0.05f;
                    attackLine.endWidth = 0.05f;
                    attackLine.startColor = new Color(1f, 0.2f, 0.2f, 0.8f);
                    attackLine.endColor = new Color(1f, 0.2f, 0.2f, 0.8f);
                }
            }
        }

        // Cooldowny
        if (isSpecialOnCooldown)
        {
            specialCooldownTimer -= Time.deltaTime;
            if (specialCooldownTimer <= 0)
            {
                isSpecialOnCooldown = false;
                if (specialLine != null) specialLine.gameObject.SetActive(false);
            }
            else
            {
                // Miganie podczas cooldownu
                if (specialLine != null)
                {
                    float alpha = 0.3f + Mathf.PingPong(Time.time * 3f, 0.7f);
                    Color c = new Color(1f, 0.8f, 0f, alpha);
                    specialLine.startColor = c;
                    specialLine.endColor = c;
                }
            }
        }

        if (isUltimateOnCooldown)
        {
            ultimateCooldownTimer -= Time.deltaTime;
            if (ultimateCooldownTimer <= 0)
            {
                isUltimateOnCooldown = false;
                if (ultimateLine != null) ultimateLine.gameObject.SetActive(false);
            }
            else
            {
                // Miganie podczas cooldownu
                if (ultimateLine != null)
                {
                    float alpha = 0.3f + Mathf.PingPong(Time.time * 3f, 0.7f);
                    Color c = new Color(0.3f, 0.6f, 1f, alpha);
                    ultimateLine.startColor = c;
                    ultimateLine.endColor = c;
                }
            }
        }

        // Umiejętności
        if (Input.GetKeyDown(KeyCode.Q) && !isSpecialOnCooldown)
        {
            StartCoroutine(PerformSpecial());
            if (specialLine != null)
            {
                specialLine.gameObject.SetActive(true);
                StartCoroutine(FlashLine(specialLine, Color.yellow, 0.3f));
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !isUltimateOnCooldown)
        {
            StartCoroutine(PerformUltimate());
            if (ultimateLine != null)
            {
                ultimateLine.gameObject.SetActive(true);
                StartCoroutine(FlashLine(ultimateLine, Color.cyan, 0.3f));
            }
        }
    }

    IEnumerator AttackFlash()
    {
        isAttacking = true;
        yield return new WaitForSeconds(0.15f);
        isAttacking = false;
    }

    IEnumerator FlashLine(LineRenderer lr, Color flashColor, float duration)
    {
        if (lr == null) yield break;

        Color originalColor = lr.startColor;
        float originalWidth = lr.startWidth;

        lr.startColor = flashColor;
        lr.endColor = flashColor;
        lr.startWidth = originalWidth * 1.5f;
        lr.endWidth = originalWidth * 1.5f;

        yield return new WaitForSeconds(duration);

        lr.startColor = originalColor;
        lr.endColor = originalColor;
        lr.startWidth = originalWidth;
        lr.endWidth = originalWidth;
    }

    void PerformBasicAttack()
    {
        int hitCount = 0;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToEnemy);

                if (angle <= attackAngle / 2)
                {
                    enemyHealth enemy = hit.GetComponent<enemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(attackDamage);
                        hitCount++;
                    }
                }
            }
        }

        if (hitCount > 0)
            Debug.Log($"⚔️ Ciupaga! Trafiono: {hitCount} wrogów");
    }

    IEnumerator PerformSpecial()
    {
        isSpecialOnCooldown = true;
        specialCooldownTimer = specialCooldown;

        if (playerHealth != null)
        {
            playerHealth.HeathValue += healValue;
            playerHealth.HeathValue = Mathf.Min(playerHealth.HeathValue, playerHealth.maxHealth);
            playerHealth.UpdateHealthUI();
        }

        for (int i = 0; i < specialRotations; i++)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, specialRange);
            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                    if (enemyScript != null)
                        enemyScript.TakeDamage(specialDamage);
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator PerformUltimate()
    {
        isUltimateOnCooldown = true;
        ultimateCooldownTimer = ultimateDuration;

        float elapsed = 0f;
        float tickTime = 0.2f;
        float tickCounter = 0f;

        while (elapsed < ultimateDuration)
        {
            tickCounter += Time.deltaTime;

            if (tickCounter >= tickTime)
            {
                tickCounter = 0f;

                Collider[] enemies = Physics.OverlapSphere(transform.position, ultimateRadius);
                foreach (Collider enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy"))
                    {
                        enemyHealth enemyScript = enemy.GetComponent<enemyHealth>();
                        if (enemyScript != null)
                        {
                            float damage = ultimateDamage * tickTime;
                            enemyScript.TakeDamage(damage);
                        }
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        float halfAngle = attackAngle / 2f;

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawRay(center, leftDir * attackRange);
        Gizmos.DrawRay(center, rightDir * attackRange);

        int segments = 20;
        Vector3 prevPoint = center + leftDir * attackRange;

        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float angle = -halfAngle + (attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + dir * attackRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center, specialRange);

        Gizmos.color = new Color(0.3f, 0.6f, 1f, 0.3f);
        Gizmos.DrawWireSphere(center, ultimateRadius);
    }
}