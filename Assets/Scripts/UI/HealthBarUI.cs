using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reusable health bar UI component that displays a Unit's current HP as a normalized slider.
/// 
/// Features:
/// - Displays health as a normalized value (0-1) using Unity's Slider component
/// - Animates in response to combat presentation events (damage/heal actions)
/// - Smooth lerp animation for health transitions
/// - Works with any Unit (player, enemy, etc.)
/// 
/// Usage:
/// 1. Attach to a GameObject with a Slider component
/// 2. Assign the Slider in the inspector (or it will auto-find on the same GameObject)
/// 3. Call Initialize(unit) with the Unit to track
/// 4. Health bar will animate when AnimateToCurrentHealth() is called during combat presentation
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _lerpSpeed = 5f;

    private Unit _unit;
    private float _targetValue;
    private bool _sliderConfigured;
    private bool _presentationMode;
    
    public bool IsInitialized;
    
    private void Awake()
    {
        EnsureSliderConfigured();
    }

    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Debug.LogError("HealthBarUI: Cannot initialize with null unit");
            return;
        }

        // Ensure slider is configured (in case Initialize is called before Awake)
        EnsureSliderConfigured();

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

        // Subscribe to health changes as a fallback for non-combat scenarios
        // During combat, AnimateToCurrentHealth() should be called from presentation events
        _unit.HealthChanged += OnHealthChanged;
        IsInitialized = true;
    }

    private void EnsureSliderConfigured()
    {
        if (_sliderConfigured)
            return;

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
            _sliderConfigured = true;
        }
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

    /// <summary>
    /// Animates the health bar to the unit's current health value.
    /// This should be called from combat presentation events (DamageAction, HealAction, etc.)
    /// to ensure the health bar animates in sync with visual feedback.
    /// </summary>
    public void AnimateToCurrentHealth()
    {
        if (_unit == null)
            return;

        _targetValue = GetNormalizedHealth();
    }

    /// <summary>
    /// Animates the health bar from a specific value to another specific value.
    /// This allows proper animation even when the unit's state has already changed.
    /// Used when combat logic pre-calculates all state changes before presentation.
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

    /// <summary>
    /// Enable presentation-driven mode where health bar only updates from AnimateToCurrentHealth() calls.
    /// This prevents the health bar from updating in response to raw state changes.
    /// Call this when entering combat to ensure animations sync with presentation events.
    /// </summary>
    public void EnablePresentationMode()
    {
        _presentationMode = true;
    }

    /// <summary>
    /// Disable presentation-driven mode, allowing health bar to respond to state changes again.
    /// Call this when exiting combat.
    /// </summary>
    public void DisablePresentationMode()
    {
        _presentationMode = false;
    }

    private void OnHealthChanged(Unit unit, int currentHP, int maxHP)
    {
        // If in presentation mode, ignore state change events
        // Health bar will only update via AnimateToCurrentHealth() calls
        if (_presentationMode)
            return;

        // Fallback for non-combat health changes
        _targetValue = maxHP <= 0 ? 0f : Mathf.Clamp01((float)currentHP / maxHP);
    }

    private float GetNormalizedHealth()
    {
        if (_unit == null || _unit.Stats.MaxHP <= 0)
            return 0f;

        return Mathf.Clamp01((float)_unit.Stats.CurrentHP / _unit.Stats.MaxHP);
    }
}
