using System;
using System.Linq;
using NUnit.Framework;

namespace Tests.EditModeTests
{
    public class MonsterNameTests
    {
        [SetUp]
        public void Setup()
        {
            // Make tests deterministic
            MonsterName.random = new Random(0);
        }

        [Test]
        public void Random_ReturnsNonEmptyString()
        {
            var name = MonsterName.Random();

            Assert.IsFalse(string.IsNullOrWhiteSpace(name));
        }

        [Test]
        public void Random_AlwaysUsesValidPrefixAndSuffix()
        {
            for (var i = 0; i < 200; i++)
            {
                var name = MonsterName.Random();

                var baseName = name.Split(" the ")[0];

                var hasValidPrefix = MonsterName.prefixes.Any(p => baseName.StartsWith(p));
                var hasValidSuffix = MonsterName.suffixes.Any(s => baseName.EndsWith(s));

                Assert.IsTrue(hasValidPrefix, $"Invalid prefix in '{name}'");
                Assert.IsTrue(hasValidSuffix, $"Invalid suffix in '{name}'");
            }
        }

        [Test]
        public void Random_TitleIfPresent_IsFromTitleList()
        {
            for (var i = 0; i < 300; i++)
            {
                var name = MonsterName.Random();

                if (name.Contains(" the "))
                {
                    var title = "the " + name.Split(" the ")[1];

                    Assert.Contains(title, MonsterName.titles.ToList());
                }
            }
        }

        [Test]
        public void Random_SometimesHasTitle_AndSometimesNot()
        {
            var withTitle = 0;
            var withoutTitle = 0;

            for (var i = 0; i < 1000; i++)
            {
                var name = MonsterName.Random();

                if (name.Contains(" the "))
                    withTitle++;
                else
                    withoutTitle++;
            }

            // Loose bounds to avoid flaky tests
            Assert.Greater(withTitle, 200, "Too few names with titles generated");
            Assert.Greater(withoutTitle, 200, "Too few names without titles generated");
        }

        [Test]
        public void Random_WithFixedSeed_IsDeterministic()
        {
            MonsterName.random = new Random(42);

            var first = MonsterName.Random();
            var second = MonsterName.Random();

            MonsterName.random = new Random(42);

            var firstAgain = MonsterName.Random();
            var secondAgain = MonsterName.Random();

            Assert.AreEqual(first, firstAgain);
            Assert.AreEqual(second, secondAgain);
        }
    }
}