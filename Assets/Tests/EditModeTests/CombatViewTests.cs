using NUnit.Framework;
using UnityEngine;

namespace Tests.EditModeTests
{
    public class CombatViewTests
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
        public void Initialize_WithValidUnits_DoesNotThrow()
        {
            var go = new GameObject("TestCombatView");
            var combatView = go.AddComponent<CombatView>();

            // Create mock child objects
            var playerViewGo = new GameObject("PlayerView");
            playerViewGo.transform.SetParent(go.transform);
            var playerView = playerViewGo.AddComponent<UnitView>();

            var enemyViewGo = new GameObject("EnemyView");
            enemyViewGo.transform.SetParent(go.transform);
            var enemyView = enemyViewGo.AddComponent<UnitView>();

            var hudGo = new GameObject("CombatHUD");
            hudGo.transform.SetParent(go.transform);
            var hud = hudGo.AddComponent<CombatHUD>();

            // Set serialized fields via reflection (in production would use inspector)
            var playerViewField = typeof(CombatView).GetField("_playerView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerViewField.SetValue(combatView, playerView);

            var enemyViewField = typeof(CombatView).GetField("_enemyView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            enemyViewField.SetValue(combatView, enemyView);

            var hudField = typeof(CombatView).GetField("_combatHUD", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            hudField.SetValue(combatView, hud);

            var player = CreateUnit("Player", 100, 10, 5, 5);
            var enemy = CreateUnit("Enemy", 80, 8, 3, 4);

            Assert.DoesNotThrow(() => combatView.Initialize(player, enemy));

            Object.DestroyImmediate(go);
        }

        [Test]
        public void PlayerView_ReturnsCorrectReference()
        {
            var go = new GameObject("TestCombatView");
            var combatView = go.AddComponent<CombatView>();

            var playerViewGo = new GameObject("PlayerView");
            playerViewGo.transform.SetParent(go.transform);
            var playerView = playerViewGo.AddComponent<UnitView>();

            var field = typeof(CombatView).GetField("_playerView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(combatView, playerView);

            Assert.AreEqual(playerView, combatView.PlayerView);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void EnemyView_ReturnsCorrectReference()
        {
            var go = new GameObject("TestCombatView");
            var combatView = go.AddComponent<CombatView>();

            var enemyViewGo = new GameObject("EnemyView");
            enemyViewGo.transform.SetParent(go.transform);
            var enemyView = enemyViewGo.AddComponent<UnitView>();

            var field = typeof(CombatView).GetField("_enemyView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(combatView, enemyView);

            Assert.AreEqual(enemyView, combatView.EnemyView);

            Object.DestroyImmediate(go);
        }
    }
}
