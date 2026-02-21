using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class UIFaderTests
    {
        private GameObject _go;
        private GameObject _runnerGo;
        private CanvasGroup _canvasGroup;
        private UIFader _fader;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestFaderTarget");
            _canvasGroup = _go.AddComponent<CanvasGroup>();

            // Use a separate GameObject so its Awake does not affect _canvasGroup
            _runnerGo = new GameObject("CoroutineRunner");
            var runner = _runnerGo.AddComponent<DraftUI>();

            _fader = new UIFader(_canvasGroup, runner);
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_runnerGo != null) Object.DestroyImmediate(_runnerGo);
        }

        [Test]
        public void Hide_WithoutAnimation_SetsCanvasGroupHidden()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _fader.Hide(animated: false);

            Assert.AreEqual(0f, _canvasGroup.alpha);
            Assert.IsFalse(_canvasGroup.interactable);
            Assert.IsFalse(_canvasGroup.blocksRaycasts);
        }

        [Test]
        public void Show_WithoutAnimation_SetsCanvasGroupVisible()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _fader.Show(animated: false);

            Assert.AreEqual(1f, _canvasGroup.alpha);
            Assert.IsTrue(_canvasGroup.interactable);
            Assert.IsTrue(_canvasGroup.blocksRaycasts);
        }
    }
}
