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
    private Transform target; // teraz jest private

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        // Szukaj gracza z tagiem "Player"
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Znaleziono gracza: " + target.name);
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

    // Publiczna metoda do recznego ustawienia targetu (opcjonalna)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}