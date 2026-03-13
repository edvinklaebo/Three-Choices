using Characters;

using NUnit.Framework;

using Systems;

using UnityEngine;

namespace Tests.EditModeTests
{
    /// <summary>
    ///     Verifies that each starting class has the correct stat identity and that
    ///     every class stays within the allowed weighted stat budget (95–105).
    /// </summary>
    public class ClassBalanceTests
    {
        // Stat weights as specified in the balance document.
        private const float WeightHealth = 1f;
        private const float WeightAttack = 2f;
        private const float WeightDefense = 2f;
        private const float WeightSpeed = 1.5f;
        private const float WeightMana = 1f;
        private const float WeightMagicScaling = 2f;
        private const float WeightHealingScaling = 2f;
        private const float WeightStatusScaling = 2f;

        private const float BudgetMin = 95f;
        private const float BudgetMax = 105f;

        private static float WeightedBudget(CharacterDefinition c)
        {
            return c.MaxHp * WeightHealth
                 + c.Attack * WeightAttack
                 + c.Armor * WeightDefense
                 + c.Speed * WeightSpeed
                 + c.Mana * WeightMana
                 + c.MagicScaling * WeightMagicScaling
                 + c.HealingScaling * WeightHealingScaling
                 + c.StatusScaling * WeightStatusScaling;
        }

        private static CharacterDefinition MakeBob()
        {
            var c = ScriptableObject.CreateInstance<CharacterDefinition>();
            c.DisplayName = "Bob";
            c.MaxHp = 20;
            c.Attack = 5;
            c.Armor = 4;
            c.Speed = 10;
            c.Mana = 16;
            c.MagicScaling = 15;
            c.HealingScaling = 0;
            c.StatusScaling = 0;
            return c;
        }

        private static CharacterDefinition MakeGarry()
        {
            var c = ScriptableObject.CreateInstance<CharacterDefinition>();
            c.DisplayName = "Garry";
            c.MaxHp = 40;
            c.Attack = 10;
            c.Armor = 14;
            c.Speed = 7;
            c.Mana = 3;
            c.MagicScaling = 0;
            c.HealingScaling = 0;
            c.StatusScaling = 0;
            return c;
        }

        private static CharacterDefinition MakeSteve()
        {
            var c = ScriptableObject.CreateInstance<CharacterDefinition>();
            c.DisplayName = "Steve";
            c.MaxHp = 28;
            c.Attack = 4;
            c.Armor = 8;
            c.Speed = 7;
            c.Mana = 10;
            c.MagicScaling = 0;
            c.HealingScaling = 14;
            c.StatusScaling = 0;
            return c;
        }

        private static CharacterDefinition MakeKyle()
        {
            var c = ScriptableObject.CreateInstance<CharacterDefinition>();
            c.DisplayName = "Kyle";
            c.MaxHp = 20;
            c.Attack = 10;
            c.Armor = 4;
            c.Speed = 14;
            c.Mana = 6;
            c.MagicScaling = 0;
            c.HealingScaling = 0;
            c.StatusScaling = 15;
            return c;
        }

        // ── Budget tests ──────────────────────────────────────────────────────

        [Test]
        public void Bob_WeightedBudget_IsWithinRange()
        {
            var c = MakeBob();
            var budget = WeightedBudget(c);
            Object.DestroyImmediate(c);
            Assert.That(budget, Is.InRange(BudgetMin, BudgetMax),
                $"Bob weighted budget {budget} is outside [{BudgetMin}, {BudgetMax}]");
        }

        [Test]
        public void Garry_WeightedBudget_IsWithinRange()
        {
            var c = MakeGarry();
            var budget = WeightedBudget(c);
            Object.DestroyImmediate(c);
            Assert.That(budget, Is.InRange(BudgetMin, BudgetMax),
                $"Garry weighted budget {budget} is outside [{BudgetMin}, {BudgetMax}]");
        }

        [Test]
        public void Steve_WeightedBudget_IsWithinRange()
        {
            var c = MakeSteve();
            var budget = WeightedBudget(c);
            Object.DestroyImmediate(c);
            Assert.That(budget, Is.InRange(BudgetMin, BudgetMax),
                $"Steve weighted budget {budget} is outside [{BudgetMin}, {BudgetMax}]");
        }

