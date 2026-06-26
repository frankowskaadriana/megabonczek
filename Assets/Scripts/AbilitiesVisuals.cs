using UnityEngine;
using System.Collections;

public class AbilityVisuals : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA WIZUALIZACJI ═══════════════")]
    public float showDuration = 0.3f;
    public float lineWidth = 0.08f;
    public int segments = 50;

    [Header("═══════════════ KOLORY ═══════════════")]
    public Color attackColor = new Color(1f, 0f, 0f, 0.7f);
    public Color specialColor = new Color(0f, 1f, 1f, 0.6f);
    public Color ultimateColor = new Color(1f, 0f, 0f, 0.6f);
    public Color stompColor = new Color(1f, 0.6f, 0f, 0.6f);
    public Color rangeColor = new Color(0f, 1f, 0f, 0.3f);

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public Transform firePoint;

    private AbilitiesMountainMan mountainMan;
    private AbilitiesSeraphim seraphim;
    private bool isMountainMan = false;

    // Linie
    private LineRenderer attackLine;
    private LineRenderer specialLine;
    private LineRenderer ultimateLine;
    private LineRenderer stompLine;

    // Obiekty
    private GameObject attackObj;
    private GameObject specialObj;
    private GameObject ultimateObj;
    private GameObject stompObj;
    private GameObject rangeObj;

    private Camera mainCamera;
    private bool isVisible = false;

    void Start()
    {
        mainCamera = Camera.main;
        mountainMan = GetComponent<AbilitiesMountainMan>();
        seraphim = GetComponent<AbilitiesSeraphim>();

        if (firePoint == null)
            firePoint = transform;

        if (mountainMan != null)
        {
            isMountainMan = true;
            Debug.Log("🎯 AbilityVisuals dla Górala - aktywowane!");

            // Stwórz wszystkie wizualizacje
            CreateAttackCone();
            CreateCircleIndicator(ref stompLine, ref stompObj, mountainMan.stompRange, stompColor, "StompRange");
            CreateCircleIndicator(ref specialLine, ref specialObj, mountainMan.specialRange, specialColor, "SpecialRange");
            CreateCircleIndicator(ref ultimateLine, ref ultimateObj, mountainMan.ultimateRadius, ultimateColor, "UltimateRange");
            CreateRangeCircle();

            // Ukryj wszystko na starcie
            SetAllVisible(false);
        }
        else if (seraphim != null)
        {
            Debug.Log("🎯 AbilityVisuals dla Seraphima");
            // Tutaj możesz dodać wizualizacje dla Seraphima
        }
        else
        {
            Debug.LogWarning("⚠️ AbilityVisuals wymaga AbilitiesMountainMan lub AbilitiesSeraphim!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!isMountainMan || mountainMan == null) return;

        // Pokaż/ukryj zasięgi na klawisze
        if (Input.GetKeyDown(KeyCode.Alpha1)) // 1 - Atak
        {
            ToggleAttackRange();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) // 2 - Stomp
        {
            ToggleStompRange();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) // 3 - Special
        {
            ToggleSpecialRange();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) // 4 - Ultimate
        {
            ToggleUltimateRange();
        }
        if (Input.GetKeyDown(KeyCode.Alpha5)) // 5 - Wszystkie
        {
            ToggleAllRanges();
        }

        // Aktualizuj pozycję wizualizacji
        UpdateAllVisuals();
    }

    void LateUpdate()
    {
        if (!isMountainMan || mountainMan == null) return;

        // Aktualizuj stożek ataku gdy widoczny
        if (attackObj != null && attackObj.activeSelf)
        {
            UpdateAttackCone();
        }
    }

    // ===== TWORZENIE WIZUALIZACJI =====

    void CreateAttackCone()
    {
        attackObj = new GameObject("AttackCone");
        attackObj.transform.SetParent(transform);
        attackObj.transform.localPosition = Vector3.zero;
        attackObj.transform.localRotation = Quaternion.identity;
        attackLine = attackObj.AddComponent<LineRenderer>();

        attackLine.startWidth = lineWidth;
        attackLine.endWidth = lineWidth;
        attackLine.useWorldSpace = false;
        attackLine.loop = false;
        attackLine.sortingOrder = -1;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = attackColor;
        attackLine.material = mat;

        attackLine.startColor = attackColor;
        attackLine.endColor = new Color(attackColor.r, attackColor.g, attackColor.b, 0.3f);

        UpdateAttackCone();
        attackObj.SetActive(false);
    }

    void UpdateAttackCone()
    {
        if (attackLine == null || mountainMan == null) return;

        Vector3 center = Vector3.zero;
        Vector3 forward = Vector3.forward;
        float range = mountainMan.attackRange;
        float angle = mountainMan.attackAngle;
        float halfAngle = angle / 2f;

        int points = segments;
        attackLine.positionCount = points + 3;

        // Początek stożka
        attackLine.SetPosition(0, center);

        // Lewa krawędź
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        attackLine.SetPosition(1, center + leftDir * range);

        // Łuk
        int pointIndex = 2;
        for (int i = 1; i <= points; i++)
        {
            float t = (float)i / points;
            float currentAngle = -halfAngle + (angle * t);
            Vector3 dir = Quaternion.Euler(0, currentAngle, 0) * forward;
            Vector3 point = center + dir * range;
            attackLine.SetPosition(pointIndex++, point);
        }

        // Prawa krawędź
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
        line.sortingOrder = -1;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        line.material = mat;

        line.startColor = color;
        line.endColor = new Color(color.r, color.g, color.b, 0.3f);

        UpdateCircleLine(line, radius);
        obj.SetActive(false);
    }

    void UpdateCircleLine(LineRenderer line, float radius)
    {
        if (line == null) return;

        int points = segments;
        line.positionCount = points;

        for (int i = 0; i < points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            line.SetPosition(i, new Vector3(x, 0.02f, z));
        }
    }

    void CreateRangeCircle()
    {
        rangeObj = new GameObject("RangeIndicator");
        rangeObj.transform.SetParent(transform);
        rangeObj.transform.localPosition = Vector3.zero;
        rangeObj.transform.localRotation = Quaternion.identity;

        // Użyjemy wielu linii dla efektu
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            GameObject lineObj = new GameObject($"RangeLine_{i}");
            lineObj.transform.SetParent(rangeObj.transform);
            lineObj.transform.localPosition = Vector3.zero;

            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.startWidth = 0.03f;
            line.endWidth = 0.03f;
            line.useWorldSpace = false;
            line.positionCount = 2;

            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = rangeColor;
            line.material = mat;

            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Sin(rad) * 2f;
            float z = Mathf.Cos(rad) * 2f;

            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, new Vector3(x, 0f, z));
        }

        rangeObj.SetActive(false);
    }

    // ===== METODY DO POKAZYWANIA/UKRYWANIA =====

    void SetAllVisible(bool visible)
    {
        if (attackObj != null) attackObj.SetActive(visible);
        if (stompObj != null) stompObj.SetActive(visible);
        if (specialObj != null) specialObj.SetActive(visible);
        if (ultimateObj != null) ultimateObj.SetActive(visible);
        if (rangeObj != null) rangeObj.SetActive(visible);
        isVisible = visible;
    }

    void ToggleAttackRange()
    {
        if (attackObj == null) return;
        bool newState = !attackObj.activeSelf;
        attackObj.SetActive(newState);
        if (newState) UpdateAttackCone();
        Debug.Log($"🔴 Stożek ataku: {(newState ? "WIDOCZNY" : "UKRYTY")}");
    }

    void ToggleStompRange()
    {
        if (stompObj == null) return;
        bool newState = !stompObj.activeSelf;
        stompObj.SetActive(newState);
        if (newState) UpdateCircleLine(stompLine, mountainMan.stompRange);
        Debug.Log($"🟠 Zasięg Stomp: {(newState ? "WIDOCZNY" : "UKRYTY")}");
    }

    void ToggleSpecialRange()
    {
        if (specialObj == null) return;
        bool newState = !specialObj.activeSelf;
        specialObj.SetActive(newState);
        if (newState) UpdateCircleLine(specialLine, mountainMan.specialRange);
        Debug.Log($"🔵 Zasięg Special: {(newState ? "WIDOCZNY" : "UKRYTY")}");
    }

    void ToggleUltimateRange()
    {
        if (ultimateObj == null) return;
        bool newState = !ultimateObj.activeSelf;
        ultimateObj.SetActive(newState);
        if (newState) UpdateCircleLine(ultimateLine, mountainMan.ultimateRadius);
        Debug.Log($"🔴 Zasięg Ultimate: {(newState ? "WIDOCZNY" : "UKRYTY")}");
    }

    void ToggleAllRanges()
    {
        bool allVisible = !isVisible;
        SetAllVisible(allVisible);
        if (allVisible)
        {
            UpdateAttackCone();
            UpdateCircleLine(stompLine, mountainMan.stompRange);
            UpdateCircleLine(specialLine, mountainMan.specialRange);
            UpdateCircleLine(ultimateLine, mountainMan.ultimateRadius);
        }
        Debug.Log($"🎯 WSZYSTKIE zasięgi: {(allVisible ? "WIDOCZNE" : "UKRYTE")}");
    }

    void UpdateAllVisuals()
    {
        if (!isVisible) return;

        // Aktualizuj tylko te które są widoczne
        if (attackObj != null && attackObj.activeSelf) UpdateAttackCone();
        if (stompObj != null && stompObj.activeSelf) UpdateCircleLine(stompLine, mountainMan.stompRange);
        if (specialObj != null && specialObj.activeSelf) UpdateCircleLine(specialLine, mountainMan.specialRange);
        if (ultimateObj != null && ultimateObj.activeSelf) UpdateCircleLine(ultimateLine, mountainMan.ultimateRadius);
    }

    // ===== METODY PUBLICZNE =====

    public void ShowAttackRange(float duration = -1f)
    {
        if (attackObj == null) return;
        attackObj.SetActive(true);
        UpdateAttackCone();
        if (duration > 0) StartCoroutine(HideAfterDelay(attackObj, duration));
    }

    public void ShowStompRange(float duration = -1f)
    {
        if (stompObj == null) return;
        stompObj.SetActive(true);
        UpdateCircleLine(stompLine, mountainMan.stompRange);
        if (duration > 0) StartCoroutine(HideAfterDelay(stompObj, duration));
    }

    public void ShowSpecialRange(float duration = -1f)
    {
        if (specialObj == null) return;
        specialObj.SetActive(true);
        UpdateCircleLine(specialLine, mountainMan.specialRange);
        if (duration > 0) StartCoroutine(HideAfterDelay(specialObj, duration));
    }

    public void ShowUltimateRange(float duration = -1f)
    {
        if (ultimateObj == null) return;
        ultimateObj.SetActive(true);
        UpdateCircleLine(ultimateLine, mountainMan.ultimateRadius);
        if (duration > 0) StartCoroutine(HideAfterDelay(ultimateObj, duration));
    }

    public void ShowAllRanges(float duration = -1f)
    {
        SetAllVisible(true);
        UpdateAllVisuals();
        if (duration > 0) StartCoroutine(HideAllAfterDelay(duration));
    }

    public void HideAllRanges()
    {
        SetAllVisible(false);
    }

    IEnumerator HideAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) obj.SetActive(false);
    }

    IEnumerator HideAllAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetAllVisible(false);
    }

    void OnDrawGizmos()
    {
        if (mountainMan == null) return;

        // Rysuj w Scene View (tylko gdy zaznaczony)
        Gizmos.color = attackColor;
        Vector3 forward = transform.forward;
        float halfAngle = mountainMan.attackAngle / 2f;
        float range = mountainMan.attackRange;

        // Stożek ataku
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;
        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * range);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * range);

        // Łuk
        int points = 20;
        for (int i = 0; i <= points; i++)
        {
            float t = (float)i / points;
            float angle = -halfAngle + (mountainMan.attackAngle * t);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = transform.position + dir * range;

            if (i > 0)
            {
                float prevAngle = -halfAngle + (mountainMan.attackAngle * ((float)(i - 1) / points));
                Vector3 prevDir = Quaternion.Euler(0, prevAngle, 0) * forward;
                Vector3 prevPoint = transform.position + prevDir * range;
                Gizmos.DrawLine(prevPoint, point);
            }
        }

        // Koła zasięgów
        Gizmos.color = stompColor;
        DrawCircle(transform.position, mountainMan.stompRange);

        Gizmos.color = specialColor;
        DrawCircle(transform.position, mountainMan.specialRange);

        Gizmos.color = ultimateColor;
        DrawCircle(transform.position, mountainMan.ultimateRadius);
    }

    void DrawCircle(Vector3 center, float radius)
    {
        int points = 30;
        Vector3 prevPoint = center + new Vector3(0, 0, radius);

        for (int i = 1; i <= points; i++)
        {
            float angle = 2f * Mathf.PI * i / points;
            float x = Mathf.Sin(angle) * radius;
            float z = Mathf.Cos(angle) * radius;
            Vector3 point = center + new Vector3(x, 0, z);
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
}