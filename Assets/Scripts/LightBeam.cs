using UnityEngine;
using System.Collections;

public class LightBeam : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA ═══════════════")]
    public float damage = 15f;
    public float range = 10f;
    public float duration = 0.5f;
    public float speed = 30f;

    [Header("═══════════════ ODRZUT ═══════════════")]
    public float pushbackForce = 8f;
    public float pushbackUpForce = 1f;

    [Header("═══════════════ WIZUALIZACJA ═══════════════")]
    public float beamWidth = 0.3f;
    public Color beamColor = Color.cyan;

    private float traveled = 0f;
    private bool hitSomething = false;
    private LineRenderer lineRenderer;
    private Vector3 startPosition;
    private Vector3 direction;
    private GameObject visualObj;

    void Start()
    {
        startPosition = transform.position;
        direction = transform.forward.normalized;

        CreateVisual();
        StartCoroutine(Lifetime());

        Debug.Log($"⚡ LightBeam wystrzelony! Kierunek: {direction}");
    }

    void CreateVisual()
    {
        visualObj = new GameObject("BeamVisual");
        visualObj.transform.SetParent(transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localRotation = Quaternion.identity;

        lineRenderer = visualObj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.sortingOrder = 10;

        lineRenderer.startWidth = beamWidth;
        lineRenderer.endWidth = beamWidth * 0.3f;
        lineRenderer.startColor = beamColor;
        lineRenderer.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);

        Material mat = new Material(Shader.Find("Sprites/Default"));
        if (mat == null) mat = new Material(Shader.Find("UI/Default"));
        mat.color = beamColor;
        lineRenderer.material = mat;

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, startPosition + direction * 0.5f);
        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (hitSomething) return;

        float move = speed * Time.deltaTime;
        transform.position += direction * move;
        traveled += move;

        if (lineRenderer != null && lineRenderer.enabled)
        {
            Vector3 endPoint = transform.position + direction * 0.5f;
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPoint);
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 1f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
                if (baseEnemy != null)
                {
                    baseEnemy.TakeDamage(damage);
                    PushbackEnemy(baseEnemy);
                    hitSomething = true;
                    AudioManager.Instance?.PlayLaser();
                    Destroy(gameObject);
                    return;
                }

                Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
                if (bazyliszek != null)
                {
                    bazyliszek.TakeDamage(damage);
                    PushbackEnemy(bazyliszek);
                    hitSomething = true;
                    AudioManager.Instance?.PlayLaser();
                    Destroy(gameObject);
                    return;
                }

                Leszy leszy = hit.collider.GetComponent<Leszy>();
                if (leszy != null)
                {
                    leszy.TakeDamage(damage);
                    PushbackEnemy(leszy);
                    hitSomething = true;
                    AudioManager.Instance?.PlayLaser();
                    Destroy(gameObject);
                    return;
                }
            }
        }

        if (traveled >= range)
        {
            hitSomething = true;
            if (lineRenderer != null) lineRenderer.enabled = false;
            Destroy(gameObject);
        }
    }

    void PushbackEnemy(object enemy)
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
            Vector3 dir = (enemyTransform.position - transform.position).normalized;
            dir.y = pushbackUpForce;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(dir * pushbackForce, ForceMode.Impulse);
        }
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);

        if (lineRenderer != null) lineRenderer.enabled = false;
        Destroy(gameObject);
    }

    // ============================================
    // METODY PUBLICZNE
    // ============================================

    public void SetBeam(float damage, float range, float duration, float speed = 30f)
    {
        this.damage = damage;
        this.range = range;
        this.duration = duration;
        this.speed = speed;
    }

    // === SETPUSHBACK - TERAZ 2 ARGUMENTY ===
    public void SetPushback(float force, float upForce)
    {
        this.pushbackForce = force;
        this.pushbackUpForce = upForce;
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void OnDestroy()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;
        if (visualObj != null) Destroy(visualObj);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + direction * range);
        Gizmos.DrawWireSphere(transform.position + direction * range, 0.2f);
    }
}