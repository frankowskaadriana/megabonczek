using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public float damage = 30f;
    public float speed = 20f;
    public int pierceCount = 0;
    public bool canPierce = false;
    public float lifetime = 2f;

    private int currentPierce = 0;
    private Vector3 direction;

    void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyHealth enemy = other.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                if (!canPierce || currentPierce >= pierceCount)
                {
                    Destroy(gameObject);
                }
                else
                {
                    currentPierce++;
                }
            }
        }
        else if (!other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}