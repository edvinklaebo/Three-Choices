using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Tests.EditModeTests
{
    public class UIServiceTests
    {
        private static readonly BindingFlags NonPublicInstance =
            BindingFlags.NonPublic | BindingFlags.Instance;

        private static Unit CreateUnit(string name, int hp = 100)
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

        /// <summary>
        /// Reads the private _bindings dictionary from a UIService instance.
        /// </summary>
        private static IReadOnlyDictionary<Unit, UnitUIBinding> GetBindings(UIService service)
        {
            var field = typeof(UIService).GetField("_bindings", NonPublicInstance);
            return (IReadOnlyDictionary<Unit, UnitUIBinding>)field?.GetValue(service);
        }

        /// <summary>
        /// Creates a minimal CombatView hierarchy with real UnitView and CombatHUD components.
        /// Returns the root GameObject (caller is responsible for destroying it).
        /// </summary>
        private static (GameObject root, CombatView combatView, UnitView playerView, UnitView enemyView)
            CreateCombatViewHierarchy()
        {
            var root = new GameObject("CombatView");
            var combatView = root.AddComponent<CombatView>();

            var playerViewGo = new GameObject("PlayerView");
            playerViewGo.transform.SetParent(root.transform);
            var playerView = playerViewGo.AddComponent<UnitView>();

            var enemyViewGo = new GameObject("EnemyView");
            enemyViewGo.transform.SetParent(root.transform);
            var enemyView = enemyViewGo.AddComponent<UnitView>();

            var hudGo = new GameObject("CombatHUD");
            hudGo.transform.SetParent(root.transform);
            var combatHUD = hudGo.AddComponent<CombatHUD>();

            // Wire serialized fields via reflection
            typeof(CombatView)
                .GetField("_playerView", NonPublicInstance)
                ?.SetValue(combatView, playerView);
            typeof(CombatView)
                .GetField("_enemyView", NonPublicInstance)
                ?.SetValue(combatView, enemyView);
            typeof(CombatView)
                .GetField("_combatHUD", NonPublicInstance)
                ?.SetValue(combatView, combatHUD);

            // Build PlayerHUD
            var playerHUDGo = new GameObject("PlayerHUD");
            var playerHUD = playerHUDGo.AddComponent<UnitHUDPanel>();
            var playerHBGo = new GameObject("HealthBar");
            playerHBGo.transform.SetParent(playerHUDGo.transform);
            playerHBGo.AddComponent<Slider>();
            var playerHB = playerHBGo.AddComponent<HealthBarUI>();
            typeof(UnitHUDPanel).GetField("_healthBar", NonPublicInstance)?.SetValue(playerHUD, playerHB);

            // Build EnemyHUD
            var enemyHUDGo = new GameObject("EnemyHUD");
            var enemyHUD = enemyHUDGo.AddComponent<UnitHUDPanel>();
            var enemyHBGo = new GameObject("HealthBar");
            enemyHBGo.transform.SetParent(enemyHUDGo.transform);
            enemyHBGo.AddComponent<Slider>();
            var enemyHB = enemyHBGo.AddComponent<HealthBarUI>();
            typeof(UnitHUDPanel).GetField("_healthBar", NonPublicInstance)?.SetValue(enemyHUD, enemyHB);

            typeof(CombatHUD).GetField("_playerHUD", NonPublicInstance)?.SetValue(combatHUD, playerHUD);
            typeof(CombatHUD).GetField("_enemyHUD", NonPublicInstance)?.SetValue(combatHUD, enemyHUD);

            return (root, combatView, playerView, enemyView);
        }

        [Test]
        public void SetBindings_WithValidBindings_PopulatesBothUnits()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetBindings(combatView.BuildBindings(player, enemy));

            var bindings = GetBindings(uiService);
            Assert.AreEqual(2, bindings.Count, "Bindings should contain both player and enemy");
            Assert.IsTrue(bindings.ContainsKey(player), "Bindings should contain the player unit");
            Assert.IsTrue(bindings.ContainsKey(enemy), "Bindings should contain the enemy unit");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetBindings_WithValidBindings_StoresCorrectUnitViews()
        {
            var (root, combatView, playerView, enemyView) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetBindings(combatView.BuildBindings(player, enemy));

            var bindings = GetBindings(uiService);
            Assert.AreEqual(playerView, bindings[player].UnitView, "Player binding should hold the player UnitView");
            Assert.AreEqual(enemyView, bindings[enemy].UnitView, "Enemy binding should hold the enemy UnitView");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetBindings_WithValidBindings_StoresHealthBars()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetBindings(combatView.BuildBindings(player, enemy));

            var bindings = GetBindings(uiService);
            Assert.IsNotNull(bindings[player].HealthBar, "Player binding should hold a HealthBarUI");
            Assert.IsNotNull(bindings[enemy].HealthBar, "Enemy binding should hold a HealthBarUI");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetBindings_WithNullBindings_LeavesBindingsEmpty()
        {
            var uiService = new UIService();
            Assert.DoesNotThrow(() => uiService.SetBindings(null));

            var bindings = GetBindings(uiService);
            Assert.AreEqual(0, bindings.Count, "Bindings should be empty when null is passed");
        }

        [Test]
        public void CombatView_BuildBindings_WithNullUnit_DoesNotThrow()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            IReadOnlyDictionary<Unit, UnitUIBinding> result = null;
            Assert.DoesNotThrow(() => result = combatView.BuildBindings(null, CreateUnit("Enemy")));
            Assert.IsNotNull(result, "BuildBindings should return an empty dictionary, not null");
            Assert.AreEqual(0, result.Count, "BuildBindings with a null unit should return empty bindings");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetBindings_WithNullBindings_ClearsExistingBindings()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetBindings(combatView.BuildBindings(player, enemy));

            // Confirm bindings were built
            Assert.AreEqual(2, GetBindings(uiService).Count);

            // Passing null should clear bindings
            uiService.SetBindings(null);

            Assert.AreEqual(0, GetBindings(uiService).Count, "SetBindings(null) should clear existing bindings");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void AnimateHealthBar_WithBuiltBindings_DoesNotThrow()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetBindings(combatView.BuildBindings(player, enemy));

            Assert.DoesNotThrow(() => uiService.AnimateHealthBar(player));
            Assert.DoesNotThrow(() => uiService.AnimateHealthBar(enemy));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void AnimateHealthBar_WithoutBindings_DoesNotThrow()
        {
            var uiService = new UIService();
            var unit = CreateUnit("Player");

            Assert.DoesNotThrow(() => uiService.AnimateHealthBar(unit));
        }
    }
}
