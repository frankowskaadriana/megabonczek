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

    [Header("References")]
    public PlayerHealth playerHealthReference;

    [Header("Visual Effects")]
    public Material berserkMaterial; // Podepnij czerwony materia³
    public MeshRenderer capsuleRenderer; // Renderer kapsu³ki
    private Material originalMaterial; // Zapamiêtaj oryginalny materia³
    private Color originalColor; // Zapamiêtaj oryginalny kolor

    private bool isSpinOnCooldown = false;
    private bool isSpinning = false;
    private float currentCooldown = 0f;
    private GameObject currentSpinHitBox;

    private bool isBerserkActive = false;
    private float berserkCooldown = 0f;
    private bool isBerserkOnCooldown = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        if (SpinHitBox != null)
        {
            SpinHitBox.SetActive(false);
            currentSpinHitBox = SpinHitBox;
        }

        // ZnajdŸ renderer kapsu³ki jeœli nie podpiêty
        if (capsuleRenderer == null)
        {
            capsuleRenderer = GetComponent<MeshRenderer>();
            if (capsuleRenderer == null)
                capsuleRenderer = GetComponentInChildren<MeshRenderer>();
        }

        // Zapamiêtaj oryginalny kolor
        if (capsuleRenderer != null)
        {
            originalMaterial = capsuleRenderer.material;
            originalColor = capsuleRenderer.material.color;
        }

        // Najpierw sprawdŸ referencjê z Inspektora
        if (playerHealthReference != null)
        {
            playerHealth = playerHealthReference;
        }
        else
        {
            playerHealth = GetComponent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
                playerHealth = GetComponentInChildren<PlayerHealth>();
            if (playerHealth == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                    playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth component not found! Berserk will not work! Please assign playerHealthReference in Inspector.");
        }
        else
        {
            Debug.Log("PlayerHealth found! Berserk ready to use.");
        }
    }

    void Update()
    {
        if (isSpinOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                isSpinOnCooldown = false;
                Debug.Log("Spin ability is ready again!");
            }
        }

        if (isBerserkOnCooldown)
        {
            berserkCooldown -= Time.deltaTime;
            if (berserkCooldown <= 0)
            {
                isBerserkOnCooldown = false;
                Debug.Log("Berserk is ready again!");
            }
        }

        if (Input.GetKeyDown(SpinKey) && !isSpinOnCooldown && !isSpinning)
        {
            StartCoroutine(PerformSpin());
        }

        // Berserk na klawisz Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q pressed - Berserk attempt");
            if (!isBerserkOnCooldown && !isBerserkActive)
            {
                StartCoroutine(ActivateBerserk());
            }
            else
            {
                if (isBerserkOnCooldown) Debug.Log("Berserk on cooldown!");
                if (isBerserkActive) Debug.Log("Berserk already active!");
            }
        }
    }

    IEnumerator PerformSpin()
    {
        isSpinning = true;

        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.SetActive(true);
            Debug.Log("Spin attack activated!");
        }

        DealSpinDamage();

        yield return new WaitForSeconds(SpinTime);

        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.SetActive(false);
        }

        isSpinning = false;
        isSpinOnCooldown = true;
        currentCooldown = cooldown;
        Debug.Log($"Spin ability on cooldown for {cooldown} seconds");
    }

    void DealSpinDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, SpinRange);

        foreach (var hitCollider in hitColliders)
        {
            enemyHealth enemy = hitCollider.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                float finalDamage = damage;

                if (isBerserkActive)
                {
                    finalDamage *= 2f;
                    enemy.health -= finalDamage;

                    if (playerHealth != null)
                    {
                        playerHealth.HeathValue += finalDamage * LifeStealValue;
                        playerHealth.HeathValue = Mathf.Min(playerHealth.HeathValue, 100f);
                    }
                    Debug.Log($"Berserk bonus! Double damage + life steal!");
                }
                else
                {
                    enemy.health -= finalDamage;
                }

                if (enemy.healthText != null)
                {
                    enemy.healthText.text = enemy.health.ToString();
                }

                Debug.Log($"Spin hit {hitCollider.name} for {finalDamage} damage!");

                if (enemy.health <= 0)
                {
                    Destroy(enemy.gameObject);
                    Debug.Log($"Enemy {hitCollider.name} defeated by spin attack!");
                }
            }
        }
    }

    IEnumerator ActivateBerserk()
    {
        Debug.Log("Activating Berserk!");
        isBerserkActive = true;

        // ZMIANA KOLORU NA CZERWONY
        ChangeColorToRed();

        // Lecz gracza
        if (playerHealth != null)
        {
            playerHealth.HeathValue += HealValue;
            playerHealth.HeathValue = Mathf.Min(playerHealth.HeathValue, 100f);
            playerHealth.isInvincible = true;
            Debug.Log($"Berserk healed for {HealValue}! Current health: {playerHealth.HeathValue}");
            Debug.Log("BERSERK ACTIVATED! You cannot die for " + Duration + " seconds!");
        }
        else
        {
            Debug.LogError("PlayerHealth is null! Cannot activate Berserk!");
        }

        yield return new WaitForSeconds(Duration);

        isBerserkActive = false;
        isBerserkOnCooldown = true;
        berserkCooldown = ultCd;

        // PRZYWRÓÆ ORYGINALNY KOLOR
        RestoreOriginalColor();

        if (playerHealth != null)
        {
            playerHealth.isInvincible = false;
        }

        Debug.Log("Berserk ended! Cooldown: " + ultCd + " seconds");
    }

    void ChangeColorToRed()
    {
        if (capsuleRenderer != null)
        {
            if (berserkMaterial != null)
            {
                capsuleRenderer.material = berserkMaterial;
                Debug.Log("Changed to Berserk material (RED)!");
            }
            else
            {
                capsuleRenderer.material.color = Color.red;
                Debug.Log("Changed color to RED!");
            }
        }
        else
        {
            Debug.LogWarning("No MeshRenderer found! Cannot change color.");
        }
    }

    void RestoreOriginalColor()
    {
        if (capsuleRenderer != null)
        {
            if (originalMaterial != null)
            {
                capsuleRenderer.material = originalMaterial;
                Debug.Log("Restored original material!");
            }
            else
            {
                capsuleRenderer.material.color = originalColor;
                Debug.Log("Restored original color!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SpinRange);
    }

    public bool IsSpinAvailable()
    {
        return !isSpinOnCooldown && !isSpinning;
    }

    public float GetCurrentCooldown()
    {
        return currentCooldown;
    }

    public bool IsBerserkActive()
    {
        return isBerserkActive;
    }
}