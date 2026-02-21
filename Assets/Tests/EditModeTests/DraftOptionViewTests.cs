using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class DraftOptionViewTests
    {
        private GameObject _btnObj;
        private DraftOptionView _view;
        private Text _text;
        private TooltipTrigger _tooltip;

        [SetUp]
        public void SetUp()
        {
            _btnObj = new GameObject("TestOption");
            _btnObj.AddComponent<Button>();
            _tooltip = _btnObj.AddComponent<TooltipTrigger>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(_btnObj.transform);
            _text = textObj.AddComponent<Text>();

            _view = _btnObj.AddComponent<DraftOptionView>();
            if (!_view.DidAwake) _view.Awake();
        }

        [TearDown]
        public void TearDown()
        {
            if (_btnObj != null)
                Object.DestroyImmediate(_btnObj);
        }

        [Test]
        public void Bind_SetsButtonText()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("Power Up", "Power Up", UpgradeType.Stat, StatType.AttackPower, 5);

            _view.Bind(upgrade, null);

            Assert.AreEqual("Power Up", _text.text);

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Bind_SetsTooltipLabel()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("tough-id", "Extra Armor", UpgradeType.Stat, StatType.Armor, 3);

            _view.Bind(upgrade, null);

            Assert.AreEqual("Extra Armor", _tooltip.Label);
            Assert.AreEqual(upgrade.Description, _tooltip.Content);

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Bind_OnClick_InvokesOnPick()
        {
            var invoked = false;
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("Quick Strike", "Quick Strike", UpgradeType.Stat, StatType.AttackPower, 2);

            _view.Bind(upgrade, () => invoked = true);

            _btnObj.GetComponent<Button>().onClick.Invoke();

            Assert.IsTrue(invoked, "onPick should be invoked when button is clicked");

            Object.DestroyImmediate(upgrade);
        }
    }
}
