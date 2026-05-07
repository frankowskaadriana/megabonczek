using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public float distance = 5f;
    public float height = 2f;
    public float sensitivity = 2f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    private float currentX = 0f;
    private float currentY = 0f;
    private Transform target;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            else return;
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

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}