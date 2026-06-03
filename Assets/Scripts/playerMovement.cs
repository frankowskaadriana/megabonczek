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

    // Publiczna właściwość dla maxSpeed
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

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }

        // ODBLOKOWANIE MYSZKI - kursor widoczny i nie zablokowany
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

        // Kierunek ruchu względem kamery
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        movementDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;

        // OBRACANIE POSTACI ZA MYSZKĄ - w stronę kursora
        RotateToMouse();

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
        // Rzutuj promień z myszki na płaszczyznę ziemi
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
        Debug.Log("GroundCheck ustawiony dla: " + gameObject.name);
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }
}