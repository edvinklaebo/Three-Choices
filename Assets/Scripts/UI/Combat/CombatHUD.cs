using UnityEngine;

/// <summary>
/// Container for player and enemy HUD panels.
/// Manages health bars, names, and status effects display.
/// </summary>
public class CombatHUD : MonoBehaviour
{
    [SerializeField] private UnitHUDPanel _playerHUD;
    [SerializeField] private UnitHUDPanel _enemyHUD;

    private void Awake()
    {
        if (_playerHUD == null)
        {
            Debug.LogError("CombatHUD: PlayerHUD not assigned");
        }

        if (_enemyHUD == null)
        {
            Debug.LogError("CombatHUD: EnemyHUD not assigned");
        }
    }

    /// <summary>
    /// Initialize both HUD panels with their respective units.
    /// </summary>
    public void Initialize(Unit player, Unit enemy)
    {
        if (player == null || enemy == null)
        {
            Debug.LogError("CombatHUD: Cannot initialize with null units");
            return;
        }

        if (_playerHUD != null)
        {
            _playerHUD.Initialize(player);
        }

        if (_enemyHUD != null)
        {
            _enemyHUD.Initialize(enemy);
        }

        Log.Info("CombatHUD initialized", new
        {
            player = player.Name,
            enemy = enemy.Name
        });
    }

    /// <summary>
    /// Get the health bar for a specific unit.
    /// </summary>
    public HealthBarUI GetHealthBar(Unit unit)
    {
        if (unit == null)
            return null;

        // Check player HUD
        if (_playerHUD != null && _playerHUD.GetUnit() == unit)
        {
            return _playerHUD.GetHealthBar();
        }

        // Check enemy HUD
        if (_enemyHUD != null && _enemyHUD.GetUnit() == unit)
        {
            return _enemyHUD.GetHealthBar();
        }

        return null;
    }
}
