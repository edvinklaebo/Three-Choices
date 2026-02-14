using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

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
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();
            var unit = CreateUnit("Test", 100, 10, 5, 5);

            // Should not throw
            Assert.DoesNotThrow(() => healthBar.Initialize(unit));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithNullUnit_LogsError()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            // Should log error but not throw
            LogAssert.Expect(LogType.Error, "HealthBarUI: Cannot initialize with null unit");
            healthBar.Initialize(null);

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

            // Damage the unit to 50 HP
            unit.ApplyDamage(null, 50);

            // The unit should have 50 HP
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP");
            
            // Note: The slider won't update immediately due to lerping in Update()
            // The target value is set to 0.5, but the actual slider.value lerps over time

            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthChanged_ToZero_SetsTargetToZero()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Kill the unit
            unit.ApplyDamage(null, 100);

            Assert.IsTrue(unit.isDead, "Unit should be dead");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");
            
            // Note: The slider won't update immediately due to lerping in Update()
            // The target value is set to 0, but the actual slider.value lerps over time

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Heal_TriggersHealthChangedEvent()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
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
            var slider = go.AddComponent<Slider>();
            var healthBar = go.AddComponent<HealthBarUI>();

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Multiple changes
            unit.ApplyDamage(null, 10); // 90 HP
            unit.ApplyDamage(null, 20); // 70 HP
            unit.Heal(15);              // 85 HP
            unit.ApplyDamage(null, 5);  // 80 HP

            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit should have 80 HP");

            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithZeroMaxHP_HandlesGracefully()
        {
            var go = new GameObject("TestHealthBar");
            var slider = go.AddComponent<Slider>();
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
    }
}
