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
            Debug.LogError("UnitHUDPanel: HealthBar not assigned");
        }
    }

    /// <summary>
    /// Initialize the HUD panel with a unit.
    /// </summary>
    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("UnitHUDPanel: Cannot initialize with null unit");
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
            _healthBar.Initialize(unit);
        }

        // Initialize status effect panel
        if (_statusEffectPanel != null)
        {
            _statusEffectPanel.Initialize(unit);
        }

        // Subscribe to health changes for numeric display
        _unit.HealthChanged += OnHealthChanged;
        UpdateHealthText();

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

    private void OnDisable()
    {
        if (_unit != null)
        {
            _unit.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (_hpText != null && _unit != null)
        {
            _hpText.text = $"{_unit.Stats.CurrentHP} / {_unit.Stats.MaxHP}";
        }
    }
}
