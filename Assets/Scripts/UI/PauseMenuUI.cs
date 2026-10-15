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

        [SerializeField] private GameObject _pauseMenuPanel = null;
        [SerializeField] private GameObject _settingsPanel = null;
        [SerializeField] private GameObject _quitButton = null;
        [SerializeField] private PlayerOverviewUI _overview = null;

        [System.NonSerialized] private System.Func<string> _getOverviewText;

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
            if (_overview != null)
                _overview.gameObject.SetActive(false);
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
            if (_overview != null)
                _overview.gameObject.SetActive(false);
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

        public void InitializeOverview(PlayerOverviewUI overview, System.Func<string> getOverviewText)
        {
            _overview = overview ?? throw new System.ArgumentNullException(nameof(overview));
            _getOverviewText = getOverviewText ?? throw new System.ArgumentNullException(nameof(getOverviewText));
            _overview.gameObject.SetActive(false);
        }

        public void OnObtainedItemsClicked()
        {
            if (_overview == null || _getOverviewText == null)
                return;

            _overview.SetText(_getOverviewText());
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(false);
            if (_settingsPanel != null)
                _settingsPanel.SetActive(false);
            _overview.gameObject.SetActive(true);
        }

        public void OnBackFromOverview()
        {
            if (_overview != null)
                _overview.gameObject.SetActive(false);
            if (_pauseMenuPanel != null)
                _pauseMenuPanel.SetActive(PauseManager.IsPaused);
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
