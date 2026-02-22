using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Reusable health bar UI component that displays a Unit's current HP as a normalized slider.
///     Features:
///     - Displays health as a normalized value (0-1) using Unity's Slider component
///     - Smooth lerp animation for health transitions
///     - Works with any Unit (player, enemy, etc.)
///     Usage:
///     1. Attach to a GameObject with a Slider component
///     2. Assign the Slider in the inspector (or it will auto-find on the same GameObject)
///     3. Call Initialize(unit) with the Unit to track
///     4. Health bar animates only when AnimateToCurrentHealth() or AnimateToHealth() is called
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _lerpSpeed = 5f;

    public bool IsInitialized;
    private bool _sliderConfigured;
    private float _targetValue;

    private Unit _unit;

    private void Awake()
    {
        EnsureSliderConfigured();
    }

    private void Update()
    {
        if (_slider == null)
            return;

        if (Mathf.Abs(_slider.value - _targetValue) > 0.001f)
            _slider.value = Mathf.Lerp(_slider.value, _targetValue, Time.deltaTime * _lerpSpeed);
    }

    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Log.Error("HealthBarUI: Cannot initialize with null unit");
            return;
        }

        // Ensure slider is configured (in case Initialize is called before Awake)
        EnsureSliderConfigured();

        _unit = unit;
        _targetValue = GetNormalizedHealth();

        // Set slider value immediately to avoid animating from 0 on first initialization
        if (_slider != null) _slider.value = _targetValue;

        IsInitialized = true;
    }

    private void EnsureSliderConfigured()
    {
        if (_sliderConfigured)
            return;

        // Auto-find slider if not assigned
        if (_slider == null) _slider = GetComponent<Slider>();

        // Configure slider for health bar use
        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.interactable = false;
            _sliderConfigured = true;
        }
    }

    /// <summary>
    ///     Animates the health bar to the unit's current health value.
    ///     This should be called from combat presentation events (DamageAction, HealAction, etc.)
    ///     to ensure the health bar animates in sync with visual feedback.
    /// </summary>
    public void AnimateToCurrentHealth()
    {
        if (_unit == null)
            return;

        _targetValue = GetNormalizedHealth();
    }

    /// <summary>
    ///     Animates the health bar from a specific value to another specific value.
    ///     This allows proper animation even when the unit's state has already changed.
    ///     Used when combat logic pre-calculates all state changes before presentation.
    /// </summary>
    /// <param name="fromNormalized">Starting health value (0-1)</param>
    /// <param name="toNormalized">Target health value (0-1)</param>
    public void AnimateToHealth(float fromNormalized, float toNormalized)
    {
        if (_slider == null)
            return;

        // Set the slider to the starting value immediately (no lerp)
        _slider.value = Mathf.Clamp01(fromNormalized);

        // Set target to the ending value (will lerp in Update)
        _targetValue = Mathf.Clamp01(toNormalized);

        Log.Info("HealthBarUI: Animating health", new
        {
            unit = _unit?.Name ?? "unknown",
            from = fromNormalized,
            to = toNormalized
        });
    }

    private float GetNormalizedHealth()
    {
        if (_unit == null || _unit.Stats.MaxHP <= 0)
            return 0f;

        return Mathf.Clamp01((float)_unit.Stats.CurrentHP / _unit.Stats.MaxHP);
    }
}