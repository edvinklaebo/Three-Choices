using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        if (_statsPanel == null)
            Log.Error("GameOverUI: _statsPanel is not assigned.");
        if (_backToMenuButton == null)
            Log.Error("GameOverUI: _backToMenuButton is not assigned.");
        else
            _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
    }

    private void Start()
    {
        var bootstrap = FindFirstObjectOfType<RunStatsTrackerBootstrap>();
        if (bootstrap != null)
            _statsPanel?.Show(bootstrap.CurrentStats.ToViewData());
        else
            Log.Warning("GameOverUI: RunStatsTrackerBootstrap not found. Stats will not be displayed.");
    }

    private static void OnBackToMenuClicked()
    {
        SceneManager.LoadScene(MainMenuScene);
    }
}
