using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("═══════════════ RUCH ═══════════════")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;

    [Header("═══════════════ KAMERA ═══════════════")]
    public Camera mainCamera;

    [Header("═══════════════ GROUND CHECK ═══════════════")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer = ~0;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    void FixedUpdate()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = moveDirection * maxSpeed;
            currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

        if (moveDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), Time.fixedDeltaTime * 10f);
        }
    }

    // ============================================================
    // METODY PUBLICZNE DLA ANIMATORA
    // ============================================================

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetSpeed()
    {
        return rb.linearVelocity.magnitude;
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }
}