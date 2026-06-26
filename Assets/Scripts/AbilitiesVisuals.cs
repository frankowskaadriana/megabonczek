using UnityEngine;
using System.Collections;

public class AbilityVisuals : MonoBehaviour
{
    [Header("Ustawienia")]
    public float showDuration = 0.2f;
    public Color attackColor = new Color(1f, 1f, 0f, 0.9f);
    public Color specialColor = new Color(0f, 1f, 1f, 0.8f);
    public Color ultimateColor = new Color(1f, 0f, 0f, 0.8f);
    public float lineWidth = 0.08f;
    public float trajectoryLength = 12f;
    public int trajectoryPoints = 20;
    public Transform firePoint;

    private AbilitiesMountainMan mountainMan;
    private AbilitiesSeraphim seraphim;
    private LineRenderer attackLine;
    private LineRenderer specialLine;
    private LineRenderer ultimateLine;
    private GameObject attackObj;
    private GameObject specialObj;
    private GameObject ultimateObj;
    private bool isMountainMan = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        mountainMan = GetComponent<AbilitiesMountainMan>();
        seraphim = GetComponent<AbilitiesSeraphim>();

        if (firePoint == null && seraphim != null)
        {
            firePoint = seraphim.firePoint;
            if (firePoint == null) firePoint = transform;
        }

