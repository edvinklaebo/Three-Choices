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
    ///     End-to-end regression tests for the main game flow from startup to first draft.
    ///     Tests the complete player journey: Splash → Menu → Character Select → Draft
    /// </summary>
    public class GameFlowPlayModeTests
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
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullGameFlow_FromStartToFirstDraft()
        {
            // === PHASE 1: Game starts and splash screen is displayed ===
            yield return SceneManager.LoadSceneAsync("SplashScreen");
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual("SplashScreen", SceneManager.GetActiveScene().name,
                "Game should start with SplashScreen");

            // Verify splash loader exists
            var splashLoader = Object.FindFirstObjectByType<SplashAsyncLoader>();
            Assert.IsNotNull(splashLoader, "SplashAsyncLoader should exist in SplashScreen");

            // Verify loading UI elements exist
            var progressBar = Object.FindFirstObjectByType<Slider>();
            Assert.IsNotNull(progressBar, "Loading progress bar should exist");

            Debug.Log("[Test] Splash screen verified, waiting for main menu transition...");

            // === PHASE 2: Loading screen transitions to main menu ===
            var waitTime = 0f;
            while (SceneManager.GetActiveScene().name != "MainMenu" && waitTime < MaxWaitTime)
            {
                yield return new WaitForSeconds(FrameWait);
                waitTime += FrameWait;
            }

            Assert.AreEqual("MainMenu", SceneManager.GetActiveScene().name,
                "Scene should automatically transition to MainMenu");

            Debug.Log("[Test] Main menu loaded");

            // === PHASE 3: Menu buttons are present and functional ===
            yield return new WaitForSeconds(0.5f);

            var mainMenuController = Object.FindFirstObjectByType<MainMenuController>();
            Assert.IsNotNull(mainMenuController, "MainMenuController should exist in MainMenu scene");

            // Verify new game button exists
            var newGameButton = FindButtonByName("New Game");
            Assert.IsNotNull(newGameButton, "New Game button should exist");
            Assert.IsTrue(newGameButton.gameObject.activeInHierarchy,
                "New Game button should be visible");
            Assert.IsTrue(newGameButton.interactable,
                "New Game button should be interactable");

            Debug.Log("[Test] Menu buttons verified");

            // === PHASE 4: New game button is pressed and character select loads ===
            newGameButton.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            // Wait for scene to load
            waitTime = 0f;
            while (SceneManager.GetActiveScene().name != "CharacterSelectScene" && waitTime < MaxWaitTime)
            {
                yield return new WaitForSeconds(FrameWait);
                waitTime += FrameWait;
            }

            Assert.AreEqual("CharacterSelectScene", SceneManager.GetActiveScene().name,
                "Pressing New Game should load CharacterSelectScene");

            Debug.Log("[Test] Character select scene loaded");

            // === PHASE 5: Character select buttons work ===
            yield return new WaitForSeconds(0.5f);

            var characterSelectController = Object.FindFirstObjectByType<CharacterSelectController>();
            Assert.IsNotNull(characterSelectController,
                "CharacterSelectController should exist in CharacterSelectScene");

            // Find character navigation buttons
            var prevButton = FindButtonByName("Previous");
            var nextButton = FindButtonByName("Next");
            var selectButton = FindButtonByName("Select");

            Assert.IsNotNull(prevButton, "Previous button should exist");
            Assert.IsNotNull(nextButton, "Next button should exist");
            Assert.IsNotNull(selectButton, "Select/Confirm button should exist");

            // Test character navigation
            var initialIndex = characterSelectController.CurrentIndex;
            nextButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);

            Assert.AreNotEqual(initialIndex, characterSelectController.CurrentIndex,
                "Next button should change selected character");

            prevButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(initialIndex, characterSelectController.CurrentIndex,
                "Previous button should return to initial character");

            // Verify select button is clickable
            Assert.IsTrue(selectButton.interactable,
                "Select button should be clickable");

            Debug.Log("[Test] Character select buttons verified");

            // === PHASE 6: Game starts when select is pressed ===
            selectButton.onClick.Invoke();
            yield return new WaitForSeconds(0.5f);

            // Wait for draft scene to load
            waitTime = 0f;
            while (SceneManager.GetActiveScene().name != "DraftScene" && waitTime < MaxWaitTime)
            {
                yield return new WaitForSeconds(FrameWait);
                waitTime += FrameWait;
            }

            Assert.AreEqual("DraftScene", SceneManager.GetActiveScene().name,
                "Confirming character selection should load DraftScene");

            Debug.Log("[Test] Draft scene loaded");

            // === PHASE 7: Verify draft system is ready to show upgrades ===
            yield return new WaitForSeconds(0.5f);

            var draftUI = Object.FindFirstObjectByType<DraftUI>();
            Assert.IsNotNull(draftUI, "DraftUI should exist in DraftScene");
            Assert.IsNotNull(draftUI.DraftButtons, "DraftUI should have draft buttons configured");
            Assert.GreaterOrEqual(draftUI.DraftButtons.Length, 3,
                "DraftUI should have at least 3 button slots configured");

            var draftController = Object.FindFirstObjectByType<DraftController>();
            Assert.IsNotNull(draftController, "DraftController should exist in DraftScene");

            // Verify RunController persisted across scenes (DontDestroyOnLoad)
            var runController = Object.FindFirstObjectByType<RunController>();
            Assert.IsNotNull(runController, "RunController should persist to DraftScene");
            Assert.IsNotNull(runController.Player, "Player should be initialized");
            Assert.IsNotNull(runController.CurrentRun, "CurrentRun should be initialized");

            Debug.Log("[Test] All phases completed successfully! Game flow verified from start to draft scene.");
            Debug.Log(
                $"[Test] Player: {runController.Player.Name}, HP: {runController.Player.Stats.CurrentHP}/{runController.Player.Stats.MaxHP}");
        }

        /// <summary>
        ///     Helper method to find a button by its text or GameObject name
        /// </summary>
        private Button FindButtonByName(string name)
        {
            var allButtons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var button in allButtons)
            {
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