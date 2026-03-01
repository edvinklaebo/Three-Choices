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
            _view.Awake();
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
            var option = new DraftOption(upgrade);

            _view.Bind(option, null);

            Assert.AreEqual("Power Up", _text.text);

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Bind_SetsTooltipLabel()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("tough-id", "Extra Armor", UpgradeType.Stat, StatType.Armor, 3);
            var option = new DraftOption(upgrade);

            _view.Bind(option, null);

            Assert.AreEqual("Extra Armor", _tooltip.Label);
            Assert.AreEqual(upgrade.Description, _tooltip.Content);

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Bind_OnClick_InvokesOnPickWithOption()
        {
            DraftOption received = null;
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("Quick Strike", "Quick Strike", UpgradeType.Stat, StatType.AttackPower, 2);
            var option = new DraftOption(upgrade);

            _view.Bind(option, o => received = o);

            _btnObj.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(option, received, "onPick should be invoked with the bound option");

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Awake_CalledTwice_OnPickInvokedOnlyOnce()
        {
            // Simulate the runtime scenario where Unity auto-calls Awake via AddComponent
            // and then BuildOptions also calls Awake explicitly.
            _view.Awake(); // second call - should be a no-op due to _initialized guard

            var callCount = 0;
            var upgrade = ScriptableObject.CreateInstance<UpgradeDefinition>();
            upgrade.EditorInit("Guard Test", "Guard Test", UpgradeType.Stat, StatType.AttackPower, 1);
            var option = new DraftOption(upgrade);

            _view.Bind(option, _ => callCount++);

            _btnObj.GetComponent<Button>().onClick.Invoke();

            Assert.AreEqual(1, callCount, "onPick should be invoked exactly once even when Awake is called multiple times");

            Object.DestroyImmediate(upgrade);
        }

        [Test]
        public void Bind_ArtifactOption_SetsButtonText()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("artifact_test", "Crown of Echoes", "A powerful crown", Rarity.Rare,
                ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            var option = new DraftOption(artifact);

            _view.Bind(option, null);

            Assert.AreEqual("Crown of Echoes", _text.text);

            Object.DestroyImmediate(artifact);
        }

        [Test]
        public void Bind_ArtifactOption_SetsTooltip()
        {
            var artifact = ScriptableObject.CreateInstance<ArtifactDefinition>();
            artifact.EditorInit("artifact_test", "Hourglass", "Shields you from death", Rarity.Epic,
                ArtifactTag.None, ArtifactEffectType.AddArtifact, false);
            var option = new DraftOption(artifact);

            _view.Bind(option, null);

            Assert.AreEqual("Hourglass", _tooltip.Label);
            Assert.AreEqual("Shields you from death", _tooltip.Content);

            Object.DestroyImmediate(artifact);
        }
    }
}
