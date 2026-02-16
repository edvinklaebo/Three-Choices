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

        // Enable presentation mode for combat
        EnablePresentationMode();

        Log.Info("CombatHUD initialized", new
        {
            player = player.Name,
            enemy = enemy.Name
        });
    }

    /// <summary>
    /// Enable presentation-driven mode for all health bars.
    /// Health bars will only update from presentation events, not raw state changes.
    /// </summary>
    public void EnablePresentationMode()
    {
        if (_playerHUD != null)
        {
            var healthBar = _playerHUD.GetHealthBar();
            if (healthBar != null)
            {
                healthBar.EnablePresentationMode();
            }
            _playerHUD.EnablePresentationMode();
        }

        if (_enemyHUD != null)
        {
            var healthBar = _enemyHUD.GetHealthBar();
            if (healthBar != null)
            {
                healthBar.EnablePresentationMode();
            }
            _enemyHUD.EnablePresentationMode();
        }

        Log.Info("CombatHUD: Presentation mode enabled");
    }

    /// <summary>
    /// Disable presentation-driven mode for all health bars.
    /// Health bars will respond to raw state changes again.
    /// </summary>
    public void DisablePresentationMode()
    {
        if (_playerHUD != null)
        {
            var healthBar = _playerHUD.GetHealthBar();
            if (healthBar != null)
            {
                healthBar.DisablePresentationMode();
            }
            _playerHUD.DisablePresentationMode();
        }

        if (_enemyHUD != null)
        {
            var healthBar = _enemyHUD.GetHealthBar();
            if (healthBar != null)
            {
                healthBar.DisablePresentationMode();
            }
            _enemyHUD.DisablePresentationMode();
        }

        Log.Info("CombatHUD: Presentation mode disabled");
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

    /// <summary>
    /// Get the HUD panel for a specific unit.
    /// </summary>
    public UnitHUDPanel GetHUDPanel(Unit unit)
    {
        if (unit == null)
            return null;

        // Check player HUD
        if (_playerHUD != null && _playerHUD.GetUnit() == unit)
        {
            return _playerHUD;
        }

        // Check enemy HUD
        if (_enemyHUD != null && _enemyHUD.GetUnit() == unit)
        {
            return _enemyHUD;
        }

        return null;
    }
}
