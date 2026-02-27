using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class IntroSequenceTests
    {
        private IntroSequence _sequence;

        [SetUp]
        public void SetUp()
        {
            _sequence = new IntroSequence(new[] { "Line A", "Line B", "Line C" });
        }

        [Test]
        public void Constructor_ThrowsOnNullLines()
        {
            Assert.Throws<System.ArgumentException>(() => new IntroSequence(null));
        }

        [Test]
        public void Constructor_ThrowsOnEmptyLines()
        {
            Assert.Throws<System.ArgumentException>(() => new IntroSequence(new string[0]));
        }

        [Test]
        public void InitialState_CurrentIndexIsZero()
        {
            Assert.AreEqual(0, _sequence.CurrentIndex);
        }

        [Test]
        public void InitialState_IsNotComplete()
        {
            Assert.IsFalse(_sequence.IsComplete);
        }

        [Test]
        public void TotalLines_ReturnsCorrectCount()
        {
            Assert.AreEqual(3, _sequence.TotalLines);
        }

        [Test]
        public void ShowNext_FiresOnLineShownWithCorrectText()
        {
            string shown = null;
            _sequence.OnLineShown += line => shown = line;

            _sequence.ShowNext();

            Assert.AreEqual("Line A", shown);
        }

        [Test]
        public void ShowNext_AdvancesCurrentIndex()
        {
            _sequence.ShowNext();

            Assert.AreEqual(1, _sequence.CurrentIndex);
        }

        [Test]
        public void ShowNext_ShowsAllLinesInOrder()
        {
            var shown = new List<string>();
            _sequence.OnLineShown += line => shown.Add(line);

            _sequence.ShowNext();
            _sequence.ShowNext();
            _sequence.ShowNext();

            Assert.AreEqual(new[] { "Line A", "Line B", "Line C" }, shown);
        }

        [Test]
        public void ShowNext_FiresOnCompleteAfterLastLine()
        {
            bool completed = false;
            _sequence.OnComplete += () => completed = true;

            _sequence.ShowNext();
            _sequence.ShowNext();
            _sequence.ShowNext();

            Assert.IsTrue(completed);
        }

        [Test]
        public void ShowNext_DoesNothingWhenAlreadyComplete()
        {
            _sequence.Skip();
            int callCount = 0;
            _sequence.OnComplete += () => callCount++;

            _sequence.ShowNext();

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void IsComplete_TrueAfterAllLinesShown()
        {
            _sequence.ShowNext();
            _sequence.ShowNext();
            _sequence.ShowNext();

            Assert.IsTrue(_sequence.IsComplete);
        }

        [Test]
        public void Skip_SetsIsCompleteImmediately()
        {
            _sequence.Skip();

            Assert.IsTrue(_sequence.IsComplete);
        }

        [Test]
        public void Skip_FiresOnComplete()
        {
            bool completed = false;
            _sequence.OnComplete += () => completed = true;

            _sequence.Skip();

            Assert.IsTrue(completed);
        }

        [Test]
        public void Skip_DoesNothingWhenAlreadyComplete()
        {
            _sequence.Skip();
            int callCount = 0;
            _sequence.OnComplete += () => callCount++;

            _sequence.Skip();

            Assert.AreEqual(0, callCount);
        }

        [Test]
        public void DefaultLines_ContainsExpectedFirstLine()
        {
            Assert.AreEqual("The world once feared necromancers.", IntroSequence.DefaultLines[0]);
        }

        [Test]
        public void DefaultLines_ContainsExpectedLastLine()
        {
            var last = IntroSequence.DefaultLines[IntroSequence.DefaultLines.Length - 1];
            Assert.AreEqual("You will sit upon the Bone Throne.", last);
        }

        [Test]
        public void DefaultLines_Has25Lines()
        {
            Assert.AreEqual(25, IntroSequence.DefaultLines.Length);
        }
    }
}
