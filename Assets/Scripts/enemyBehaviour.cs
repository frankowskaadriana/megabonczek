using UnityEngine;
using System.Collections;

public class enemyBehaviour : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float moveForce = 5f; // Si³a poruszania siê w stronê gracza
    public float maxSpeed = 2f; // Maksymalna prêdkoœæ
    public float RotationSpeed = 5f; // Prêdkoœæ obracania siê w stronê gracza
    public float climbForce = 5f; // Si³a wspinania siê w górê
    public float waitTimeToClimb = 2f; // Czas oczekiwania przed rozpoczêciem wspinaczki

    [Header("Push Settings")]
    public float pushForce = 10f; // Si³a odpychania
    public float timeToPush = 2f; // Czas po jakim wróg zostanie odepchniêty (w sekundach)

    private Rigidbody rb;
    private bool isClimbing = false;
    private float collisionTimer = 0f; // Timer licz¹cy czas kolizji
    private GameObject currentCollidingEnemy = null; // Przeciwnik, z którym jest kolizja

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Jeœli nie ma Rigidbody, dodaj go
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // ZamroŸ rotacjê, ¿eby wróg nie przewraca³ siê podczas ruchu
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;
    }

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

            // Dodaj si³ê w kierunku gracza
            rb.AddForce(direction * moveForce, ForceMode.Force);

            // Ogranicz prêdkoœæ do maksymalnej
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }

            // Obrót w stronê gracza
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
    }

    void ClimbVerticallyOverTime()
    {
        // Dodaj si³ê do góry
        rb.AddForce(Vector3.up * climbForce, ForceMode.Force);

        // Opcjonalnie: ogranicz prêdkoœæ wspinaczki
        if (rb.linearVelocity.y > maxSpeed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxSpeed, rb.linearVelocity.z);
        }
    }

    // Kolizja rozpoczêta
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentCollidingEnemy = collision.gameObject;
            collisionTimer = 0f; // Resetuj timer
            StartCoroutine(PushCoroutine());
        }
    }

    // Kolizja trwa
    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collisionTimer += Time.deltaTime;
        }
    }

    // Kolizja zakoñczona
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            currentCollidingEnemy = null;
            collisionTimer = 0f;
            StopCoroutine(PushCoroutine());
        }
    }

    // Coroutine do odpychania
    IEnumerator PushCoroutine()
    {
        while (collisionTimer < timeToPush && currentCollidingEnemy != null)
        {
            yield return null; // Czekaj
        }

        // Jeœli min¹³ czas i nadal jest kolizja, odepchnij
        if (collisionTimer >= timeToPush && currentCollidingEnemy != null)
        {
            PushAway(currentCollidingEnemy);
        }
    }

    // Odpychanie w losow¹ stronê
    void PushAway(GameObject otherEnemy)
    {
        // Losowy kierunek (poziomy)
        Vector3 randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        // Dodaj si³ê odpychaj¹c¹ do obu przeciwników
        Rigidbody otherRb = otherEnemy.GetComponent<Rigidbody>();

        if (otherRb != null)
        {
            otherRb.AddForce(randomDirection * pushForce, ForceMode.Impulse);
        }

        // Odepchnij równie¿ tego przeciwnika w przeciwn¹ stronê
        rb.AddForce(-randomDirection * pushForce, ForceMode.Impulse);

        // Resetuj timer po odepchniêciu
        collisionTimer = 0f;
        currentCollidingEnemy = null;

        Debug.Log($"{gameObject.name} zosta³ odepchniêty od {otherEnemy.name}");
    }

    // Opcjonalnie: metoda do resetowania pozycji (np. po spadniêciu)
    void ResetEnemy()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}