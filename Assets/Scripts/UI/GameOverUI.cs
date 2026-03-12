using Controllers;

using Events;

using UI.Stats;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Utils;

namespace UI
{
    /// <summary>
    /// Displays run statistics on the game-over screen and provides a button to return to the main menu.
    /// Reads stats from <see cref="RunStatsTrackerBootstrap"/> which persists across scene loads.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        private const string MainMenuScene = "MainMenu";

        [Header("References")]
        [SerializeField] private StatsPanelUI _statsPanel;
        [SerializeField] private Button _backToMenuButton;

        private void Awake()
        {
            if (this._statsPanel == null)
                Log.Error("GameOverUI: _statsPanel is not assigned.");
            if (this._backToMenuButton == null)
                Log.Error("GameOverUI: _backToMenuButton is not assigned.");
            else
                this._backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        }

        private void Start()
        {
            var stats = RunStatsTrackerBootstrap.Instance?.Stats;

            if (stats != null)
                this._statsPanel.Show(stats.ToViewData());
            else
                Log.Warning("Stats tracker missing.");
        }

        private static void OnBackToMenuClicked()
        {
            GameEvents.ReturnToMainMenu_Event?.Invoke();
            SceneManager.LoadScene(MainMenuScene);
        }
    }
}
