using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("═══════════════ RUCH ═══════════════")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("═══════════════ KLAWISZE ═══════════════")]
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    [Header("═══════════════ GROUND CHECK ═══════════════")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector3 movementDirection;
    private Camera mainCamera;
    private bool isGrounded;
    private Vector3 startPosition;
    private AudioManager audioManager;
    private float footstepTimer = 0f;

    public float maxSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezeRotationY;

        mainCamera = Camera.main;
        startPosition = transform.position;
        audioManager = AudioManager.Instance;

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(upKey)) vertical = 1f;
        if (Input.GetKey(downKey)) vertical = -1f;
        if (Input.GetKey(rightKey)) horizontal = 1f;
        if (Input.GetKey(leftKey)) horizontal = -1f;

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        movementDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        RotateToMouse();

        // Dźwięki kroków
        if (movementDirection.magnitude > 0.1f && isGrounded)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer > 0.3f)
            {
                footstepTimer = 0f;
                if (audioManager != null) audioManager.PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        if (transform.position.y < -10f)
        {
            ResetPosition();
        }
    }

    void FixedUpdate()
    {
        if (movementDirection.magnitude > 0.1f)
        {
            Vector3 targetVelocity = movementDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void RotateToMouse()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            direction.y = 0f;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }
        }
    }

    void ResetPosition()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Pozycja gracza zresetowana");
    }

    public void SetGroundCheck(Transform newGroundCheck)
    {
        groundCheck = newGroundCheck;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    // ========== METODY DLA ANIMACJI ==========

    public Vector3 GetVelocity()
    {
        if (rb != null)
            return rb.linearVelocity;
        return Vector3.zero;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}