using TMPro;
using UnityEngine;

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
        if (_healthBar == null)
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

        _unit = unit;

        // Set unit name
        if (_nameText != null)
        {
            _nameText.text = unit.Name;
        }

        // Initialize health bar
        if (_healthBar != null)
        {
            _healthBar.Bind(unit);
        }

        // Initialize status effect panel
        if (_statusEffectPanel != null)
        {
            _statusEffectPanel.Initialize(unit);
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
        return _healthBar;
    }

    /// <summary>
    /// Get the unit this panel is tracking.
    /// </summary>
    public Unit GetUnit()
    {
        return _unit;
    }


    /// <summary>
    /// Update HP text with explicit values for presentation-driven display.
    /// Clamps currentHP to 0 to avoid showing negative numbers.
    /// </summary>
    public void UpdateHealthText(int currentHP, int maxHP)
    {
        if (_hpText)
        {
            // Clamp currentHP to 0 minimum to avoid showing negative numbers
            var displayHP = Mathf.Max(0, currentHP);
            _hpText.text = $"{displayHP} / {maxHP}";
        }
    }


    private void UpdateHealthTextFromUnit()
    {
        if (_hpText != null && _unit != null)
        {
            UpdateHealthText(_unit.Stats.CurrentHP, _unit.Stats.MaxHP);
        }
    }
}
