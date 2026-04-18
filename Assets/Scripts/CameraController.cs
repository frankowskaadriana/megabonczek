using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float sensitivity = 2f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float currentX = 0f;
    private float currentY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentX = angles.y;
            currentY = angles.x;
        }

        if (transform.parent != null && transform.parent == target)
        {
            transform.parent = null;
        }
    }

    void LateUpdate()
    {
        // Szukaj tylko jeœli target jest null
        if (target == null)
        {
            // Szukaj po tagu LUB po komponencie PlayerMovement
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                player = FindFirstObjectByType<PlayerMovement>()?.gameObject;
            }
            if (player == null)
            {
                player = FindFirstObjectByType<AngelAbilities>()?.gameObject;
            }
            if (player == null)
            {
                player = FindFirstObjectByType<AbilitiesMountainMan>()?.gameObject;
            }

            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera target ustawiony na: " + target.name);
            }
            else
            {
                return;
            }
        }

        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

        target.rotation = Quaternion.Euler(0, currentX, 0);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 direction = new Vector3(0, height, -distance);
        Vector3 targetPosition = target.position + rotation * direction;

        transform.position = targetPosition;
        transform.LookAt(target.position + Vector3.up * (height / 2));
    }

    // Publiczna metoda do rêcznego ustawienia targetu (z LevelSystem)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        Debug.Log("Camera target recznie ustawiony na: " + (newTarget != null ? newTarget.name : "null"));
    }
}