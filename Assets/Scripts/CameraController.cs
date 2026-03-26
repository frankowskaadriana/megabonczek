using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("CameraData")]
    public Transform target;
    public float distance = 5f;
    public float height = 2f;
    public float sensitivity = 2f;

    private float currentX = 0f;
    private float currentY = 0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentX = angles.y;
            currentY = angles.x;
        }

        // Od³¹cz kamerê od gracza jeœli jest dzieckiem
        if (transform.parent != null && transform.parent == target)
        {
            transform.parent = null;
        }

        // Reset pozycji kamery
        transform.position = target.position + new Vector3(0, height, -distance);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Obs³uga wejœcia myszy
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

        // OBRACANIE GRACZA W POZIOMIE (TYLKO W OSI Y)
        // Gracz obraca siê razem z kamer¹ w poziomie
        target.rotation = Quaternion.Euler(0, currentX, 0);

        // Rotacja kamery (pionowa i pozioma)
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // Oblicz pozycjê kamery (wzglêdem gracza)
        Vector3 direction = new Vector3(0, height, -distance);
        Vector3 targetPosition = target.position + rotation * direction;

        // Ustaw pozycjê kamery
        transform.position = targetPosition;

        // Kamera patrzy na gracza
        transform.LookAt(target.position + Vector3.up * (height / 2));
    }
}