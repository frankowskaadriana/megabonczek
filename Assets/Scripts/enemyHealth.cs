using UnityEngine;

public class enemyHealth : MonoBehaviour
{
    public GameObject Player;
    public float health = 100f; // Enemy's health
    public TMPro.TextMeshPro healthText; // Reference to the TextMeshPro component for displaying health

    void Start()
    {
        healthText.text = health.ToString(); // Initialize the health text with the current health value
    }

    void Update()
    {
        TextFacePlayer();
    }

    void TextFacePlayer()
    {
        Vector3 direction = Player.transform.position - healthText.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        healthText.transform.rotation = rotation * Quaternion.Euler(0, 180, 0); // Odwróæ o 180 stopni
    }

    // Poprawiona metoda - dodaj OnCollisionEnter zamiast OnBulletTouch w Update
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            health -= 10f; // Zmniejsz zdrowie o 10
            healthText.text = health.ToString(); // Aktualizuj tekst zdrowia

            if (health <= 0)
            {
                Destroy(gameObject); // Zniszcz wroga, gdy zdrowie spadnie do 0 lub poni¿ej
            }

            Destroy(collision.gameObject); // Zniszcz pocisk po trafieniu
        }
    }
}