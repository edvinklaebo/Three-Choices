using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace UI.Tooltip
{
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
            this.canvasRect = this.canvas.GetComponent<RectTransform>();

            // Top-left pivot is ideal for tooltips
            this.tooltipRect.pivot = new Vector2(0, 1);
        }

        private void Update()
        {
            Vector2 mouse = Input.mousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.canvasRect,
                mouse,
                this.canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : this.canvas.worldCamera,
                out var localPoint
                );

            localPoint += this.mouseOffset;

            SetClampedPosition(localPoint);
        }

        public void SetText(string text, string label = "")
        {
            this.header.gameObject.SetActive(!string.IsNullOrWhiteSpace(label));
            this.header.text = label;
            this.content.text = text;

            this.layout.enabled = this.content.text.Length > this.characterWrapLimit;

            // Force layout rebuild so size is correct this frame
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.tooltipRect);
        }

        private void SetClampedPosition(Vector2 pos)
        {
            var width = this.tooltipRect.rect.width;
            var height = this.tooltipRect.rect.height;

            var pivot = this.tooltipRect.pivot;

            var leftLimit = -this.canvasRect.rect.width * 0.5f;
            var rightLimit = this.canvasRect.rect.width * 0.5f;
            var bottomLimit = -this.canvasRect.rect.height * 0.5f;
            var topLimit = this.canvasRect.rect.height * 0.5f;

            var minX = leftLimit + width * pivot.x;
            var maxX = rightLimit - width * (1 - pivot.x);

            var minY = bottomLimit + height * pivot.y;
            var maxY = topLimit - height * (1 - pivot.y);

            var x = Mathf.Clamp(pos.x, minX, maxX);
            var y = Mathf.Clamp(pos.y, minY, maxY);

            this.tooltipRect.localPosition = new Vector2(x, y);
        }
    }
}