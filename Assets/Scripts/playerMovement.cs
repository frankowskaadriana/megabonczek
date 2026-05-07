using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement")]
    public float moveForce = 10f;
    public float maxSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        startPosition = transform.position;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        Physics.gravity = new Vector3(0, gravity, 0);

        // Automatyczne tworzenie ground check
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }

    void Update()
    {
        // Sprawdzanie czy na ziemi
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            Debug.Log("IsGrounded: " + isGrounded); // Sprawdü w konsoli
        }

        // Ruch
        HandleMovement();

        // Skok
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Debug.Log("Skok!"); // Sprawdü czy w ogÛle wchodzi
            Jump();
        }

        // Reset pozycji
        if (transform.position.y < -10f)
        {
            ResetPosition();
        }

        // Kucanie
        HandleCrouch();
    }

    void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(forwardKey)) vertical = 1f;
        if (Input.GetKey(backKey)) vertical = -1f;
        if (Input.GetKey(rightKey)) horizontal = 1f;
        if (Input.GetKey(leftKey)) horizontal = -1f;

        Vector3 moveDirection = (transform.right * horizontal + transform.forward * vertical).normalized;

        if (moveDirection != Vector3.zero)
        {
            rb.AddForce(moveDirection * moveForce, ForceMode.Force);
        }

        // Ograniczenie predkosci
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        if (horizontalVelocity.magnitude > maxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);
        }

        // Tarcie
        if (moveDirection == Vector3.zero && isGrounded)
        {
            rb.linearVelocity = new Vector3(velocity.x * 0.95f, velocity.y, velocity.z * 0.95f);
        }
    }

    void Jump()
    {
        // Reset predkosci pionowej
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        Debug.Log("Skok wykonany! Sila: " + jumpForce);
    }

    void ResetPosition()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(1f, 0.5f, 1f);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
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