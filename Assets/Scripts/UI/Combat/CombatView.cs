using UnityEngine;

/// <summary>
///     Root combat view orchestrator.
///     Manages player and enemy views, subscribes to combat events.
///     Purely reactive - no game logic.
/// </summary>
public class CombatView : MonoBehaviour
{
    [SerializeField] private UnitView _playerView;
    [SerializeField] private UnitView _enemyView;
    [SerializeField] private CombatHUD _combatHUD;
    [SerializeField] private TurnIndicatorUI _turnIndicator;
    [SerializeField] private FloatingTextPool _floatingTextPool;

    private CanvasGroup _canvasGroup;

    public UnitView PlayerView => _playerView;
    public UnitView EnemyView => _enemyView;
    public CombatHUD CombatHUD => _combatHUD;

    private void Awake()
    {
        if (_playerView == null) Log.Error("CombatView: PlayerView not assigned");

        if (_enemyView == null) Log.Error("CombatView: EnemyView not assigned");

        if (_combatHUD == null) Log.Error("CombatView: CombatHUD not assigned");

        // Get or add CanvasGroup for show/hide functionality
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start hidden
        Hide();
    }

    /// <summary>
    ///     Initialize the combat view with player and enemy units.
    /// </summary>
    public void Initialize(Unit player, Unit enemy)
    {
        if (player == null || enemy == null)
        {
            Log.Error("CombatView: Cannot initialize with null units");
            return;
        }

        // Initialize unit views
        _playerView.Initialize(player, isPlayer: true);
        _enemyView.Initialize(enemy, isPlayer: false);

        // Initialize HUD
        _combatHUD.Initialize(player, enemy);

        // Show combat view when initialized
        Show();

        Log.Info("CombatView initialized", new
        {
            player = player.Name,
            enemy = enemy.Name
        });
    }

    /// <summary>
    ///     Show turn indicator for active unit.
    ///     NOTE: Currently not called - turn events not wired in combat flow.
    ///     This is a placeholder for future turn-by-turn combat visualization.
    ///     Leave _turnIndicator unassigned (null) in inspector if not implementing turn events.
    /// </summary>
    public void ShowTurnIndicator(Unit activeUnit)
    {
        if (_turnIndicator != null) _turnIndicator.ShowTurn(activeUnit);
    }

    /// <summary>
    ///     Hide turn indicator.
    ///     NOTE: Currently not called - turn events not wired in combat flow.
    ///     This is a placeholder for future turn-by-turn combat visualization.
    ///     Leave _turnIndicator unassigned (null) in inspector if not implementing turn events.
    /// </summary>
    public void HideTurnIndicator()
    {
        if (_turnIndicator != null) _turnIndicator.Hide();
    }

    /// <summary>
    ///     Get the UnitView for a given Unit.
    ///     Used by AnimationService and UIService for unit lookups.
    /// </summary>
    public UnitView GetUnitView(Unit unit)
    {
        if (unit == null)
            return null;

        if (_playerView?.Unit == unit)
            return _playerView;

        if (_enemyView?.Unit == unit)
            return _enemyView;

        return null;
    }

    /// <summary>
    ///     Show the combat view.
    ///     Called when combat starts.
    /// </summary>
    public void Show()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }

    /// <summary>
    ///     Hide the combat view.
    ///     Called when draft UI is shown after battle.
    /// </summary>
    public void Hide()
    {
        // Disable presentation mode when combat ends
        if (_combatHUD != null) _combatHUD.DisablePresentationMode();

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}