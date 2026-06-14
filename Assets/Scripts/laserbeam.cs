using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA LASERA ═══════════════")]
    public float damage = 60f;
    public float lifetime = 0.3f;
    public Gradient laserGradient;

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.positionCount = 2;

        if (laserGradient != null)
            lineRenderer.colorGradient = laserGradient;
        else
        {
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = new Color(1f, 0.5f, 0f);
        }

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.forward * transform.localScale.z);

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"⚡ Laser trafił gracza! Obrażenia: {damage}");
            }
        }
    }
}