        [Test]
        public void Kyle_WeightedBudget_IsWithinRange()
        {
            var c = MakeKyle();
            var budget = WeightedBudget(c);
            Object.DestroyImmediate(c);
            Assert.That(budget, Is.InRange(BudgetMin, BudgetMax),
                $"Kyle weighted budget {budget} is outside [{BudgetMin}, {BudgetMax}]");
        }

        // ── Class identity tests ──────────────────────────────────────────────

        [Test]
        public void Garry_HasHighestHp_ReflectsWarriorIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(garry.MaxHp, bob.MaxHp, "Garry HP should exceed Bob HP");
            Assert.Greater(garry.MaxHp, kyle.MaxHp, "Garry HP should exceed Kyle HP");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Garry_HasHighestArmor_ReflectsWarriorIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(garry.Armor, bob.Armor, "Garry Armor should exceed Bob Armor");
            Assert.Greater(garry.Armor, kyle.Armor, "Garry Armor should exceed Kyle Armor");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Bob_HasHighestMagicScaling_ReflectsMageIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(bob.MagicScaling, garry.MagicScaling, "Bob MagicScaling should exceed Garry");
            Assert.Greater(bob.MagicScaling, steve.MagicScaling, "Bob MagicScaling should exceed Steve");
            Assert.Greater(bob.MagicScaling, kyle.MagicScaling, "Bob MagicScaling should exceed Kyle");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Bob_HasHighestMana_ReflectsMageIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(bob.Mana, garry.Mana, "Bob Mana should exceed Garry");
            Assert.Greater(bob.Mana, kyle.Mana, "Bob Mana should exceed Kyle");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Steve_HasHighestHealingScaling_ReflectsPriestIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(steve.HealingScaling, garry.HealingScaling, "Steve HealingScaling should exceed Garry");
            Assert.Greater(steve.HealingScaling, bob.HealingScaling, "Steve HealingScaling should exceed Bob");
            Assert.Greater(steve.HealingScaling, kyle.HealingScaling, "Steve HealingScaling should exceed Kyle");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Kyle_HasHighestSpeed_ReflectsRogueIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(kyle.Speed, garry.Speed, "Kyle Speed should exceed Garry");
            Assert.Greater(kyle.Speed, steve.Speed, "Kyle Speed should exceed Steve");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        [Test]
        public void Kyle_HasHighestStatusScaling_ReflectsRogueIdentity()
        {
            var garry = MakeGarry();
            var bob = MakeBob();
            var steve = MakeSteve();
            var kyle = MakeKyle();

            Assert.Greater(kyle.StatusScaling, garry.StatusScaling, "Kyle StatusScaling should exceed Garry");
            Assert.Greater(kyle.StatusScaling, bob.StatusScaling, "Kyle StatusScaling should exceed Bob");
            Assert.Greater(kyle.StatusScaling, steve.StatusScaling, "Kyle StatusScaling should exceed Steve");

            Object.DestroyImmediate(garry);
            Object.DestroyImmediate(bob);
            Object.DestroyImmediate(steve);
            Object.DestroyImmediate(kyle);
        }

        // ── PlayerFactory mapping tests ───────────────────────────────────────

        [Test]
        public void PlayerFactory_MapsAllNewStats_Correctly()
        {
            var c = ScriptableObject.CreateInstance<CharacterDefinition>();
            c.DisplayName = "Test";
            c.MaxHp = 30;
            c.Attack = 8;
            c.Armor = 10;
            c.Speed = 9;
            c.Mana = 12;
            c.MagicScaling = 5;
            c.HealingScaling = 7;
            c.StatusScaling = 3;

            var unit = PlayerFactory.CreateFromCharacter(c);

            Assert.AreEqual(12, unit.Stats.Mana);
            Assert.AreEqual(5, unit.Stats.MagicScaling);
            Assert.AreEqual(7, unit.Stats.HealingScaling);
            Assert.AreEqual(3, unit.Stats.StatusScaling);

            Object.DestroyImmediate(c);
        }
    }
}
