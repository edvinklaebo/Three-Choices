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
        private static Dictionary<Unit, UnitUIBinding> GetBindings(UIService service)
        {
            var field = typeof(UIService).GetField("_bindings", NonPublicInstance);
            return (Dictionary<Unit, UnitUIBinding>)field.GetValue(service);
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
        public void BuildBindings_WithValidCombatView_PopulatesBothUnits()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetCombatView(combatView);
            uiService.BuildBindings(player, enemy);

            var bindings = GetBindings(uiService);
            Assert.AreEqual(2, bindings.Count, "Bindings should contain both player and enemy");
            Assert.IsTrue(bindings.ContainsKey(player), "Bindings should contain the player unit");
            Assert.IsTrue(bindings.ContainsKey(enemy), "Bindings should contain the enemy unit");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BuildBindings_WithValidCombatView_StoresCorrectUnitViews()
        {
            var (root, combatView, playerView, enemyView) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetCombatView(combatView);
            uiService.BuildBindings(player, enemy);

            var bindings = GetBindings(uiService);
            Assert.AreEqual(playerView, bindings[player].UnitView, "Player binding should hold the player UnitView");
            Assert.AreEqual(enemyView, bindings[enemy].UnitView, "Enemy binding should hold the enemy UnitView");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BuildBindings_WithValidCombatView_StoresHealthBars()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetCombatView(combatView);
            uiService.BuildBindings(player, enemy);

            var bindings = GetBindings(uiService);
            Assert.IsNotNull(bindings[player].HealthBar, "Player binding should hold a HealthBarUI");
            Assert.IsNotNull(bindings[enemy].HealthBar, "Enemy binding should hold a HealthBarUI");

            Object.DestroyImmediate(root);
        }

        [Test]
        public void BuildBindings_WithNullCombatView_LeavesBindingsEmpty()
        {
            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            var uiService = new UIService();
            // Do not call SetCombatView
            Assert.DoesNotThrow(() => uiService.BuildBindings(player, enemy));

            var bindings = GetBindings(uiService);
            Assert.AreEqual(0, bindings.Count, "Bindings should be empty when CombatView is null");
        }

        [Test]
        public void BuildBindings_WithNullPlayer_DoesNotThrow()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var uiService = new UIService();
            uiService.SetCombatView(combatView);

            Assert.DoesNotThrow(() => uiService.BuildBindings(null, CreateUnit("Enemy")));

            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetCombatView_WhenCombatViewIsNull_ClearsExistingBindings()
        {
            var (root, combatView, _, _) = CreateCombatViewHierarchy();

            var player = CreateUnit("Player");
            var enemy = CreateUnit("Enemy");

            combatView.Initialize(player, enemy);

            var uiService = new UIService();
            uiService.SetCombatView(combatView);
            uiService.BuildBindings(player, enemy);

            // Confirm bindings were built
            Assert.AreEqual(2, GetBindings(uiService).Count);

            // Setting a new (null) CombatView should clear bindings
            uiService.SetCombatView(null);

            Assert.AreEqual(0, GetBindings(uiService).Count, "SetCombatView should clear existing bindings");

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
            uiService.SetCombatView(combatView);
            uiService.BuildBindings(player, enemy);

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
