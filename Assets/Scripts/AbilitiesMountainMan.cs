using UnityEngine;
using System.Collections;

public class AbilitiesMountainMan : MonoBehaviour
{
    [Header("Spin - Base Values")]
    public float baseDamage = 20f;
    public float baseSpinRange = 3f;

    [Header("Spin - Current Values")]
    public float currentDamage;
    public float currentSpinRange;

    [Header("Multipliers (from Level System)")]
    public float damageMultiplier = 1f;
    public float rangeMultiplier = 1f;

    [Header("Spin Settings")]
    public float cooldown = 3f;
    public float SpinTime = 1f;
    public KeyCode SpinKey = KeyCode.E;
    public GameObject SpinHitBox;

    [Header("Sword Slash - Auto Attack")]
    public float slashBaseDamage = 15f;
    public float slashBaseRange = 2f;
    public float autoAttackInterval = 2f; // Atak automatycznie co 2 sekundy
    public float slashDuration = 0.2f;
    public GameObject slashHitBox; // Podepnij hitbox zamachu

    [Header("Berserk")]
    public float ultCd = 10f;
    public float Duration = 5f;
    public float ResistValue = 0.5f;
    public float HealValue = 30f;
    public float LifeStealValue = 0.2f;

    [Header("References")]
    public PlayerHealth playerHealthReference;

    [Header("Visual Effects")]
    public Material berserkMaterial;
    public MeshRenderer capsuleRenderer;
    private Material originalMaterial;
    private Color originalColor;

    // Spin
    private bool isSpinOnCooldown = false;
    private bool isSpinning = false;
    private float currentCooldown = 0f;
    private GameObject currentSpinHitBox;

    // Slash (Auto Attack)
    private float currentSlashDamage;
    private float currentSlashRange;
    private bool isSlashing = false;
    private float autoAttackTimer = 0f;

    // Berserk
    private bool isBerserkActive = false;
    private float berserkCooldown = 0f;
    private bool isBerserkOnCooldown = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        // Inicjalizacja
        currentDamage = baseDamage;
        currentSpinRange = baseSpinRange;
        currentSlashDamage = slashBaseDamage;
        currentSlashRange = slashBaseRange;

        // Ustaw timer na pierwszy atak
        autoAttackTimer = autoAttackInterval;

        // Spin hitbox
        if (SpinHitBox != null)
        {
            SpinHitBox.SetActive(false);
            currentSpinHitBox = SpinHitBox;
        }

        // Slash hitbox
        if (slashHitBox != null)
        {
            slashHitBox.SetActive(false);
        }

