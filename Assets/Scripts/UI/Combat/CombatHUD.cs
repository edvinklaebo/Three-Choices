using Core;

using UnityEngine;

using Utils;

namespace UI.Combat
{
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
            if (this._playerHUD == null)
            {
                Log.Error("CombatHUD: PlayerHUD not assigned");
            }

            if (this._enemyHUD == null)
            {
                Log.Error("CombatHUD: EnemyHUD not assigned");
            }
        }

        /// <summary>
        /// Initialize both HUD panels with their respective units.
        /// </summary>
        public void Initialize(Unit player, Unit enemy)
        {
            if (player == null || enemy == null)
            {
                Log.Error("CombatHUD: Cannot initialize with null units");
                return;
            }

            if (this._playerHUD != null)
            {
                this._playerHUD.Initialize(player);
            }

            if (this._enemyHUD != null)
            {
                this._enemyHUD.Initialize(enemy);
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
            if (this._playerHUD != null && this._playerHUD.GetUnit() == unit)
            {
                return this._playerHUD.GetHealthBar();
            }

            // Check enemy HUD
            if (this._enemyHUD != null && this._enemyHUD.GetUnit() == unit)
            {
                return this._enemyHUD.GetHealthBar();
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
            if (this._playerHUD != null && this._playerHUD.GetUnit() == unit)
            {
                return this._playerHUD;
            }

            // Check enemy HUD
            if (this._enemyHUD != null && this._enemyHUD.GetUnit() == unit)
            {
                return this._enemyHUD;
            }

            return null;
        }
    }
}
