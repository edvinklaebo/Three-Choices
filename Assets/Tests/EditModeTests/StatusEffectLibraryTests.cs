using Core.StatusEffects;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    /// <summary>
    /// Tests for <see cref="StatusEffectLibrary"/>.
    /// Verifies lookup by ID, null/empty ID handling, and duplicate-ID deduplication.
    /// </summary>
    public class StatusEffectLibraryTests
    {
        private StatusEffectLibrary _library;

        [SetUp]
        public void Setup()
        {
            _library = ScriptableObject.CreateInstance<StatusEffectLibrary>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(_library);
        }

        private static StatusEffectDefinition MakeDef(string id, string displayName = "Effect")
        {
            var def = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            def.EditorInit(id, displayName, "desc", Color.white);
            return def;
        }

        // ---- GetDefinition ----

        [Test]
        public void GetDefinition_KnownId_ReturnsCorrectDefinition()
        {
            var def = MakeDef("Poison");
            _library.EditorAddDefinition(def);

            var result = _library.GetDefinition("Poison");

            Assert.AreEqual(def, result, "Should return the definition registered under 'Poison'");
            Object.DestroyImmediate(def);
        }

        [Test]
        public void GetDefinition_UnknownId_ReturnsNull()
        {
            var result = _library.GetDefinition("NonExistent");

            Assert.IsNull(result, "Unknown ID should return null");
        }

        [Test]
        public void GetDefinition_NullId_ReturnsNull()
        {
            var result = _library.GetDefinition(null);

            Assert.IsNull(result, "Null ID should return null");
        }

        [Test]
        public void GetDefinition_EmptyId_ReturnsNull()
        {
            var result = _library.GetDefinition(string.Empty);

            Assert.IsNull(result, "Empty string ID should return null");
        }

        [Test]
        public void GetDefinition_MultipleDefinitions_ReturnsCorrectOne()
        {
            var poison = MakeDef("Poison", "Poison");
            var bleed = MakeDef("Bleed", "Bleed");
            var burn = MakeDef("Burn", "Burn");

            _library.EditorAddDefinition(poison);
            _library.EditorAddDefinition(bleed);
            _library.EditorAddDefinition(burn);

            Assert.AreEqual(poison, _library.GetDefinition("Poison"));
            Assert.AreEqual(bleed, _library.GetDefinition("Bleed"));
            Assert.AreEqual(burn, _library.GetDefinition("Burn"));

            Object.DestroyImmediate(poison);
            Object.DestroyImmediate(bleed);
            Object.DestroyImmediate(burn);
        }

        // ---- Duplicate ID handling ----

        [Test]
        public void GetDefinition_DuplicateId_FirstDefinitionWins()
        {
            var first = MakeDef("Poison", "First Poison");
            var second = MakeDef("Poison", "Second Poison");

            _library.EditorAddDefinition(first);
            _library.EditorAddDefinition(second);

            var result = _library.GetDefinition("Poison");

            Assert.AreEqual(first, result, "First registered definition should win on duplicate ID");

            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        // ---- Lazy lookup rebuild ----

        [Test]
        public void GetDefinition_CalledMultipleTimes_ReturnsSameResult()
        {
            var def = MakeDef("Bleed");
            _library.EditorAddDefinition(def);

            var first = _library.GetDefinition("Bleed");
            var second = _library.GetDefinition("Bleed");

            Assert.AreEqual(first, second, "Repeated lookups should return the same definition");
            Object.DestroyImmediate(def);
        }

        [Test]
        public void EditorAddDefinition_NullDefinition_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _library.EditorAddDefinition(null),
                "Adding a null definition should not throw");
        }

        [Test]
        public void EditorAddDefinition_SameDefinitionTwice_DoesNotDuplicate()
        {
            var def = MakeDef("Poison");
            _library.EditorAddDefinition(def);
            _library.EditorAddDefinition(def); // duplicate

            // Should still return the correct definition (no crash, no duplicate entry issue)
            var result = _library.GetDefinition("Poison");
            Assert.AreEqual(def, result, "Duplicate add should not cause issues with lookup");

            Object.DestroyImmediate(def);
        }
    }
}
