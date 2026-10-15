using System;
using System.Reflection;
using Core;
using Events;
using NUnit.Framework;
using Systems;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class PlayerOverviewUITests
    {
        private GameObject _root;
        private FightStartedEventChannel _fightStarted;

        [SetUp]
        public void SetUp()
        {
            PauseManager.Resume();
            _root = new GameObject("Overview test");
        }

        [TearDown]
        public void TearDown()
        {
            PauseManager.Resume();
            UnityEngine.Object.DestroyImmediate(_root);
            if (_fightStarted != null)
                UnityEngine.Object.DestroyImmediate(_fightStarted);
        }

        [Test]
        public void SetText_RequiresInitializedDependencies()
        {
            var overview = _root.AddComponent<PlayerOverviewUI>();
            Assert.Throws<InvalidOperationException>(() => overview.SetText("Items"));
            Assert.Throws<ArgumentNullException>(() => overview.Initialize(null, null));
        }

        [Test]
        public void SetText_TreatsItemNamesAsPlainText()
        {
            var overview = CreateOverview(out var text);

            overview.SetText("<size=100>Literal item</size>");

            Assert.IsFalse(text.richText);
            Assert.AreEqual("<size=100>Literal item</size>", text.text);
        }

        [Test]
        public void PauseOverviewBackAndResume_RefreshesAndHidesAllPanels()
        {
            var pause = CreateChild("Pause");
            var settings = CreateChild("Settings");
            var overview = CreateOverview(out var text);
            var menu = _root.AddComponent<PauseMenuUI>();
            menu.Initialize(pause, settings);
            var contents = "First acquisition";
            menu.InitializeOverview(overview, () => contents);

            PauseManager.Pause();
            menu.OnObtainedItemsClicked();
            Assert.IsFalse(pause.activeSelf);
            Assert.IsTrue(overview.gameObject.activeSelf);
            Assert.AreEqual(contents, text.text);
            Assert.IsTrue(PauseManager.IsPaused);
            menu.OnBackFromOverview();
            Assert.IsTrue(pause.activeSelf);
            Assert.IsFalse(overview.gameObject.activeSelf);
            contents = "New acquisition";
            menu.OnObtainedItemsClicked();
            Assert.AreEqual(contents, text.text);

            PauseManager.TogglePause();
            Assert.IsFalse(pause.activeSelf);
            Assert.IsFalse(settings.activeSelf);
            Assert.IsFalse(overview.gameObject.activeSelf);
        }

        [Test]
        public void GameplayBootstrap_CachesFightBeforeStartAndCreatesScrollableOverview()
        {
            var bootstrap = CreateBootstrap(true);
            var player = new Unit("Player");
            player.RecordObtainedItem(new ObtainedItem("fire", "Fireball", "A starting spell",
                ObtainedItemCategory.Abilities));
            _fightStarted.Raise(player, 1);
            InvokeStart(bootstrap);

            var menu = _root.GetComponentInChildren<PauseMenuUI>(true);
            InvokeStart(menu);
            PauseManager.Pause();
            menu.OnObtainedItemsClicked();
            var overview = _root.GetComponentInChildren<PlayerOverviewUI>(true);
            var scroll = overview.GetComponentInChildren<ScrollRect>();
            StringAssert.Contains("Fireball x1", scroll.content.GetComponent<TMP_Text>().text);
            Assert.IsFalse(scroll.horizontal);
            Assert.IsTrue(scroll.vertical);
            Assert.IsNotNull(scroll.viewport.GetComponent<RectMask2D>());

            menu.OnBackFromOverview();
            player.RecordObtainedItem(new ObtainedItem("fire", "Fireball", "A starting spell",
                ObtainedItemCategory.Abilities));
            menu.OnObtainedItemsClicked();
            StringAssert.Contains("Fireball x2", scroll.content.GetComponent<TMP_Text>().text);
        }

        [Test]
        public void MainMenuBootstrap_PreservesPauseWithoutObtainedItems()
        {
            var bootstrap = CreateBootstrap(false);
            InvokeStart(bootstrap);
            var menu = _root.GetComponentInChildren<PauseMenuUI>(true);
            InvokeStart(menu);

            PauseManager.Pause();
            menu.OnObtainedItemsClicked();

            Assert.IsTrue(PauseManager.IsPaused);
            Assert.IsNull(_root.GetComponentInChildren<PlayerOverviewUI>(true));
            Assert.IsNull(_root.transform.Find("PauseMenuUI/PauseMenuPanel/MenuContent/Button_Obtained Items"));
        }

        private PauseMenuBootstrap CreateBootstrap(bool gameplay)
        {
            _root.SetActive(false);
            var canvas = _root.AddComponent<Canvas>();
            var bootstrap = _root.AddComponent<PauseMenuBootstrap>();
            SetField(bootstrap, "_canvas", canvas);
            if (gameplay)
            {
                _fightStarted = ScriptableObject.CreateInstance<FightStartedEventChannel>();
                SetField(bootstrap, "_fightStarted", _fightStarted);
            }
            _root.SetActive(true);
            return bootstrap;
        }

        private PlayerOverviewUI CreateOverview(out TMP_Text text)
        {
            var panel = CreateChild("Overview");
            var scroll = panel.AddComponent<ScrollRect>();
            var content = CreateChild("Content");
            content.transform.SetParent(panel.transform, false);
            text = content.AddComponent<TextMeshProUGUI>();
            scroll.content = (RectTransform)content.transform;
            var overview = panel.AddComponent<PlayerOverviewUI>();
            overview.Initialize(text, scroll);
            return overview;
        }

        private GameObject CreateChild(string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(_root.transform, false);
            return child;
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static void InvokeStart(MonoBehaviour target)
        {
            target.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
        }
    }
}
