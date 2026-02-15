using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class PauseManagerTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset pause state before each test
            if (PauseManager.IsPaused)
                PauseManager.Resume();
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            // Ensure we're not paused after tests
            if (PauseManager.IsPaused)
                PauseManager.Resume();
            Time.timeScale = 1f;
        }

        [Test]
        public void IsPaused_InitiallyFalse()
        {
            Assert.IsFalse(PauseManager.IsPaused);
        }

        [Test]
        public void Pause_SetsIsPausedTrue()
        {
            PauseManager.Pause();
            Assert.IsTrue(PauseManager.IsPaused);
        }

        [Test]
        public void Pause_SetsTimeScaleToZero()
        {
            PauseManager.Pause();
            Assert.AreEqual(0f, Time.timeScale);
        }

        [Test]
        public void Resume_SetsIsPausedFalse()
        {
            PauseManager.Pause();
            PauseManager.Resume();
            Assert.IsFalse(PauseManager.IsPaused);
        }

        [Test]
        public void Resume_RestoresTimeScale()
        {
            PauseManager.Pause();
            PauseManager.Resume();
            Assert.AreEqual(1f, Time.timeScale);
        }

        [Test]
        public void TogglePause_TogglesFromUnpausedToPaused()
        {
            Assert.IsFalse(PauseManager.IsPaused);
            PauseManager.TogglePause();
            Assert.IsTrue(PauseManager.IsPaused);
        }

        [Test]
        public void TogglePause_TogglesFromPausedToUnpaused()
        {
            PauseManager.Pause();
            Assert.IsTrue(PauseManager.IsPaused);
            PauseManager.TogglePause();
            Assert.IsFalse(PauseManager.IsPaused);
        }

        [Test]
        public void Pause_RaisesEventWithTrueParameter()
        {
            bool eventRaised = false;
            bool eventValue = false;

            PauseManager.OnPauseStateChanged += (isPaused) =>
            {
                eventRaised = true;
                eventValue = isPaused;
            };

            PauseManager.Pause();

            Assert.IsTrue(eventRaised);
            Assert.IsTrue(eventValue);
        }

        [Test]
        public void Resume_RaisesEventWithFalseParameter()
        {
            PauseManager.Pause();

            bool eventRaised = false;
            bool eventValue = true;

            PauseManager.OnPauseStateChanged += (isPaused) =>
            {
                eventRaised = true;
                eventValue = isPaused;
            };

            PauseManager.Resume();

            Assert.IsTrue(eventRaised);
            Assert.IsFalse(eventValue);
        }

        [Test]
        public void Pause_WhenAlreadyPaused_DoesNotRaiseEvent()
        {
            PauseManager.Pause();

            int eventCount = 0;
            PauseManager.OnPauseStateChanged += (_) => eventCount++;

            PauseManager.Pause();

            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotRaiseEvent()
        {
            int eventCount = 0;
            PauseManager.OnPauseStateChanged += (_) => eventCount++;

            PauseManager.Resume();

            Assert.AreEqual(0, eventCount);
        }
    }
}
