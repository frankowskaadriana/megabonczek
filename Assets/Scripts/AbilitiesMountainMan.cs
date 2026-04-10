using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("Spin")]
    public float damage = 20f;
    public float cooldown = 3f;
    public float SpinRange = 3f;
    public float SpinTime = 1f;
    public KeyCode SpinKey = KeyCode.E;
    public GameObject SpinHitBox;

    [Header("Berserk")]
    public float ultCd = 10f;
    public float Duration = 5f;
    public float ResistValue = 0.5f;
    public float HealValue = 30f;
    public float LifeStealValue = 0.2f;

    private bool isSpinOnCooldown = false;
    private bool isSpinning = false;
    private float currentCooldown = 0f;
    private GameObject currentSpinHitBox;

    void Start()
    {
        // Ukryj hitbox na starcie
        if (SpinHitBox != null)
        {
            SpinHitBox.SetActive(false);
            currentSpinHitBox = SpinHitBox;
        }
    }

    void Update()
    {
        // Obs³uga cooldownu
        if (isSpinOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                isSpinOnCooldown = false;
                Debug.Log("Spin ability is ready again!");
            }
        }

        // Sprawdzanie inputu dla ataku wiruj¹cego
        if (Input.GetKeyDown(SpinKey) && !isSpinOnCooldown && !isSpinning)
        {
            StartCoroutine(PerformSpin());
        }
    }

    IEnumerator PerformSpin()
    {
        isSpinning = true;

        // Aktywuj hitbox
        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.SetActive(true);
            Debug.Log("Spin attack activated!");
        }

        // Zadaj obra¿enia natychmiast po aktywacji
        DealSpinDamage();

        // Czekaj przez czas trwania ataku
        yield return new WaitForSeconds(SpinTime);

        // Dezaktywuj hitbox
        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.SetActive(false);
        }

        isSpinning = false;

        // Rozpocznij cooldown
        isSpinOnCooldown = true;
        currentCooldown = cooldown;
        Debug.Log($"Spin ability on cooldown for {cooldown} seconds");
    }

    void DealSpinDamage()
    {
        // ZnajdŸ wszystkie obiekty w zasiêgu wiruj¹cego ataku
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, SpinRange);

        foreach (var hitCollider in hitColliders)
        {
            // SprawdŸ czy trafiony obiekt to wróg (ma komponent enemyHealth)
            enemyHealth enemy = hitCollider.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                // Zadaj obra¿enia wrogowi
                enemy.health -= damage;

                // Aktualizuj tekst zdrowia jeœli istnieje
                if (enemy.healthText != null)
                {
                    enemy.healthText.text = enemy.health.ToString();
                }

                Debug.Log($"Spin hit {hitCollider.name} for {damage} damage!");

                // SprawdŸ czy wróg nie ¿yje
                if (enemy.health <= 0)
                {
                    Destroy(enemy.gameObject);
                    Debug.Log($"Enemy {hitCollider.name} defeated by spin attack!");
                }
            }
        }
    }

    // Metoda do wizualizacji zasiêgu ataku w edytorze
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SpinRange);
    }

    // Publiczna metoda do sprawdzenia czy umiejêtnoœæ jest dostêpna
    public bool IsSpinAvailable()
    {
        return !isSpinOnCooldown && !isSpinning;
    }

    // Publiczna metoda do uzyskania aktualnego cooldownu
    public float GetCurrentCooldown()
    {
        return currentCooldown;
    }
    void Berserk()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Dodaj logikê do zwiêkszenia odpornoœci, leczenia i kradzie¿y ¿ycia
            // Aktywuj tryb Berserk
            //StartCoroutine(ActivateBerserk());
        }
    }
}