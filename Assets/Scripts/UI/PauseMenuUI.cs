using Events;

using Systems;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    /// <summary>
    /// Handles pause menu UI display and user interactions.
    /// Separated from game logic - only handles UI concerns.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";

        [SerializeField] private GameObject _pauseMenuPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _quitButton;

        private void OnEnable()
        {
            PauseManager.OnPauseStateChanged += HandlePauseStateChanged;
        }

        private void OnDisable()
        {
            PauseManager.OnPauseStateChanged -= HandlePauseStateChanged;
        }

        private void Start()
        {
            // Ensure panels are hidden at start
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_quitButton != null)
                _quitButton.SetActive(PlatformUtils.IsQuitSupported());
        }

        private void HandlePauseStateChanged(bool isPaused)
        {
            if (_pauseMenuPanel != null)
            {
                _pauseMenuPanel.SetActive(isPaused);
            }

            // Hide settings panel when unpausing
            if (!isPaused && _settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }
        }

        public static void OnMainMenuClicked()
        {
            PauseManager.Resume();
            GameEvents.ReturnToMainMenu_Event?.Invoke();
            SceneManager.LoadScene(MainMenuScene);
        }

        public static void OnResumeClicked()
        {
            PauseManager.Resume();
        }

        public void OnSettingsClicked()
        {
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(true);
        }

        public void OnBackFromSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(true);
        }

        public static void OnQuitClicked()
        {
            PauseManager.Resume();
            Application.Quit();
        }

        /// <summary>
        /// Initialize the pause menu UI with references.
        /// Allows programmatic setup without reflection.
        /// </summary>
        public void Initialize(GameObject pauseMenuPanel, GameObject settingsPanel, GameObject quitButton = null)
        {
            _pauseMenuPanel = pauseMenuPanel;
            _settingsPanel = settingsPanel;
            _quitButton = quitButton;
        }
    }
}
