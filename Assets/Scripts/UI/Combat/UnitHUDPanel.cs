using Core;

using TMPro;

using UnityEngine;

using Utils;

namespace UI.Combat
{
    /// <summary>
    /// Individual HUD panel for a unit.
    /// Displays name, health bar, and status effects.
    /// Subscribes to unit events for automatic updates.
    /// </summary>
    public class UnitHUDPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private HealthBarUI _healthBar;
        [SerializeField] private StatusEffectPanel _statusEffectPanel;
        [SerializeField] private TextMeshProUGUI _hpText;

        private Unit _unit;

        private void Awake()
        {
            if (this._healthBar == null)
            {
                Log.Error("UnitHUDPanel: HealthBar not assigned");
            }
        }

        /// <summary>
        /// Initialize the HUD panel with a unit.
        /// </summary>
        public void Initialize(Unit unit)
        {
            if (unit == null)
            {
                Log.Error("UnitHUDPanel: Cannot initialize with null unit");
                return;
            }

            this._unit = unit;

            // Set unit name
            if (this._nameText != null)
            {
                this._nameText.text = unit.Name;
            }

            // Initialize health bar
            if (this._healthBar != null)
            {
                this._healthBar.Bind(unit);
            }

            // Initialize status effect panel
            if (this._statusEffectPanel != null)
            {
                this._statusEffectPanel.Initialize(unit);
            }

            UpdateHealthTextFromUnit();

            Log.Info("UnitHUDPanel initialized", new
            {
                unit = unit.Name
            });
        }

        /// <summary>
        /// Get the health bar component for this HUD panel.
        /// </summary>
        public HealthBarUI GetHealthBar()
        {
            return this._healthBar;
        }

        /// <summary>
        /// Get the unit this panel is tracking.
        /// </summary>
        public Unit GetUnit()
        {
            return this._unit;
        }


        /// <summary>
        /// Update HP text with explicit values for presentation-driven display.
        /// Clamps currentHP to 0 to avoid showing negative numbers.
        /// </summary>
        public void UpdateHealthText(int currentHP, int maxHP)
        {
            if (this._hpText)
            {
                // Clamp currentHP to 0 minimum to avoid showing negative numbers
                var displayHP = Mathf.Max(0, currentHP);
                this._hpText.text = $"{displayHP} / {maxHP}";
            }
        }


        private void UpdateHealthTextFromUnit()
        {
            if (this._hpText != null && this._unit != null)
            {
                UpdateHealthText(this._unit.Stats.CurrentHP, this._unit.Stats.MaxHP);
            }
        }
    }
}
