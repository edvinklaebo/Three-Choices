using System.Collections;
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
        public void AnimateScale_PositiveDuration_SetsFromScaleOnFirstFrame()
        {
            var runner = _runnerGo.AddComponent<DraftUI>();
            var from = new Vector3(1.1f, 1.1f, 1.1f);
            var to = Vector3.one;
            _targetGo.transform.localScale = Vector3.zero;

            UIAnimator.AnimateScale(_targetGo.transform, from, to, 1f, runner);

            Assert.AreEqual(from.x, _targetGo.transform.localScale.x, 0.1f);
            Assert.AreEqual(from.y, _targetGo.transform.localScale.y, 0.1f);
            Assert.AreEqual(from.z, _targetGo.transform.localScale.z, 0.1f);
        }
        
        
        [Test(ExpectedResult = null)]
        public IEnumerator AnimateScale_VerySmallDuration_Completes()
        {
            var runner = _runnerGo.AddComponent<DraftUI>();

            UIAnimator.AnimateScale(_targetGo.transform, Vector3.one, Vector3.one * 3f, 0.0001f, runner);

            yield return null;

            Assert.AreEqual(Vector3.one * 3f, _targetGo.transform.localScale);
        }
    }
}
