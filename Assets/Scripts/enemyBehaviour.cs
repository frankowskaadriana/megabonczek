using UnityEngine;

public class enemyBehaviour : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveSpeed = 2f; // Prêdkoœæ poruszania siê w stronê gracza
    public float RotationSpeed = 5f; // Prêdkoœæ obracania siê w stronê gracza
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveToPlayer();
    }
   void MoveToPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;

            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
}
