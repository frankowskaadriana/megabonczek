using UnityEngine;
using System.Collections;

public class SeraphimAnimationController : MonoBehaviour
{
    [Header("═══════════════ REFERENCJE ═══════════════")]
    public Animator animator;
    public AbilitiesSeraphim abilitiesSeraphim;
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;

    [Header("═══════════════ USTAWIENIA ANIMACJI ═══════════════")]
    public float movementSmoothTime = 0.1f;
    public float floatAmplitude = 0.02f;
    public float floatSpeed = 2f;

    private float currentSpeed;
    private float targetSpeed;
    private bool isShooting = false;
    private bool isDying = false;
    private bool isCasting = false;
    private Vector3 startPosition;
    private float floatOffset;

    void Start()
    {
        startPosition = transform.position;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (abilitiesSeraphim == null)
            abilitiesSeraphim = GetComponent<AbilitiesSeraphim>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        // Ustaw domyślne parametry
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsFloating", true);
            animator.SetBool("IsDead", false);
        }

        Debug.Log("🎬 Seraphim Animation Controller gotowy!");
    }

    void Update()
    {
        if (isDying) return;

        UpdateMovementAnimation();
        UpdateShootingAnimation();
        UpdateFloatingAnimation();
    }

    void UpdateMovementAnimation()
    {
        if (playerMovement == null || animator == null) return;

        // Pobierz prędkość z PlayerMovement
        Vector3 velocity = playerMovement.GetVelocity();
        targetSpeed = velocity.magnitude;

        // Płynne przejście
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, movementSmoothTime);

        // Ustaw parametry animatora
        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("IsGrounded", playerMovement.IsGrounded());
    }

    void UpdateShootingAnimation()
    {
        if (animator == null) return;

        // Sprawdź czy postać strzela
        bool isShootingNow = false;
        if (abilitiesSeraphim != null)
        {
            // Możesz dodać flagę w SeraphimAbilities
            // isShootingNow = seraphimAbilities.IsShooting;
        }

        animator.SetBool("IsShooting", isShootingNow);
    }

    void UpdateFloatingAnimation()
    {
        if (animator == null) return;

        // Seraphim unosi się w powietrzu
        animator.SetBool("IsFloating", true);

        // Dodaj lekkie unoszenie (efekt latania)
        floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position += Vector3.up * floatOffset * Time.deltaTime * 0.5f;
    }

    // ========== METODY WYWOŁYWANE Z INNYCH SKRYPTÓW ==========

    public void TriggerShoot()
    {
        if (isDying || animator == null) return;
        animator.SetTrigger("Shoot");
        isShooting = true;
        StartCoroutine(ResetShootFlag());
        Debug.Log("🎬 Animacja: Strzał");
    }

    IEnumerator ResetShootFlag()
    {
        yield return new WaitForSeconds(0.3f);
        isShooting = false;
    }

    public void TriggerAbility()
    {
        if (isDying || animator == null) return;
        animator.SetTrigger("Ability");
        StartCoroutine(CastingAnimation(2f));
        Debug.Log("🎬 Animacja: Umiejętność");
    }

    IEnumerator CastingAnimation(float duration)
    {
        isCasting = true;
        if (animator != null)
            animator.SetBool("IsCasting", true);

        yield return new WaitForSeconds(duration);

        isCasting = false;
        if (animator != null)
            animator.SetBool("IsCasting", false);
    }

    public void TriggerUltimate()
    {
        if (isDying || animator == null) return;
        animator.SetTrigger("Ultimate");
        StartCoroutine(CastingAnimation(5f));
        Debug.Log("🎬 Animacja: Ultimate");
    }

    public void TriggerDamage()
    {
        if (isDying || animator == null) return;
        animator.SetTrigger("Damage");
        Debug.Log("🎬 Animacja: Obrażenia");
    }

    public void TriggerDeath()
    {
        if (isDying || animator == null) return;
        isDying = true;
        animator.SetTrigger("Death");
        animator.SetBool("IsDead", true);
        Debug.Log("🎬 Animacja: Śmierć");
    }

    public void Respawn()
    {
        isDying = false;
        if (animator != null)
        {
            animator.SetBool("IsDead", false);
            animator.SetTrigger("Respawn");
        }
        Debug.Log("🎬 Animacja: Odrodzenie");
    }

    // ========== GETTERY ==========

    public bool IsDying() => isDying;
    public bool IsCasting() => isCasting;
    public float GetCurrentSpeed() => currentSpeed;
}