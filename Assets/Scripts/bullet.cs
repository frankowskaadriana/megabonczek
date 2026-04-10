using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f;
    private float timer;

    void OnEnable()
    {
        timer = 0f;
        // Opcjonalnie: automatycznie zwróæ do pool po czasie
        Invoke(nameof(ReturnToPool), lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Zadaj obra¿enia
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        CancelInvoke();
        BulletPool.Instance.ReturnBullet(gameObject);
    }
}