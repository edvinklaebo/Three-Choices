using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable health bar UI component that displays a Unit's current HP as a normalized fill bar.
/// 
/// Features:
/// - Displays health as a normalized value (0-1) using Image.fillAmount
/// - Listens to Unit's HealthChanged event for automatic updates
/// - Smooth lerp animation for health transitions
/// - Works with any Unit (player, enemy, etc.)
/// 
/// Usage:
/// 1. Attach to a GameObject with a child Image component (set fillType to Filled)
/// 2. Assign the fill Image in the inspector
/// 3. Call Initialize(unit) with the Unit to track
/// 4. Health bar will automatically update when unit health changes
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image _fillImage;
    [SerializeField] private float _lerpSpeed = 5f;

    private Unit _unit;
    private float _targetFillAmount;
    private float _currentFillAmount;

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
        
        Log.Warning("HealthBarUI: Initializing HealthBarUI");
        
        _unit = unit;
        _targetFillAmount = GetNormalizedHealth();
        _currentFillAmount = _targetFillAmount;
        
        if (_fillImage != null)
        {
            _fillImage.fillAmount = _currentFillAmount;
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
        if (_fillImage == null)
            return;

        if (Mathf.Abs(_currentFillAmount - _targetFillAmount) > 0.001f)
        {
            _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _lerpSpeed);
            _fillImage.fillAmount = _currentFillAmount;
        }
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        _targetFillAmount = maxHP <= 0 ? 0f : Mathf.Clamp01((float)currentHP / maxHP);
    }

    private float GetNormalizedHealth()
    {
        if (_unit == null || _unit.Stats.MaxHP <= 0)
            return 0f;

        return Mathf.Clamp01((float)_unit.Stats.CurrentHP / _unit.Stats.MaxHP);
    }
}
