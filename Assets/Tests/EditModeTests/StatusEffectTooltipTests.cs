using System.Reflection;

using Core;
using Core.StatusEffects;

using NUnit.Framework;

using TMPro;

using UI.Combat;
using UI.Tooltip;

using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class StatusEffectTooltipTests
    {
        private GameObject _root;
        private Tooltip _tooltip;
        private StatusEffectDefinition _definition;
        private StatusEffectLibrary _library;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Tooltip test", typeof(RectTransform), typeof(Canvas));
            var tooltipObject = new GameObject("Tooltip", typeof(RectTransform));
            tooltipObject.SetActive(false);
            tooltipObject.transform.SetParent(_root.transform);
            _tooltip = tooltipObject.AddComponent<Tooltip>();
            _tooltip.header = CreateText("Header", tooltipObject.transform);
            _tooltip.content = CreateText("Content", tooltipObject.transform);
            _tooltip.layout = tooltipObject.AddComponent<LayoutElement>();
            SetField(_tooltip, "tooltipRect", tooltipObject.GetComponent<RectTransform>());
            SetField(_tooltip, "canvas", _root.GetComponent<Canvas>());
            _tooltip.Awake();

            var system = _root.AddComponent<TooltipSystem>();
            system.tooltip = _tooltip;
            system.Awake();

            _definition = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            _definition.EditorInit("Poison", "Poison", "Blocks healing.", Color.magenta);
            _library = ScriptableObject.CreateInstance<StatusEffectLibrary>();
            _library.EditorAddDefinition(_definition);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_library);
            TooltipSystem.instance = null;
        }

        [Test]
        public void Hover_ShowsDefinitionAndCurrentValues()
        {
            var icon = CreateIcon();
            icon.SetEffect("Poison", 3, 2, _definition);

            icon.OnPointerEnter(null);

            Assert.IsTrue(_tooltip.gameObject.activeSelf);
            Assert.AreEqual("Poison", _tooltip.header.text);
            Assert.AreEqual("Blocks healing.\n\nStacks: 3\nTurns remaining: 2", _tooltip.content.text);
        }

        [Test]
        public void Hover_WithoutDuration_DoesNotShowTurnsRemaining()
        {
            var icon = CreateIcon();
            icon.SetEffect("Poison", 1, 0, _definition);

            icon.OnPointerEnter(null);

            StringAssert.Contains("Stacks: 1", _tooltip.content.text);
            StringAssert.DoesNotContain("Turns remaining", _tooltip.content.text);
        }

        [Test]
        public void Hover_WithoutDefinition_DoesNotShowTooltip()
        {
            var icon = CreateIcon();
            icon.SetEffect("Unknown", 1, 2);

            icon.OnPointerEnter(null);

            Assert.IsFalse(_tooltip.gameObject.activeSelf);
        }

        [Test]
        public void PointerExit_HidesTooltip()
        {
            var icon = CreateHoveredIcon();

            icon.OnPointerExit(null);

            Assert.IsFalse(_tooltip.gameObject.activeSelf);
        }

        [Test]
        public void Disable_HidesTooltipWithoutPointerExit()
        {
            var icon = CreateHoveredIcon();

            icon.gameObject.SetActive(false);

            Assert.IsFalse(_tooltip.gameObject.activeSelf);
        }

        [Test]
        public void DisableOtherIcon_DoesNotHideCurrentTooltip()
        {
            var first = CreateHoveredIcon();
            var second = CreateHoveredIcon();

            first.gameObject.SetActive(false);

            Assert.IsTrue(_tooltip.gameObject.activeSelf);
            second.OnPointerExit(null);
            Assert.IsFalse(_tooltip.gameObject.activeSelf);
        }

        [Test]
        public void Overflow_HidesTooltipAndClearsPreviousSprite()
        {
            var icon = CreateHoveredIcon();
            var image = icon.GetComponent<Image>();
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), Vector2.zero);
            image.sprite = sprite;
            try
            {
                icon.SetOverflow(2);
                icon.OnPointerEnter(null);

                Assert.IsFalse(_tooltip.gameObject.activeSelf);
                Assert.IsNull(image.sprite);
            }
            finally
            {
                Object.DestroyImmediate(sprite);
            }
        }

        [Test]
        public void Refresh_WithSameEffectCount_UpdatesHoveredTooltipInPlace()
        {
            var unit = new Unit("Tooltip target");
            var poison = new Poison(3, 2, 1);
            unit.StatusEffects.Add(poison);
            var panel = CreatePanel(unit);
            var icon = panel.GetComponentInChildren<StatusEffectIcon>();
            icon.OnPointerEnter(null);
            poison.Stacks = 5;
            poison.Duration = 1;

            panel.SendMessage("Update");

            Assert.AreSame(icon, panel.GetComponentInChildren<StatusEffectIcon>());
            Assert.IsTrue(_tooltip.gameObject.activeSelf);
            StringAssert.Contains("Stacks: 5\nTurns remaining: 1", _tooltip.content.text);
        }

        [Test]
        public void Refresh_WithSameCountReplacement_UpdatesDefinition()
        {
            var unit = new Unit("Tooltip target");
            unit.StatusEffects.Add(new Poison(3, 2, 1));
            var panel = CreatePanel(unit);
            var icon = panel.GetComponentInChildren<StatusEffectIcon>();
            icon.OnPointerEnter(null);
            unit.StatusEffects[0] = new Bleed(2, 3, 1);
            var bleed = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            bleed.EditorInit("Bleed", "Bleed", "Physical damage.", Color.red);
            _library.EditorAddDefinition(bleed);
            try
            {
                panel.SendMessage("Update");

                Assert.AreEqual("Bleed", _tooltip.header.text);
                StringAssert.Contains("Physical damage.", _tooltip.content.text);
            }
            finally
            {
                Object.DestroyImmediate(bleed);
            }
        }

        [Test]
        public void Refresh_WhenHoveredEffectExpires_HidesTooltip()
        {
            var unit = new Unit("Tooltip target");
            unit.StatusEffects.Add(new Poison(3, 1, 1));
            var panel = CreatePanel(unit);
            panel.GetComponentInChildren<StatusEffectIcon>().OnPointerEnter(null);
            unit.StatusEffects.Clear();

            panel.SendMessage("Update");

            Assert.IsFalse(_tooltip.gameObject.activeSelf);
        }

        [Test]
        public void GameAssets_AllRegisteredStatusEffectsHaveTooltipMetadata()
        {
            var library = Resources.Load<StatusEffectLibrary>("status_effect_pool");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/StatusEffectIcon.prefab");
            Assert.IsNotNull(prefab.GetComponent<StatusEffectIcon>());
            var ids = new[] { "Poison", "Bleed", "Burn", "Stunned", "Regeneration", "Vulnerable", "Weak" };
            foreach (var id in ids)
            {
                var definition = library.GetDefinition(id);
                Assert.IsNotNull(definition, id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(definition.DisplayName), id);
                Assert.IsFalse(string.IsNullOrWhiteSpace(definition.Description), id);
            }
        }

        private StatusEffectPanel CreatePanel(Unit unit)
        {
            var panelObject = new GameObject("Status panel");
            panelObject.SetActive(false);
            panelObject.transform.SetParent(_root.transform);
            var panel = panelObject.AddComponent<StatusEffectPanel>();
            SetField(panel, "_iconPrefab", CreateIcon());
            SetField(panel, "_library", _library);
            SetField(panel, "_iconContainer", panelObject.transform);
            panelObject.SetActive(true);
            panel.Initialize(unit);
            return panel;
        }

        private StatusEffectIcon CreateHoveredIcon()
        {
            var icon = CreateIcon();
            icon.SetEffect("Poison", 3, 2, _definition);
            icon.OnPointerEnter(null);
            return icon;
        }

        private StatusEffectIcon CreateIcon()
        {
            var iconObject = new GameObject("Status icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(_root.transform);
            var icon = iconObject.AddComponent<StatusEffectIcon>();
            SetField(icon, "_iconImage", iconObject.GetComponent<Image>());
            return icon;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent);
            return textObject.AddComponent<TextMeshProUGUI>();
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
        }
    }
}
