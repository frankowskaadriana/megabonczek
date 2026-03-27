using UnityEngine;

public class playerMovement : MonoBehaviour
{
    [Header("Keybinds")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Movement")]
    public float moveSpeed = 5f;
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

        // Zapisz początkową pozycję
        startPosition = transform.position;

        // Zamroź rotację (ale tylko X i Z, Y pozostaje odblokowane!)
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;

        // Ustaw grawitację
        Physics.gravity = new Vector3(0, gravity, 0);

        // Reset rotacji gracza
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        // Sprawdź czy gracz stoi na ziemi
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // Ruch (używa transform.forward który zmienia się z rotacją gracza)
        HandleMovement();

        // Skok
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        // Reset pozycji jeśli spadł za mapę
        if (transform.position.y < -10f)
        {
            ResetPosition();
        }
        // Kucanie
        crouch();
    }

    void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(forwardKey)) vertical = 1f;
        if (Input.GetKey(backKey)) vertical = -1f;
        if (Input.GetKey(rightKey)) horizontal = 1f;
        if (Input.GetKey(leftKey)) horizontal = -1f;

        // Ruch względem rotacji gracza (gracz już jest obrócony przez kamerę)
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        move = move.normalized * moveSpeed;
        move.y = rb.linearVelocity.y;

        rb.linearVelocity = move;
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void ResetPosition()
    {
        transform.position = startPosition;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log("Pozycja gracza zresetowana");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
    void crouch()
    {
        if (Input.GetKeyDown(crouchKey))
        {
            // Zmniejsz wysokość gracza (przykładowo)
            transform.localScale = new Vector3(1f, 0.5f, 1f);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            // Przywróć normalną wysokość
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}