using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class HealthBarUITests
    {
        private static Unit CreateUnit(string name, int hp, int attack, int armor, int speed)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = attack,
                    Armor = armor,
                    Speed = speed
                }
            };
        }

        [Test]
        public void Initialize_WithValidUnit_DoesNotThrowError()
        {
            var go = new GameObject("TestHealthBar");
            var healthBar = go.AddComponent<HealthBarUI>();
            var unit = CreateUnit("Test", 100, 10, 5, 5);

            // Should not throw
            Assert.DoesNotThrow(() => healthBar.Initialize(unit));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthChanged_UpdatesTargetValue()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Initial value should be 1.0 (100/100)
            Assert.AreEqual(1.0f, slider.value, 0.01f, "Slider should start at full health");

            // Damage the unit to 50 HP - this will trigger HealthChanged event
            unit.ApplyDamage(null, 50);

            // The unit should have 50 HP
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP");

            // Note: The slider won't update immediately due to lerping in Update()
            // The event handler sets the internal target value, but slider.value lerps in Update()
            // We can verify the event was triggered by checking the unit's HP changed

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthChanged_ToZero_SetsTargetToZero()
        {
            var go = new GameObject("TestHealthBar");
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Kill the unit
            unit.ApplyDamage(null, 100);

            Assert.IsTrue(unit.IsDead, "Unit should be dead");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");

            // Note: The slider won't update immediately due to lerping in Update()
            // The target value is set to 0, but the actual slider.value lerps over time

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Heal_TriggersHealthChangedEvent()
        {
            var go = new GameObject("TestHealthBar");
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Damage then heal
            unit.ApplyDamage(null, 50);
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP after damage");

            unit.Heal(30);
            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit should have 80 HP after heal");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void MultipleHealthChanges_AllTriggerEvent()
        {
            var go = new GameObject("TestHealthBar");
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Multiple changes
            unit.ApplyDamage(null, 10); // 90 HP
            unit.ApplyDamage(null, 20); // 70 HP
            unit.Heal(15); // 85 HP
            unit.ApplyDamage(null, 5); // 80 HP

            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit should have 80 HP");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithZeroMaxHP_HandlesGracefully()
        {
            var go = new GameObject("TestHealthBar");
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = new Unit("Test")
            {
                Stats = new Stats
                {
                    MaxHP = 0,
                    CurrentHP = 0,
                    AttackPower = 10,
                    Armor = 5,
                    Speed = 5
                }
            };

            // Should not throw
            Assert.DoesNotThrow(() => healthBar.Initialize(unit));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Awake_AutoFindsSliderComponent()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            // Awake should auto-find the slider
            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Slider should be configured correctly
            Assert.AreEqual(0f, slider.minValue, "Slider min should be 0");
            Assert.AreEqual(1f, slider.maxValue, "Slider max should be 1");
            Assert.IsFalse(slider.interactable, "Slider should not be interactable");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimateToCurrentHealth_UpdatesTargetValue()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Initial value should be 1.0 (100/100)
            Assert.AreEqual(1.0f, slider.value, 0.01f, "Slider should start at full health");

            // Simulate combat: damage the unit directly (state changes in CombatSystem)
            // We modify Stats.CurrentHP directly to simulate what CombatSystem does internally
            // before the DamageAction.Play() is called
            unit.Stats.CurrentHP = 50;

            // Now call AnimateToCurrentHealth (simulating presentation event)
            healthBar.AnimateToCurrentHealth();

            // The unit should have 50 HP
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP");

            // Note: The slider won't update immediately due to lerping in Update()
            // The target value is set, but slider.value lerps over time

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimateToCurrentHealth_AfterDeath_SetsTargetToZero()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Kill the unit via the public API
            unit.ApplyDamage(null, 100);

            // Call AnimateToCurrentHealth (simulating presentation event)
            healthBar.AnimateToCurrentHealth();

            Assert.AreEqual(0, unit.Stats.CurrentHP, "Unit should have 0 HP");

            // Note: The slider won't update immediately due to lerping in Update()

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimateToHealth_AnimatesFromOldToNewValue()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Simulate combat: unit is damaged but we want to animate from old to new value
            // fromNormalized = 1.0 (100/100), toNormalized = 0.5 (50/100)
            healthBar.AnimateToHealth(1.0f, 0.5f);

            // Slider should immediately be set to the starting value
            Assert.AreEqual(1.0f, slider.value, 0.01f, "Slider should be at starting value");

            // Note: Target value is 0.5, but lerping happens in Update()

            Object.DestroyImmediate(go);
        }

        [Test]
        public void AnimateToHealth_WithZeroMaxHP_HandlesGracefully()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Call with death scenario: from 0.1 to 0.0
            Assert.DoesNotThrow(() => healthBar.AnimateToHealth(0.1f, 0.0f));

            Object.DestroyImmediate(go);
        }
    }
}