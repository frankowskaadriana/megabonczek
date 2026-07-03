using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("═══════════════ RUCH ═══════════════")]
    public float maxSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;

    [Header("═══════════════ KAMERA ═══════════════")]
    public Camera mainCamera;

    [Header("═══════════════ OBRACANIE ZA MYSZKĄ ═══════════════")]
    public float rotationSpeed = 15f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 mouseWorldPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // Zablokuj ruch w pionie
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        // ============================================================
        // RUCH - WSAD
        // ============================================================
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = (forward * vertical + right * horizontal).normalized;

        // ============================================================
        // OBRACANIE ZA MYSZKĄ (TOP-DOWN)
        // ============================================================
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            mouseWorldPosition = ray.GetPoint(distance);
            mouseWorldPosition.y = 0f;

            Vector3 direction = mouseWorldPosition - transform.position;
            direction.y = 0f;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
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

        // Tylko X i Z, Y zablokowane
        rb.linearVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
    }

    // ============================================================
    // METODY PUBLICZNE
    // ============================================================

    public Vector3 GetVelocity()
    {
        return rb.linearVelocity;
    }

    public float GetSpeed()
    {
        return rb.linearVelocity.magnitude;
    }

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }

    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
}