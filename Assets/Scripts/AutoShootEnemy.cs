using UnityEngine;
using System.Collections;

public class AutoShootEnemy : MonoBehaviour
{
    public float shootDelay = 2f;
    public float bulletSpeed = 10f;
    public float shootRange = 20f; // Maksymalny zasiêg strzelania

    void Start()
    {
        // Rozpocznij strzelanie w pêtli
        StartCoroutine(ShootRoutine());
    }

    IEnumerator ShootRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootDelay);
            ShootAtClosestEnemy();
        }
    }

    public void ShootAtClosestEnemy()
    {
        // ZnajdŸ wszystkich wrogów
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0) return;

        // ZnajdŸ najbli¿szego wroga
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance < closestDistance && distance <= shootRange)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        // Jeœli znaleziono wroga, strzelaj
        if (closestEnemy != null && gameObject.activeInHierarchy)
        {
            // Oblicz kierunek do najbli¿szego wroga
            Vector3 direction = (closestEnemy.transform.position - transform.position).normalized;

            // Pobierz pocisk z pool
            GameObject bullet = BulletPool.Instance.GetBullet(
                transform.position,
                Quaternion.LookRotation(direction)
            );

            // Ustaw prêdkoœæ pocisku
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.speed = bulletSpeed;
            }
        }
    }

    // Opcjonalnie: wizualizacja zasiêgu w edytorze
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootRange);
    }
}