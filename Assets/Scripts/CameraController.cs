using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA KAMERY IZOMETRYCZNEJ ═══════════════")]
    public Transform target;
    public float cameraAngle = 45f;
    public float cameraDistance = 12f;
    public float cameraHeight = 8f;
    public float smoothSpeed = 5f;

    private Vector3 desiredPosition;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera target ustawiony na: " + target.name);
            }
        }

        if (target != null)
        {
            UpdateCameraPosition();
            transform.position = desiredPosition;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
            else return;
        }

        UpdateCameraPosition();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;

        Quaternion rotation = Quaternion.Euler(cameraAngle, 0, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -cameraDistance);
        desiredPosition = target.position + offset;
        desiredPosition.y += cameraHeight;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}