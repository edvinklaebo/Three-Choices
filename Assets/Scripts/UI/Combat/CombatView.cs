using System.Collections.Generic;

using Core;

using Systems;

using UnityEngine;

using Utils;

namespace UI.Combat
{
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

        [Header("Projectiles")]
        [SerializeField] private Transform _projectile;

        private CanvasGroup _canvasGroup;

        public UnitView PlayerView => this._playerView;
        public UnitView EnemyView => this._enemyView;
        public Transform Projectile => this._projectile;

        private void Awake()
        {
            if (this._playerView == null) Log.Error("CombatView: PlayerView not assigned");

            if (this._enemyView == null) Log.Error("CombatView: EnemyView not assigned");

            if (this._combatHUD == null) Log.Error("CombatView: CombatHUD not assigned");

            // Get or add CanvasGroup for show/hide functionality
            this._canvasGroup = GetComponent<CanvasGroup>();
            if (this._canvasGroup == null) this._canvasGroup = gameObject.AddComponent<CanvasGroup>();

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
            this._playerView.Initialize(player, isPlayer: true, player.Portrait);
            this._enemyView.Initialize(enemy, isPlayer: false, enemy.Portrait);

            // Initialize HUD
            this._combatHUD.Initialize(player, enemy);

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
            if (this._turnIndicator != null) this._turnIndicator.ShowTurn(activeUnit);
        }

        /// <summary>
        ///     Hide turn indicator.
        ///     NOTE: Currently not called - turn events not wired in combat flow.
        ///     This is a placeholder for future turn-by-turn combat visualization.
        ///     Leave _turnIndicator unassigned (null) in inspector if not implementing turn events.
        /// </summary>
        public void HideTurnIndicator()
        {
            if (this._turnIndicator != null) this._turnIndicator.Hide();
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
                    this._playerView,
                    this._combatHUD?.GetHealthBar(player),
                    this._combatHUD?.GetHUDPanel(player)),
                [enemy] = new(
                    this._enemyView,
                    this._combatHUD?.GetHealthBar(enemy),
                    this._combatHUD?.GetHUDPanel(enemy))
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

            if (this._playerView?.Unit == unit)
                return this._playerView;

            if (this._enemyView?.Unit == unit)
                return this._enemyView;

            return null;
        }

        /// <summary>
        ///     Show the combat view.
        ///     Called when combat starts.
        /// </summary>
        public void Show()
        {
            if (this._canvasGroup)
            {
                this._canvasGroup.alpha = 1f;
                this._canvasGroup.interactable = true;
                this._canvasGroup.blocksRaycasts = true;
            }
        }

        /// <summary>
        ///     Hide the combat view.
        ///     Called when draft UI is shown after battle.
        /// </summary>
        public void Hide()
        {
            if (this._canvasGroup)
            {
                this._canvasGroup.alpha = 0f;
                this._canvasGroup.interactable = false;
                this._canvasGroup.blocksRaycasts = false;
            }
        }
    }
}