using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class TooltipTests
    {
        [TearDown]
        public void TearDown()
        {
            // Clean up static instance to prevent test interference
            TooltipSystem.instance = null;
        }

        private static Tooltip CreateTooltip()
        {
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(RectTransform));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var tooltipGO = new GameObject("Tooltip", typeof(RectTransform));
            var tooltipRect = tooltipGO.GetComponent<RectTransform>();
            tooltipRect.SetParent(canvasGO.transform);
            tooltipRect.sizeDelta = new Vector2(200, 100);

            var headerGO = new GameObject("Header", typeof(TextMeshProUGUI));
            headerGO.transform.SetParent(tooltipGO.transform);
            var header = headerGO.GetComponent<TextMeshProUGUI>();

            var contentGO = new GameObject("Content", typeof(TextMeshProUGUI));
            contentGO.transform.SetParent(tooltipGO.transform);
            var content = contentGO.GetComponent<TextMeshProUGUI>();

            var layout = tooltipGO.AddComponent<LayoutElement>();

            var tooltip = tooltipGO.AddComponent<Tooltip>();

            tooltip.header = header;
            tooltip.content = content;
            tooltip.layout = layout;

            // assign private fields via reflection
            typeof(Tooltip)
                .GetField("tooltipRect", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tooltip, tooltipRect);

            typeof(Tooltip)
                .GetField("canvas", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(tooltip, canvas);

            tooltip.Awake();

            return tooltip;
        }

        [Test]
        public void SetText_HidesHeader_WhenLabelEmpty()
        {
            var tooltip = CreateTooltip();

            tooltip.SetText("content");

            Assert.IsFalse(tooltip.header.gameObject.activeSelf);
        }

        [Test]
        public void SetText_ShowsHeader_WhenLabelProvided()
        {
            var tooltip = CreateTooltip();

            tooltip.SetText("content", "Title");

            Assert.IsTrue(tooltip.header.gameObject.activeSelf);
            Assert.AreEqual("Title", tooltip.header.text);
        }

        [Test]
        public void SetText_EnablesLayout_WhenTextTooLong()
        {
            var tooltip = CreateTooltip();
            tooltip.characterWrapLimit = 5;

            tooltip.SetText("123456789");

            Assert.IsTrue(tooltip.layout.enabled);
        }

        [Test]
        public void SetText_DisablesLayout_WhenTextShort()
        {
            var tooltip = CreateTooltip();
            tooltip.characterWrapLimit = 50;

            tooltip.SetText("short");

            Assert.IsFalse(tooltip.layout.enabled);
        }

        [Test]
        public void SetClampedPosition_ClampsInsideCanvas()
        {
            var tooltip = CreateTooltip();

            var tooltipRect = tooltip.GetComponent<RectTransform>();
            tooltipRect.sizeDelta = new Vector2(200, 100);

            var method = typeof(Tooltip)
                .GetMethod("SetClampedPosition", BindingFlags.NonPublic | BindingFlags.Instance);

            // Try to place way outside bounds
            method?.Invoke(tooltip, new object[] { new Vector2(9999, 9999) });

            var pos = tooltipRect.localPosition;

            Assert.LessOrEqual(Mathf.Abs(pos.x), 5000);
            Assert.LessOrEqual(Mathf.Abs(pos.y), 5000);
        }

        [Test]
        public void TooltipSystem_Show_ActivatesTooltip()
        {
            var tooltip = CreateTooltip();

            var sysGO = new GameObject("TooltipSystem");
            var system = sysGO.AddComponent<TooltipSystem>();
            system.tooltip = tooltip;

            system.Awake();

            TooltipSystem.Show("body", "title");

            Assert.IsTrue(tooltip.gameObject.activeSelf);
            Assert.AreEqual("body", tooltip.content.text);
        }

        [Test]
        public void TooltipSystem_Hide_DeactivatesTooltip()
        {
            var tooltip = CreateTooltip();

            var sysGO = new GameObject("TooltipSystem");
            var system = sysGO.AddComponent<TooltipSystem>();
            system.tooltip = tooltip;

            system.Awake();

            TooltipSystem.Hide();

            Assert.IsFalse(tooltip.gameObject.activeSelf);
        }

        [Test]
        public void TooltipSystem_Hide_DoesNotThrow_WhenTooltipDestroyed()
        {
            var tooltip = CreateTooltip();

            var sysGO = new GameObject("TooltipSystem");
            var system = sysGO.AddComponent<TooltipSystem>();
            system.tooltip = tooltip;

            system.Awake();

            // Destroy the tooltip to simulate scene transition
            Object.DestroyImmediate(tooltip.gameObject);

            // This should not throw an exception
            Assert.DoesNotThrow(() => TooltipSystem.Hide());
        }

        [Test]
        public void TooltipSystem_Show_DoesNotThrow_WhenTooltipDestroyed()
        {
            var tooltip = CreateTooltip();

            var sysGO = new GameObject("TooltipSystem");
            var system = sysGO.AddComponent<TooltipSystem>();
            system.tooltip = tooltip;

            system.Awake();

            // Destroy the tooltip to simulate scene transition
            Object.DestroyImmediate(tooltip.gameObject);

            // This should not throw an exception
            Assert.DoesNotThrow(() => TooltipSystem.Show("test", "label"));
        }

        [Test]
        public void TooltipSystem_Hide_DoesNotThrow_WhenInstanceNull()
        {
            // Clear the instance to simulate it being destroyed
            TooltipSystem.instance = null;

            // This should not throw an exception
            Assert.DoesNotThrow(() => TooltipSystem.Hide());
        }
    }
}