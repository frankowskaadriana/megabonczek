using UnityEngine;
using System.Collections;

public class LightBeam : MonoBehaviour
{
    public float damage = 10f;
    public float range = 10f;
    public float duration = 0.5f;
    public float speed = 25f;

    public float pushbackForce = 5f;
    public float pushbackUpForce = 0.5f;
    public float pushbackInterval = 0.1f;

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

        SetupLineRenderer();
        StartCoroutine(Lifetime());
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.sortingOrder = 1;

        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = new Color(1f, 0f, 0f, 0.3f);

        if (lineRenderer.material == null)
        {
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position + transform.forward * range);

        lineRenderer.enabled = true;
    }

    void Update()
    {
        if (hitSomething) return;

        float move = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * move);
        traveled += move;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            BaseEnemy baseEnemy = hit.collider.GetComponent<BaseEnemy>();
            if (baseEnemy != null)
            {
                baseEnemy.TakeDamage(damage);
                PushbackEnemy(baseEnemy);
                hitSomething = true;
                AudioManager.Instance?.PlayLaser();
                return;
            }

            Bazyliszek bazyliszek = hit.collider.GetComponent<Bazyliszek>();
            if (bazyliszek != null)
            {
                bazyliszek.TakeDamage(damage);
                PushbackEnemy(bazyliszek);
                hitSomething = true;
                AudioManager.Instance?.PlayLaser();
                return;
            }

            Leszy leszy = hit.collider.GetComponent<Leszy>();
            if (leszy != null)
            {
                leszy.TakeDamage(damage);
                PushbackEnemy(leszy);
                hitSomething = true;
                AudioManager.Instance?.PlayLaser();
                return;
            }
        }

        if (!hitSomething && lineRenderer != null && lineRenderer.enabled)
        {
            Vector3 endPoint = transform.position + transform.forward * Mathf.Min(2f, range - traveled);
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPoint);
        }

        if (traveled >= range)
        {
            hitSomething = true;
            if (lineRenderer != null)
                lineRenderer.enabled = false;
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
            Vector3 direction = (enemyTransform.position - transform.position).normalized;
            direction.y = pushbackUpForce;

            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * pushbackForce, ForceMode.Impulse);
        }
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }

        Destroy(gameObject);
    }

    public void SetBeam(float damage, float range, float duration, float speed = 25f)
    {
        this.damage = damage;
        this.range = range;
        this.duration = duration;
        this.speed = speed;
    }

    public void SetPushback(float force, float upForce, float interval = 0.1f)
    {
        this.pushbackForce = force;
        this.pushbackUpForce = upForce;
        this.pushbackInterval = interval;
    }

    void OnDestroy()
    {
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }
}