        // Visual effects
        if (capsuleRenderer == null)
        {
            capsuleRenderer = GetComponent<MeshRenderer>();
            if (capsuleRenderer == null)
                capsuleRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (capsuleRenderer != null)
        {
            originalMaterial = capsuleRenderer.material;
            originalColor = capsuleRenderer.material.color;
        }

        // Player health
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
            Debug.LogError("PlayerHealth component not found!");
        }
    }

    void Update()
    {
        // Cooldown spina
        if (isSpinOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
                isSpinOnCooldown = false;
        }

        // Cooldown berserka
        if (isBerserkOnCooldown)
        {
            berserkCooldown -= Time.deltaTime;
            if (berserkCooldown <= 0)
                isBerserkOnCooldown = false;
        }

        // AUTOMATYCZNY ZAMACH CO 2 SEKUNDY
        if (!isSlashing && !isSpinning)
        {
            autoAttackTimer -= Time.deltaTime;
            if (autoAttackTimer <= 0f)
            {
                StartCoroutine(PerformSlash());
                autoAttackTimer = autoAttackInterval; // Reset timera
            }
        }

        // Spin - klawisz E
        if (Input.GetKeyDown(SpinKey) && !isSpinOnCooldown && !isSpinning)
        {
            StartCoroutine(PerformSpin());
        }

        // Berserk - klawisz Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isBerserkOnCooldown && !isBerserkActive)
            {
                StartCoroutine(ActivateBerserk());
            }
        }
    }

    // ==================== AUTOMATYCZNY ZAMACH MIECZEM ====================
    IEnumerator PerformSlash()
    {
        isSlashing = true;

        // Aktywuj hitbox
        if (slashHitBox != null)
        {
            slashHitBox.SetActive(true);
            Debug.Log($"Automatyczny zamach! Obra¿enia: {currentSlashDamage}, Zasiêg: {currentSlashRange}");
        }

        // Zadaj obra¿enia
        DealSlashDamage();

        // Poczekaj chwilê
        yield return new WaitForSeconds(slashDuration);

        // Dezaktywuj hitbox
        if (slashHitBox != null)
        {
            slashHitBox.SetActive(false);
        }

        isSlashing = false;
    }

    void DealSlashDamage()
    {
        // U¿yj hitboxa do zadawania obra¿eñ
        if (slashHitBox != null)
        {
            Collider[] hitColliders = Physics.OverlapSphere(slashHitBox.transform.position, currentSlashRange);

            foreach (var hitCollider in hitColliders)
            {
                enemyHealth enemy = hitCollider.GetComponent<enemyHealth>();
                if (enemy != null)
                {
                    float finalDamage = currentSlashDamage;

                    if (isBerserkActive)
                    {
                        finalDamage *= 2f;
                        if (playerHealth != null)
                            playerHealth.Heal(finalDamage * LifeStealValue);
                    }

                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"Zamach trafi³ {enemy.name} za {finalDamage} obra¿eñ!");
                }
            }
        }
        else
        {
            // Alternatywa: sfera wokó³ gracza jeœli brak hitboxa
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentSlashRange);

            foreach (var hitCollider in hitColliders)
            {
                enemyHealth enemy = hitCollider.GetComponent<enemyHealth>();
                if (enemy != null)
                {
                    float finalDamage = currentSlashDamage;

                    if (isBerserkActive)
                    {
                        finalDamage *= 2f;
                        if (playerHealth != null)
                            playerHealth.Heal(finalDamage * LifeStealValue);
                    }

                    enemy.TakeDamage(finalDamage);
                    Debug.Log($"Zamach trafi³ {enemy.name} za {finalDamage} obra¿eñ!");
                }
            }
        }
    }

    // ==================== SPIN ====================
    IEnumerator PerformSpin()
    {
        isSpinning = true;
        if (currentSpinHitBox != null)
            currentSpinHitBox.SetActive(true);

        DealSpinDamage();
        yield return new WaitForSeconds(SpinTime);

        if (currentSpinHitBox != null)
            currentSpinHitBox.SetActive(false);

        isSpinning = false;
        isSpinOnCooldown = true;
        currentCooldown = cooldown;
    }

    void DealSpinDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, currentSpinRange);
        foreach (var hitCollider in hitColliders)
        {
            enemyHealth enemy = hitCollider.GetComponent<enemyHealth>();
            if (enemy != null)
            {
                float finalDamage = currentDamage;
                if (isBerserkActive)
                {
                    finalDamage *= 2f;
                    if (playerHealth != null)
                        playerHealth.Heal(finalDamage * LifeStealValue);
                }
                enemy.TakeDamage(finalDamage);
            }
        }
    }

    // ==================== BERSERK ====================
    IEnumerator ActivateBerserk()
    {
        isBerserkActive = true;
        ChangeColorToRed();

        if (playerHealth != null)
        {
            playerHealth.Heal(HealValue);
            playerHealth.isInvincible = true;
        }

        yield return new WaitForSeconds(Duration);

        isBerserkActive = false;
        isBerserkOnCooldown = true;
        berserkCooldown = ultCd;
        RestoreOriginalColor();

        if (playerHealth != null)
            playerHealth.isInvincible = false;
    }

    // ==================== UPDATE STATYSTYK ====================
    public void UpdateDamage()
    {
        currentDamage = baseDamage * damageMultiplier;
        currentSlashDamage = slashBaseDamage * damageMultiplier;
        Debug.Log($"Obra¿enia spina: {currentDamage}, Obra¿enia zamachu: {currentSlashDamage}");
    }

    public void UpdateRange()
    {
        currentSpinRange = baseSpinRange * rangeMultiplier;
        currentSlashRange = slashBaseRange * rangeMultiplier;

        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.transform.localScale = Vector3.one * (currentSpinRange / baseSpinRange);
        }

        Debug.Log($"Zasiêg spina: {currentSpinRange}, Zasiêg zamachu: {currentSlashRange}");
    }

    // ==================== WIZUALIZACJA ====================
    void ChangeColorToRed()
    {
        if (capsuleRenderer != null)
        {
            if (berserkMaterial != null)
                capsuleRenderer.material = berserkMaterial;
            else
                capsuleRenderer.material.color = Color.red;
        }
    }

    void RestoreOriginalColor()
    {
        if (capsuleRenderer != null)
        {
            if (originalMaterial != null)
                capsuleRenderer.material = originalMaterial;
            else
                capsuleRenderer.material.color = originalColor;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentSpinRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, currentSlashRange);
    }

    // ==================== GETTERY ====================
    public bool IsSpinAvailable() => !isSpinOnCooldown && !isSpinning;
    public float GetCurrentCooldown() => currentCooldown;
    public bool IsBerserkActive() => isBerserkActive;
    public bool IsSlashing() => isSlashing;
}