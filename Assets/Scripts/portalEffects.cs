using UnityEngine;
using System.Collections;
public class PortalEffect : MonoBehaviour
{
    [Header("═══════════════ EFEKTY ═══════════════")]
    public float rotationSpeed = 30f;
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.1f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
}