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
    public Vector2 mouseOffset = new(20, -20);

    private RectTransform canvasRect;

    public void Awake()
    {
        canvasRect = canvas.GetComponent<RectTransform>();

        // Top-left pivot is ideal for tooltips
        tooltipRect.pivot = new Vector2(0, 1);
    }

    private void Update()
    {
        Vector2 mouse = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mouse,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out var localPoint
        );

        localPoint += mouseOffset;

        SetClampedPosition(localPoint);
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

    private void SetClampedPosition(Vector2 pos)
    {
        var width = tooltipRect.rect.width;
        var height = tooltipRect.rect.height;

        var pivot = tooltipRect.pivot;

        var leftLimit = -canvasRect.rect.width * 0.5f;
        var rightLimit = canvasRect.rect.width * 0.5f;
        var bottomLimit = -canvasRect.rect.height * 0.5f;
        var topLimit = canvasRect.rect.height * 0.5f;

        var minX = leftLimit + width * pivot.x;
        var maxX = rightLimit - width * (1 - pivot.x);

        var minY = bottomLimit + height * pivot.y;
        var maxY = topLimit - height * (1 - pivot.y);

        var x = Mathf.Clamp(pos.x, minX, maxX);
        var y = Mathf.Clamp(pos.y, minY, maxY);

        tooltipRect.localPosition = new Vector2(x, y);
    }
}