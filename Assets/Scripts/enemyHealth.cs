using UnityEngine;
using System.Collections;

public class enemyHealth : MonoBehaviour
{
    public GameObject Player;
    public float health = 100f; // Enemy's health
    public TMPro.TextMeshPro healthText; // Reference to the TextMeshPro component for displaying health

    private bool canTakeSpinDamage = true; // Flaga kontroluj¹ca cooldown obra¿eñ od spinu

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

    private void OnCollisionEnter(Collision collision)
    {
        // Obs³uga trafienia pociskiem
        if (collision.gameObject.CompareTag("Bullet"))
        {
            health -= 10f; // Zmniejsz zdrowie o 10
            healthText.text = health.ToString(); // Aktualizuj tekst zdrowia
            Destroy(collision.gameObject); // Zniszcz pocisk po trafieniu

            // SprawdŸ czy wróg nie ¿yje
            if (health <= 0)
            {
                Destroy(gameObject); // Zniszcz wroga, gdy zdrowie spadnie do 0 lub poni¿ej
            }
        }

        // Obs³uga trafienia atakiem wiruj¹cym
        if (collision.gameObject.CompareTag("SpinHitBox") && canTakeSpinDamage)
        {
            Debug.Log("Spin hitbox collision detected");
            health -= 20f; // Zmniejsz zdrowie o 20 dla ataku wiruj¹cego
            healthText.text = health.ToString(); // Aktualizuj tekst zdrowia

            // Rozpocznij cooldown dla obra¿eñ od spinu
            StartCoroutine(SpinDamageCooldown());

            // SprawdŸ czy wróg nie ¿yje
            if (health <= 0)
            {
                Destroy(gameObject); // Zniszcz wroga, gdy zdrowie spadnie do 0 lub poni¿ej
            }
        }
    }

    // Coroutine do obs³ugi cooldowna obra¿eñ od spinu
    IEnumerator SpinDamageCooldown()
    {
        canTakeSpinDamage = false; // Zablokuj mo¿liwoœæ zadawania obra¿eñ od spinu
        yield return new WaitForSeconds(0.5f); // Odczekaj 0.5 sekundy
        canTakeSpinDamage = true; // Odblokuj mo¿liwoœæ zadawania obra¿eñ od spinu
    }
}