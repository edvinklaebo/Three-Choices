using Core.StatusEffects;

using NUnit.Framework;

using UnityEngine;

namespace Tests.EditModeTests
{
    public class StatusEffectDefinitionTests
    {
        private static StatusEffectDefinition CreateDefinition(
            string id = "Poison",
            string displayName = "Poison",
            string description = "Deals damage each turn based on stacks.",
            Color color = default)
        {
            var def = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            def.EditorInit(id, displayName, description, color == default ? new Color(0.5f, 0f, 0.8f) : color);
            return def;
        }

        [Test]
        public void Id_ReturnsCorrectValue()
        {
            var def = CreateDefinition(id: "Poison");
            Assert.AreEqual("Poison", def.Id);
        }

        [Test]
        public void DisplayName_ReturnsCorrectValue()
        {
            var def = CreateDefinition(displayName: "Poison");
            Assert.AreEqual("Poison", def.DisplayName);
        }

        [Test]
        public void Description_ReturnsCorrectValue()
        {
            var def = CreateDefinition(description: "Deals 3 damage per stack each turn.");
            Assert.AreEqual("Deals 3 damage per stack each turn.", def.Description);
        }

        [Test]
        public void Color_ReturnsCorrectValue()
        {
            var expected = new Color(0.5f, 0f, 0.8f);
            var def = CreateDefinition(color: expected);
            Assert.AreEqual(expected, def.Color);
        }

        [Test]
        public void Icon_IsNullByDefault()
        {
            var def = CreateDefinition();
            Assert.IsNull(def.Icon);
        }

        [Test]
        public void CreateInstance_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => ScriptableObject.CreateInstance<StatusEffectDefinition>());
        }
    }

    public class StatusEffectLibraryTests
    {
        private static StatusEffectDefinition CreateDefinition(string id, string displayName)
        {
            var def = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            def.EditorInit(id, displayName, "A test description.", Color.white);
            return def;
        }

        private static StatusEffectLibrary CreateLibrary(params StatusEffectDefinition[] defs)
        {
            var lib = ScriptableObject.CreateInstance<StatusEffectLibrary>();
            foreach (var d in defs)
                lib.EditorAddDefinition(d);
            return lib;
        }

        [Test]
        public void GetDefinition_ReturnsCorrectDefinition_ForKnownId()
        {
            var poisonDef = CreateDefinition("Poison", "Poison");
            var lib = CreateLibrary(poisonDef);

            var result = lib.GetDefinition("Poison");

            Assert.AreSame(poisonDef, result);
        }

        [Test]
        public void GetDefinition_ReturnsNull_ForUnknownId()
        {
            var lib = CreateLibrary(CreateDefinition("Poison", "Poison"));

            var result = lib.GetDefinition("UnknownEffect");

            Assert.IsNull(result);
        }

        [Test]
        public void GetDefinition_ReturnsNull_ForNullId()
        {
            var lib = CreateLibrary(CreateDefinition("Poison", "Poison"));

            var result = lib.GetDefinition(null);

            Assert.IsNull(result);
        }

        [Test]
        public void GetDefinition_ReturnsNull_ForEmptyId()
        {
            var lib = CreateLibrary(CreateDefinition("Poison", "Poison"));

            var result = lib.GetDefinition(string.Empty);

            Assert.IsNull(result);
        }

        [Test]
        public void GetDefinition_FindsAllThreeDefaultEffects()
        {
            var poisonDef = CreateDefinition("Poison", "Poison");
            var bleedDef  = CreateDefinition("Bleed", "Bleed");
            var burnDef   = CreateDefinition("Burn", "Burn");
            var lib = CreateLibrary(poisonDef, bleedDef, burnDef);

            Assert.AreSame(poisonDef, lib.GetDefinition("Poison"));
            Assert.AreSame(bleedDef,  lib.GetDefinition("Bleed"));
            Assert.AreSame(burnDef,   lib.GetDefinition("Burn"));
        }

        [Test]
        public void EditorAddDefinition_IgnoresNull()
        {
            var lib = CreateLibrary();
            Assert.DoesNotThrow(() => lib.EditorAddDefinition(null));
            Assert.IsNull(lib.GetDefinition("any"));
        }

        [Test]
        public void EditorAddDefinition_IgnoresDuplicate()
        {
            var def = CreateDefinition("Poison", "Poison");
            var lib = CreateLibrary(def);
            lib.EditorAddDefinition(def); // add again

            // Should still resolve correctly (not crash or duplicate)
            Assert.AreSame(def, lib.GetDefinition("Poison"));
        }
    }
}
