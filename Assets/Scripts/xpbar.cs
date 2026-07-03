using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBar : MonoBehaviour
{
    [Header("??????????????? REFERENCJE ???????????????")]
    public Image xpFillImage;
    public TextMeshProUGUI xpText;

    [Header("??????????????? KOLORY ???????????????")]
    public Color normalColor = new Color(0.2f, 0.6f, 1f);
    public Color almostFullColor = new Color(1f, 0.8f, 0f);
    public Color fullColor = new Color(1f, 0.5f, 0f);

    [Header("??????????????? USTAWIENIA ???????????????")]
    public float smoothSpeed = 5f;

    private LevelSystem levelSystem;
    private float currentFill = 0f;
    private float targetFill = 0f;

    void Start()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();

        if (levelSystem == null)
        {
            Debug.LogWarning("?? LevelSystem nie znaleziony na scenie!");
            return;
        }

        if (xpFillImage != null)
        {
            currentFill = 0f;
            targetFill = 0f;
            xpFillImage.fillAmount = 0f;
        }

        UpdateXPBar();
    }

    void Update()
    {
        if (levelSystem == null) return;

        if (xpFillImage != null)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
            xpFillImage.fillAmount = currentFill;

            if (targetFill > 0.9f)
                xpFillImage.color = fullColor;
            else if (targetFill > 0.6f)
                xpFillImage.color = almostFullColor;
            else
                xpFillImage.color = normalColor;
        }

        if (xpText != null)
        {
            xpText.text = $"{levelSystem.currentXP} / {levelSystem.xpRequired} XP";
        }
    }

    public void UpdateXPBar()
    {
        if (levelSystem == null) return;

        float xpPercent = (float)levelSystem.currentXP / levelSystem.xpRequired;
        targetFill = Mathf.Clamp01(xpPercent);

        if (xpFillImage != null)
        {
            currentFill = targetFill;
            xpFillImage.fillAmount = currentFill;
        }

        if (xpText != null)
        {
            xpText.text = $"{levelSystem.currentXP} / {levelSystem.xpRequired} XP";
        }
    }

    public void Refresh()
    {
        UpdateXPBar();
    }
}