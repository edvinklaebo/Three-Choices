using System.Collections.Generic;
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
    ///     Build a deterministic mapping of units to their UI components.
    ///     Call this once after <see cref="Initialize" /> and pass the result to
    ///     <see cref="UIService.SetBindings" /> so all UI lookups use direct references.
    ///     Returns an empty dictionary and logs a warning when either unit is null,
    ///     consistent with the fail-soft pattern used elsewhere in combat setup.
    /// </summary>
    public IReadOnlyDictionary<Unit, UnitUIBinding> BuildBindings(Unit player, Unit enemy)
    {
        if (player == null || enemy == null)
        {
            Log.Warning("CombatView.BuildBindings: called with null unit — bindings not built");
            return new Dictionary<Unit, UnitUIBinding>();
        }

        return new Dictionary<Unit, UnitUIBinding>
        {
            [player] = new(
                _playerView,
                _combatHUD?.GetHealthBar(player),
                _combatHUD?.GetHUDPanel(player)),
            [enemy] = new(
                _enemyView,
                _combatHUD?.GetHealthBar(enemy),
                _combatHUD?.GetHUDPanel(enemy))
        };
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
        if (_canvasGroup)
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
        if (_canvasGroup)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}