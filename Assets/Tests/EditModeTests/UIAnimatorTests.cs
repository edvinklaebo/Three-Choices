using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class UIAnimatorTests
    {
        private GameObject _targetGo;
        private GameObject _runnerGo;

        [SetUp]
        public void SetUp()
        {
            _targetGo = new GameObject("AnimatorTarget");
            _runnerGo = new GameObject("CoroutineRunner");
        }

        [TearDown]
        public void TearDown()
        {
            if (_targetGo != null) Object.DestroyImmediate(_targetGo);
            if (_runnerGo != null) Object.DestroyImmediate(_runnerGo);
        }

        [Test]
        public void AnimateScale_ZeroDuration_SetsScaleToTargetImmediately()
        {
            var runner = _runnerGo.AddComponent<DraftUI>();
            var from = new Vector3(1.1f, 1.1f, 1.1f);
            var to = Vector3.one;
            _targetGo.transform.localScale = from;

            UIAnimator.AnimateScale(_targetGo.transform, from, to, 0f, runner);

            Assert.AreEqual(to, _targetGo.transform.localScale);
        }

        [Test]
        public void AnimateScale_ZeroDuration_ReturnsCoroutine()
        {
            var runner = _runnerGo.AddComponent<DraftUI>();

            var coroutine = UIAnimator.AnimateScale(_targetGo.transform, Vector3.one, Vector3.one * 2f, 0f, runner);

            Assert.IsNotNull(coroutine);
        }

        [Test]
        public void AnimateScale_PositiveDuration_SetsFromScaleOnFirstFrame()
        {
            var runner = _runnerGo.AddComponent<DraftUI>();
            var from = new Vector3(1.1f, 1.1f, 1.1f);
            var to = Vector3.one;
            _targetGo.transform.localScale = Vector3.zero;

            UIAnimator.AnimateScale(_targetGo.transform, from, to, 1f, runner);

            Assert.AreEqual(from, _targetGo.transform.localScale);
        }
    }
}
