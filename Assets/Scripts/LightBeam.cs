using UnityEngine;
using System.Collections;

public class LightBeam : MonoBehaviour
{
    [Header("Ustawienia")]
    public float damage = 10f;
    public float range = 10f;
    public float duration = 0.5f;
    public float speed = 25f;

    [Header("Odrzut")]
    public float pushbackForce = 5f;
    public float pushbackUpForce = 0.5f;
    public float pushbackInterval = 0.1f;

    [Header("Wizualizacja")]
    public float beamWidth = 0.15f;
    public Color beamColor = Color.red;
    public float beamStartWidth = 0.2f;
    public float beamEndWidth = 0.05f;

    private float traveled = 0f;
    private bool hitSomething = false;
    private float pushbackTimer = 0f;
    private LineRenderer lineRenderer;
    private Vector3 startPosition;
    private Vector3 direction;

    void Start()
    {
        startPosition = transform.position;
        direction = transform.forward;

        // Konfiguracja LineRenderer
        SetupLineRenderer();

        // Uruchom lifetime
        StartCoroutine(Lifetime());
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // Ustawienia LineRenderer
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.sortingOrder = 1;

        // Szerokość
        lineRenderer.startWidth = beamStartWidth;
        lineRenderer.endWidth = beamEndWidth;

        // Kolory
        lineRenderer.startColor = beamColor;
        lineRenderer.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);

        // Materiał
        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Ustaw początkowe pozycje
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * range);

        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (hitSomething) return;

        // Ruch do przodu
        float move = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * move);
        traveled += move;

        // Raycast do wykrywania wrogów
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null && !enemy.gameObject.CompareTag("Dead"))
            {
                enemy.TakeDamage(damage);

                // Odrzut co określony interwał
                pushbackTimer += Time.deltaTime;
                if (pushbackTimer >= pushbackInterval)
                {
                    pushbackTimer = 0f;
                    PushbackEnemy(enemy);
                }

                hitSomething = true;
                Debug.Log($"⚡ LightBeam trafił w {enemy.name}! Obrażenia: {damage}");
            }
        }

        // Aktualizuj LineRenderer - tylko jeśli nie trafił
        if (!hitSomething && lineRenderer != null && lineRenderer.enabled)
        {
            Vector3 endPoint = transform.position + transform.forward * Mathf.Min(2f, range - traveled);
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPoint);
        }

        // Sprawdź zasięg
        if (traveled >= range)
        {
            hitSomething = true;
            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }

    void PushbackEnemy(EnemyHealth enemy)
    {
        if (enemy == null) return;

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Kierunek od lightbeamu do wroga
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            direction.y = pushbackUpForce;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * pushbackForce, ForceMode.Impulse);

            Debug.Log($"💥 Odrzucono {enemy.name} przez LightBeam! Siła: {pushbackForce}");
        }
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);

        // Wyłącz LineRenderer przed zniszczeniem
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        Destroy(gameObject);
    }

    // ===== METODY PUBLICZNE =====

    public void SetBeam(float damage, float range, float duration, float speed = 25f)
    {
        this.damage = damage;
        this.range = range;
        this.duration = duration;
        this.speed = speed;
    }

    public void SetPushback(float force, float upForce, float interval = 0.1f)
    {
        pushbackForce = force;
        pushbackUpForce = upForce;
        pushbackInterval = interval;
    }

    public void SetBeamColor(Color color)
    {
        beamColor = color;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = new Color(color.r, color.g, color.b, 0.3f);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * range);
    }

    void OnDestroy()
    {
        // Cleanup
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}