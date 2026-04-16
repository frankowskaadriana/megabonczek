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
        currentDamage = baseDamage;
        currentSpinRange = baseSpinRange;

        if (SpinHitBox != null)
        {
            SpinHitBox.SetActive(false);
            currentSpinHitBox = SpinHitBox;
        }

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
        if (isSpinOnCooldown)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0)
            {
                isSpinOnCooldown = false;
            }
        }

        if (isBerserkOnCooldown)
        {
            berserkCooldown -= Time.deltaTime;
            if (berserkCooldown <= 0)
            {
                isBerserkOnCooldown = false;
            }
        }

        if (Input.GetKeyDown(SpinKey) && !isSpinOnCooldown && !isSpinning)
        {
            StartCoroutine(PerformSpin());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isBerserkOnCooldown && !isBerserkActive)
            {
                StartCoroutine(ActivateBerserk());
            }
        }
    }

    public void UpdateDamage()
    {
        currentDamage = baseDamage * damageMultiplier;
        Debug.Log($"Obra¿enia: {currentDamage}");
    }

    public void UpdateRange()
    {
        currentSpinRange = baseSpinRange * rangeMultiplier;
        if (currentSpinHitBox != null)
        {
            currentSpinHitBox.transform.localScale = Vector3.one * (currentSpinRange / baseSpinRange);
        }
    }

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
    }

    public bool IsSpinAvailable() => !isSpinOnCooldown && !isSpinning;
    public float GetCurrentCooldown() => currentCooldown;
    public bool IsBerserkActive() => isBerserkActive;
}