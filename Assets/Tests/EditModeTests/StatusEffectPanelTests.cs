using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class StatusEffectPanelTests
    {
        private static Unit CreateUnit(string name, int hp)
        {
            return new Unit(name)
            {
                Stats = new Stats
                {
                    MaxHP = hp,
                    CurrentHP = hp,
                    AttackPower = 10,
                    Armor = 5,
                    Speed = 5
                }
            };
        }

        [Test]
        public void Initialize_WithValidUnit_DoesNotThrow()
        {
            var go = new GameObject("TestStatusEffectPanel");
            var panel = go.AddComponent<StatusEffectPanel>();

            var unit = CreateUnit("Test", 100);

            Assert.DoesNotThrow(() => panel.Initialize(unit));

            Object.DestroyImmediate(go);
        }


        [Test]
        public void RefreshDisplay_WithNoEffects_DoesNotCrash()
        {
            var go = new GameObject("TestStatusEffectPanel");
            var panel = go.AddComponent<StatusEffectPanel>();

            // Create a simple icon prefab
            var iconPrefab = new GameObject("IconPrefab");
            var icon = iconPrefab.AddComponent<StatusEffectIcon>();

            var field = typeof(StatusEffectPanel).GetField("_iconPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(panel, icon);

            var unit = CreateUnit("Test", 100);
            panel.Initialize(unit);

            // No status effects - should handle gracefully
            Assert.AreEqual(0, unit.StatusEffects.Count);

            Object.DestroyImmediate(iconPrefab);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RefreshDisplay_WithMultipleEffects_HandlesCorrectly()
        {
            var go = new GameObject("TestStatusEffectPanel");
            var panel = go.AddComponent<StatusEffectPanel>();

            // Create a simple icon prefab
            var iconPrefab = new GameObject("IconPrefab");
            var icon = iconPrefab.AddComponent<StatusEffectIcon>();

            var field = typeof(StatusEffectPanel).GetField("_iconPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(panel, icon);

            var unit = CreateUnit("Test", 100);
            
            // Add some status effects
            unit.ApplyStatus(new Poison(5, 3));
            unit.ApplyStatus(new Bleed(3, 2));

            panel.Initialize(unit);

            Assert.AreEqual(2, unit.StatusEffects.Count);

            // Cleanup: Destroy go first to clean up instantiated icons, then prefab
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(iconPrefab);
        }
    }
}
