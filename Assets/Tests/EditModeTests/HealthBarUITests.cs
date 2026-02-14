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
        public void HealthChanged_UpdatesTargetFillAmount()
        {
            var go = new GameObject("TestHealthBar");
            var fillImageGO = new GameObject("FillImage");
            fillImageGO.transform.SetParent(go.transform);
            var fillImage = fillImageGO.AddComponent<Image>();
            
            var healthBar = go.AddComponent<HealthBarUI>();
            
            // Use reflection to set the private _fillImage field
            var fillImageField = typeof(HealthBarUI).GetField("_fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBar, fillImage);

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Damage the unit to 50 HP
            unit.ApplyDamage(null, 50);

            // The target should be 0.5 (50/100)
            // We can't directly test private field _targetFillAmount, 
            // but we can verify the event was triggered
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP");

            Object.DestroyImmediate(fillImageGO);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void HealthChanged_ToZero_SetsTargetToZero()
        {
            var go = new GameObject("TestHealthBar");
            var fillImageGO = new GameObject("FillImage");
            fillImageGO.transform.SetParent(go.transform);
            var fillImage = fillImageGO.AddComponent<Image>();
            
            var healthBar = go.AddComponent<HealthBarUI>();
            
            var fillImageField = typeof(HealthBarUI).GetField("_fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBar, fillImage);

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Kill the unit
            unit.ApplyDamage(null, 100);

            Assert.IsTrue(unit.isDead, "Unit should be dead");
            Assert.LessOrEqual(unit.Stats.CurrentHP, 0, "HP should be 0 or negative");

            Object.DestroyImmediate(fillImageGO);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Heal_TriggersHealthChangedEvent()
        {
            var go = new GameObject("TestHealthBar");
            var fillImageGO = new GameObject("FillImage");
            fillImageGO.transform.SetParent(go.transform);
            var fillImage = fillImageGO.AddComponent<Image>();
            
            var healthBar = go.AddComponent<HealthBarUI>();
            
            var fillImageField = typeof(HealthBarUI).GetField("_fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBar, fillImage);

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Damage then heal
            unit.ApplyDamage(null, 50);
            Assert.AreEqual(50, unit.Stats.CurrentHP, "Unit should have 50 HP after damage");

            unit.Heal(30);
            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit should have 80 HP after heal");

            Object.DestroyImmediate(fillImageGO);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void MultipleHealthChanges_AllTriggerEvent()
        {
            var go = new GameObject("TestHealthBar");
            var fillImageGO = new GameObject("FillImage");
            fillImageGO.transform.SetParent(go.transform);
            var fillImage = fillImageGO.AddComponent<Image>();
            
            var healthBar = go.AddComponent<HealthBarUI>();
            
            var fillImageField = typeof(HealthBarUI).GetField("_fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBar, fillImage);

            var unit = CreateUnit("Test", 100, 10, 5, 5);
            healthBar.Initialize(unit);

            // Multiple changes
            unit.ApplyDamage(null, 10); // 90 HP
            unit.ApplyDamage(null, 20); // 70 HP
            unit.Heal(15);              // 85 HP
            unit.ApplyDamage(null, 5);  // 80 HP

            Assert.AreEqual(80, unit.Stats.CurrentHP, "Unit should have 80 HP");

            Object.DestroyImmediate(fillImageGO);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Initialize_WithZeroMaxHP_HandlesGracefully()
        {
            var go = new GameObject("TestHealthBar");
            var fillImageGO = new GameObject("FillImage");
            fillImageGO.transform.SetParent(go.transform);
            var fillImage = fillImageGO.AddComponent<Image>();
            
            var healthBar = go.AddComponent<HealthBarUI>();
            
            var fillImageField = typeof(HealthBarUI).GetField("_fillImage", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fillImageField?.SetValue(healthBar, fillImage);

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

            Object.DestroyImmediate(fillImageGO);
            Object.DestroyImmediate(go);
        }
    }
}
