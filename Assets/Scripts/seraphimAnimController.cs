using UnityEngine;

public class seraphimAnimController : MonoBehaviour
{
    [Header("═══════════════ REFERENCJE ═══════════════")]
    public Animator animator;
    public PlayerMovement playerMovement;
    public SpriteRenderer spriteRenderer;

    [Header("═══════════════ USTAWIENIA ═══════════════")]
    public float speedThreshold = 0.1f;

    private void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (playerMovement == null || animator == null) return;

        // ============================================================
        // PRĘDKOŚĆ
        // ============================================================
        float speed = playerMovement.GetSpeed();
        bool isMoving = speed > speedThreshold;

        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("Speed", speed);

        // ============================================================
        // OBRÓT (FLIP) - dla 2D
        // ============================================================
        if (spriteRenderer != null)
        {
            Vector3 moveDirection = playerMovement.GetMoveDirection();
            if (moveDirection.magnitude > 0.1f)
            {
                if (moveDirection.x > 0)
                    spriteRenderer.flipX = false;
                else if (moveDirection.x < 0)
                    spriteRenderer.flipX = true;
            }
        }

        // ============================================================
        // PARAMETRY DODATKOWE (opcjonalnie)
        // ============================================================
        // Jeśli masz parametry w animatorze:
        // animator.SetFloat("Vertical", playerMovement.GetMoveDirection().z);
        // animator.SetFloat("Horizontal", playerMovement.GetMoveDirection().x);
    }
}