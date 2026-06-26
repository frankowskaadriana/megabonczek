using UnityEngine;
using System.Collections;

public class LightBeam : MonoBehaviour
{
    public float damage = 10f;
    public float range = 10f;
    public float duration = 0.5f;
    public float speed = 25f;

    private float traveled = 0f;
    private bool hitSomething = false;

    void Start() => StartCoroutine(Lifetime());

    void Update()
    {
        if (hitSomething) return;

        float move = speed * Time.deltaTime;
        transform.Translate(Vector3.forward * move);
        traveled += move;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 1f))
        {
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                hitSomething = true;
            }
        }

        if (traveled >= range) hitSomething = true;
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}