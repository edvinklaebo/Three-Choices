using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Tests.PlayModeTests
{
    /// <summary>
    ///     PlayMode tests for the Continue Game functionality.
    ///     Ensures that continuing a saved game loads the player state correctly.
    /// </summary>
    public class ContinueGamePlayModeTests
    {
        private const float MaxWaitTime = 10f;
        private const float FrameWait = 0.1f;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Clean up any saved state before each test
            SaveService.Delete();

            // Clear any event subscriptions
            GameEvents.CharacterSelected_Event = null;
            GameEvents.NewRunRequested_Event = null;

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            // Clean up after tests
            SaveService.Delete();
            GameEvents.CharacterSelected_Event = null;
            GameEvents.NewRunRequested_Event = null;

            // Clean up any persisting RunController
            var runController = Object.FindFirstObjectByType<RunController>();
            if (runController != null)
                Object.Destroy(runController.gameObject);

            yield return null;
        }

        [UnityTest]
        public IEnumerator ContinueButton_IsVisible_WhenSaveExists()
        {
            // === SETUP: Create a saved game ===
            var savedRun = new RunState
            {
                fightIndex = 3,
                player = new Unit("SavedHero")
                {
                    Stats = new Stats
                    {
                        MaxHP = 100,
                        CurrentHP = 75,
                        AttackPower = 12,
                        Armor = 5,
                        Speed = 10
                    }
                }
            };
            SaveService.Save(savedRun);
            Assert.IsTrue(SaveService.HasSave(), "Save file should exist");

            // === PHASE 1: Load main menu ===
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);

            // === PHASE 2: Verify continue button is visible ===
            var continueButton = FindButtonByName("Continue");
            Assert.IsNotNull(continueButton, "Continue button should exist");
            Assert.IsTrue(continueButton.gameObject.activeInHierarchy,
                "Continue button should be visible when save exists");

            Log.Info("[Test] Continue button is visible when save exists");
        }

        [UnityTest]
        public IEnumerator ContinueButton_IsHidden_WhenNoSaveExists()
        {
            // === SETUP: Ensure no save exists ===
            SaveService.Delete();
            Assert.IsFalse(SaveService.HasSave(), "Save file should not exist");

            // === PHASE 1: Load main menu ===
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name);

            // === PHASE 2: Verify continue button is hidden ===
            var continueButton = FindButtonByName("Continue Button");
            Assert.IsNotNull(continueButton, "Continue button should exist");
            Assert.IsFalse(continueButton.gameObject.activeInHierarchy,
                "Continue button should be hidden when no save exists");

            Log.Info("[Test] Continue button is hidden when no save exists");
        }

        [UnityTest]
        public IEnumerator ContinueGame_LoadsPlayerStateCorrectly()
        {
            // === SETUP: Create RunController and save a game state ===
            var runControllerPrefab = Resources.Load<GameObject>("RunController");
            if (runControllerPrefab == null)
            {
                // If prefab doesn't exist in Resources, create manually
                var rcGO = new GameObject("RunController");
                rcGO.AddComponent<RunController>();
                Object.DontDestroyOnLoad(rcGO);
            }

            var savedRun = new RunState
            {
                fightIndex = 5,
                player = new Unit("ContinuedHero")
                {
                    Stats = new Stats
                    {
                        MaxHP = 150,
                        CurrentHP = 80,
                        AttackPower = 15,
                        Armor = 7,
                        Speed = 12
                    }
                }
            };
            SaveService.Save(savedRun);

            // === PHASE 1: Load main menu ===
            yield return SceneManager.LoadSceneAsync("MainMenu");
            yield return new WaitForSeconds(0.5f);

            var runController = Object.FindFirstObjectByType<RunController>();
            Assert.IsNotNull(runController, "RunController should exist");

            // === PHASE 2: Click continue button ===
            var continueButton = FindButtonByName("Continue");
            Assert.IsNotNull(continueButton, "Continue button should exist");
            Assert.IsTrue(continueButton.gameObject.activeInHierarchy,
                "Continue button should be visible");

            continueButton.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            // === PHASE 3: Verify player state is loaded correctly ===
            // The continue button should trigger ContinueRun which loads the state
            Assert.IsNotNull(runController.Player, "Player should be loaded");
            Assert.IsNotNull(runController.CurrentRun, "CurrentRun should be loaded");

            // Verify the loaded state matches what we saved
            Assert.AreEqual("ContinuedHero", runController.Player.Name,
                "Player name should match saved state");
            Assert.AreEqual(62, runController.Player.Stats.CurrentHP,
                "Player CurrentHP should match saved state");
            Assert.AreEqual(150, runController.Player.Stats.MaxHP,
                "Player MaxHP should match saved state");
            Assert.AreEqual(15, runController.Player.Stats.AttackPower,
                "Player AttackPower should match saved state");
            Assert.AreEqual(7, runController.Player.Stats.Armor,
                "Player Armor should match saved state");
            Assert.AreEqual(12, runController.Player.Stats.Speed,
                "Player Speed should match saved state");

            Assert.AreEqual(6, runController.CurrentRun.fightIndex,
                "Fight index should be incremented once by CombatController.Start() raising requestNextFight");

            Log.Info("[Test] Continue game loaded player state correctly");
            Log.Info($"[Test] Player: {runController.Player.Name}, " +
                      $"HP: {runController.Player.Stats.CurrentHP}/{runController.Player.Stats.MaxHP}, " +
                      $"Attack: {runController.Player.Stats.AttackPower}, Fight: {runController.CurrentRun.fightIndex}");

            // === PHASE 4: Wait for scene transition to DraftScene ===
            var waitTime = 0f;
            while (SceneManager.GetActiveScene().name != "DraftScene" && waitTime < MaxWaitTime)
            {
                yield return new WaitForSeconds(FrameWait);
                waitTime += FrameWait;
            }

            Assert.AreEqual("DraftScene", SceneManager.GetActiveScene().name,
                "Continue should load DraftScene");

            Log.Info("[Test] Successfully transitioned to DraftScene after continue");
        }

        /// <summary>
        ///     Helper method to find a button by its text or GameObject name
        /// </summary>
        private Button FindButtonByName(string name)
        {
            var allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var button in allButtons)
            {
                Log.Info($"Button: {button.name}");

                // Check GameObject name
                if (button.gameObject.name.Contains(name))
                    return button;

                // Check text component
                var text = button.GetComponentInChildren<Text>();
                if (text != null && text.text.Contains(name))
                    return button;

                // Check TextMeshPro
                var tmpText = button.GetComponentInChildren<TextMeshProUGUI>();
                if (tmpText != null && tmpText.text.Contains(name))
                    return button;
            }

            return null;
        }
    }
}