        if (mountainMan != null)
        {
            isMountainMan = true;
            attackColor = new Color(1f, 0f, 0f, 0.8f);
            CreateAttackCone();
            CreateCircleIndicator(ref specialLine, ref specialObj, GetSpecialRange(), specialColor, "SpecialIndicator");
            CreateCircleIndicator(ref ultimateLine, ref ultimateObj, GetUltimateRange(), ultimateColor, "UltimateIndicator");

            attackObj.SetActive(false);
            specialObj.SetActive(false);
            ultimateObj.SetActive(false);
        }
        else if (seraphim != null)
        {
            CreateTrajectoryLine();
            CreateCircleIndicator(ref specialLine, ref specialObj, GetSpecialRange(), specialColor, "SpecialIndicator");
            CreateCircleIndicator(ref ultimateLine, ref ultimateObj, GetUltimateRange(), ultimateColor, "UltimateIndicator");

            attackObj.SetActive(false);
            specialObj.SetActive(false);
            ultimateObj.SetActive(false);
        }
        else
        {
            Debug.LogError("AbilityVisuals wymaga AbilitiesMountainMan lub AbilitiesSeraphim!");
            enabled = false;
        }
    }

    void CreateTrajectoryLine()
    {
        attackObj = new GameObject("TrajectoryLine");
        attackObj.transform.SetParent(transform);
        attackObj.transform.localPosition = Vector3.zero;
        attackLine = attackObj.AddComponent<LineRenderer>();

        attackLine.startWidth = lineWidth;
        attackLine.endWidth = lineWidth * 0.5f;
        attackLine.useWorldSpace = true;
        attackLine.positionCount = trajectoryPoints;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = attackColor;
        attackLine.material = mat;

        attackLine.startColor = attackColor;
        attackLine.endColor = new Color(attackColor.r, attackColor.g, attackColor.b, 0f);
    }

    void UpdateTrajectoryLine()
    {
        if (attackLine == null || seraphim == null) return;

        Vector3 startPoint = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = GetMouseDirection();

        attackLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float t = (float)i / (trajectoryPoints - 1);
            Vector3 point = startPoint + direction * (t * trajectoryLength);
            attackLine.SetPosition(i, point);
        }
    }

    Vector3 GetMouseDirection()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 startPoint = firePoint != null ? firePoint.position : transform.position;
            return (hitPoint - startPoint).normalized;
        }

        return transform.forward;
    }

    float GetSpecialRange()
    {
        if (mountainMan != null) return mountainMan.specialRange;
        if (seraphim != null) return seraphim.chargeRange;
        return 3f;
    }

    float GetUltimateRange()
    {
        if (mountainMan != null) return mountainMan.ultimateRadius;
        if (seraphim != null) return seraphim.judgmentRadius;
        return 5f;
    }

    void CreateAttackCone()
    {
        attackObj = new GameObject("AttackIndicator");
        attackObj.transform.SetParent(transform);
        attackObj.transform.localPosition = Vector3.zero;
        attackObj.transform.localRotation = Quaternion.identity;
        attackLine = attackObj.AddComponent<LineRenderer>();

        attackLine.startWidth = lineWidth;
        attackLine.endWidth = lineWidth;
        attackLine.useWorldSpace = false;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = attackColor;
        attackLine.material = mat;

        attackLine.startColor = attackColor;
        attackLine.endColor = attackColor;
        attackLine.sortingOrder = -1;

        UpdateAttackCone();
    }

    void UpdateAttackCone()
    {
        if (attackLine == null || mountainMan == null) return;

        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;
        float range = mountainMan.attackRange;
        float angle = mountainMan.attackAngle;
        float halfAngle = angle / 2f;

        int segments = 30;
        attackLine.positionCount = segments + 3;

        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        attackLine.SetPosition(0, center);
        attackLine.SetPosition(1, center + leftDir * range);

        int pointIndex = 2;
        for (int i = 1; i <= segments; i++)
        {
            float t = (float)i / segments;
            float currentAngle = -halfAngle + (angle * t);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 point = center + dir * range;
            attackLine.SetPosition(pointIndex++, point);
        }

        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        attackLine.SetPosition(attackLine.positionCount - 1, center + rightDir * range);
    }

    void CreateCircleIndicator(ref LineRenderer line, ref GameObject obj, float radius, Color color, string name)
    {
        obj = new GameObject(name);
        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        line = obj.AddComponent<LineRenderer>();

        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.loop = true;
        line.useWorldSpace = false;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;

        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = -1;

        UpdateCircleLine(line, radius);
    }

    void UpdateCircleLine(LineRenderer line, float radius)
    {
        if (line == null) return;

        int segments = 50;
        line.positionCount = segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            line.SetPosition(i, new Vector3(x, 0.02f, z));
        }
    }

    void LateUpdate()
    {
        if (mountainMan != null)
        {
            if (attackObj != null && attackObj.activeSelf) UpdateAttackCone();
            if (specialObj != null && specialObj.activeSelf) UpdateCircleLine(specialLine, GetSpecialRange());
            if (ultimateObj != null && ultimateObj.activeSelf) UpdateCircleLine(ultimateLine, GetUltimateRange());
        }
        else if (seraphim != null)
        {
            if (attackObj != null && attackObj.activeSelf) UpdateTrajectoryLine();
            if (specialObj != null && specialObj.activeSelf) UpdateCircleLine(specialLine, GetSpecialRange());
            if (ultimateObj != null && ultimateObj.activeSelf) UpdateCircleLine(ultimateLine, GetUltimateRange());
        }
    }

    public void ShowAttackRange()
    {
        if (attackObj != null)
        {
            if (mountainMan != null) UpdateAttackCone();
            else if (seraphim != null) UpdateTrajectoryLine();

            attackObj.SetActive(true);
            StartCoroutine(HideAfterDelay(attackObj));
        }
    }

    public void ShowSpecialRange()
    {
        if (specialObj != null)
        {
            UpdateCircleLine(specialLine, GetSpecialRange());
            specialObj.SetActive(true);
            StartCoroutine(HideAfterDelay(specialObj));
        }
    }

    public void ShowUltimateRange()
    {
        if (ultimateObj != null)
        {
            UpdateCircleLine(ultimateLine, GetUltimateRange());
            ultimateObj.SetActive(true);
            StartCoroutine(HideAfterDelay(ultimateObj));
        }
    }

    IEnumerator HideAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(showDuration);
        if (obj != null) obj.SetActive(false);
    }
}