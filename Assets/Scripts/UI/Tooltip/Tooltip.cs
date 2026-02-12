using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    public TextMeshProUGUI header;
    public TextMeshProUGUI content;
    public LayoutElement layout;

    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private Canvas canvas;

    public int characterWrapLimit = 80;
    public Vector2 mouseOffset = new Vector2(20, -20);

    private RectTransform canvasRect;

    public void Awake()
    {
        canvasRect = canvas.GetComponent<RectTransform>();

        // Top-left pivot is ideal for tooltips
        tooltipRect.pivot = new Vector2(0, 1);
    }

    public void SetText(string text, string label = "")
    {
        header.gameObject.SetActive(!string.IsNullOrWhiteSpace(label));
        header.text = label;
        content.text = text;

        layout.enabled = content.text.Length > characterWrapLimit;

        // Force layout rebuild so size is correct this frame
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRect);
    }

    void Update()
    {
        Vector2 mouse = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mouse,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 localPoint
        );

        localPoint += mouseOffset;

        SetClampedPosition(localPoint);
    }

    private void SetClampedPosition(Vector2 pos)
    {
        float width = tooltipRect.rect.width;
        float height = tooltipRect.rect.height;

        Vector2 pivot = tooltipRect.pivot;

        float leftLimit   = -canvasRect.rect.width * 0.5f;
        float rightLimit  =  canvasRect.rect.width * 0.5f;
        float bottomLimit = -canvasRect.rect.height * 0.5f;
        float topLimit    =  canvasRect.rect.height * 0.5f;

        float minX = leftLimit + width * pivot.x;
        float maxX = rightLimit - width * (1 - pivot.x);

        float minY = bottomLimit + height * pivot.y;
        float maxY = topLimit - height * (1 - pivot.y);

        float x = Mathf.Clamp(pos.x, minX, maxX);
        float y = Mathf.Clamp(pos.y, minY, maxY);

        tooltipRect.localPosition = new Vector2(x, y);
    }
}
