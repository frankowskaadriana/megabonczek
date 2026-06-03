using UnityEngine;
using UnityEngine.UI;

public class CursorDot : MonoBehaviour
{
    public float dotSize = 15f;
    public Color dotColor = Color.white;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(dotSize, dotSize);

        Image image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        image.color = dotColor;

        // Stwórz okrągłą teksturę
        Texture2D texture = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dx = x - 16;
                float dy = y - 16;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, dist < 15 ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));

        Cursor.visible = false;
    }

    void Update()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Input.mousePosition;
        }
    }
}