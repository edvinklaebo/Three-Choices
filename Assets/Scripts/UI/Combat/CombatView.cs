using UnityEngine;

/// <summary>
/// Root combat view orchestrator.
/// Manages player and enemy views, subscribes to combat events.
/// Purely reactive - no game logic.
/// </summary>
public class CombatView : MonoBehaviour
{
    [SerializeField] private UnitView _playerView;
    [SerializeField] private UnitView _enemyView;
    [SerializeField] private CombatHUD _combatHUD;
    [SerializeField] private TurnIndicatorUI _turnIndicator;

    public UnitView PlayerView => _playerView;
    public UnitView EnemyView => _enemyView;

    private void Awake()
    {
        if (_playerView == null)
        {
            Debug.LogError("CombatView: PlayerView not assigned");
        }

        if (_enemyView == null)
        {
            Debug.LogError("CombatView: EnemyView not assigned");
        }

        if (_combatHUD == null)
        {
            Debug.LogError("CombatView: CombatHUD not assigned");
        }
    }

    /// <summary>
    /// Initialize the combat view with player and enemy units.
    /// </summary>
    public void Initialize(Unit player, Unit enemy)
    {
        if (player == null || enemy == null)
        {
            Debug.LogError("CombatView: Cannot initialize with null units");
            return;
        }

        // Initialize unit views
        _playerView.Initialize(player, isPlayer: true);
        _enemyView.Initialize(enemy, isPlayer: false);

        // Initialize HUD
        _combatHUD.Initialize(player, enemy);

        Log.Info("CombatView initialized", new
        {
            player = player.Name,
            enemy = enemy.Name
        });
    }

    /// <summary>
    /// Show turn indicator for active unit.
    /// Called from combat events.
    /// </summary>
    public void ShowTurnIndicator(Unit activeUnit)
    {
        if (_turnIndicator != null)
        {
            _turnIndicator.ShowTurn(activeUnit);
        }
    }

    /// <summary>
    /// Hide turn indicator.
    /// </summary>
    public void HideTurnIndicator()
    {
        if (_turnIndicator != null)
        {
            _turnIndicator.Hide();
        }
    }

    /// <summary>
    /// Get the UnitView for a given Unit.
    /// Used by AnimationService and UIService for unit lookups.
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
}
