using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable health bar UI component that displays a Unit's current HP as a normalized slider.
/// 
/// Features:
/// - Displays health as a normalized value (0-1) using Unity's Slider component
/// - Listens to Unit's HealthChanged event for automatic updates
/// - Smooth lerp animation for health transitions
/// - Works with any Unit (player, enemy, etc.)
/// 
/// Usage:
/// 1. Attach to a GameObject with a Slider component
/// 2. Assign the Slider in the inspector (or it will auto-find on the same GameObject)
/// 3. Call Initialize(unit) with the Unit to track
/// 4. Health bar will automatically update when unit health changes
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _lerpSpeed = 5f;

    private Unit _unit;
    private float _targetValue;

    private void Awake()
    {
        // Auto-find slider if not assigned
        if (_slider == null)
        {
            _slider = GetComponent<Slider>();
        }

        // Configure slider for health bar use
        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.interactable = false;
        }
    }

    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("HealthBarUI: Cannot initialize with null unit");
            return;
        }

        // Unsubscribe from previous unit if any
        if (_unit != null)
        {
            _unit.HealthChanged -= OnHealthChanged;
        }

        _unit = unit;
        _targetValue = GetNormalizedHealth();
        
        // Set slider value immediately to avoid animating from 0 on first initialization
        if (_slider != null)
        {
            _slider.value = _targetValue;
        }

        // Subscribe to new unit's health changes
        _unit.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (_unit != null)
        {
            _unit.HealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (_slider == null)
            return;

        if (Mathf.Abs(_slider.value - _targetValue) > 0.001f)
        {
            _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * _lerpSpeed);
        }
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        _targetValue = maxHP <= 0 ? 0f : Mathf.Clamp01((float)currentHP / maxHP);
    }

    private float GetNormalizedHealth()
    {
        if (_unit == null || _unit.Stats.MaxHP <= 0)
            return 0f;

        return Mathf.Clamp01((float)_unit.Stats.CurrentHP / _unit.Stats.MaxHP);
    }
}
