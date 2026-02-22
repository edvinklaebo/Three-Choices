using System;
using System.Collections;
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
[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private float _animationDuration = 0.25f;

    private bool _sliderConfigured;

    private Unit _unit;
    private Coroutine _animation;

    public void Awake()
    {
        if (_slider == null)
            _slider = GetComponent<Slider>();

        if (_slider == null)
            throw new InvalidOperationException("HealthBarUI requires a Slider.");
        
        _slider.interactable = false;
    }

    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Log.Error("HealthBarUI: Cannot initialize with null unit");
            return;
        }

        _unit = unit;
    }

    /// <summary>
    ///     Animates the health bar from a specific HP value to another HP value.
    ///     This allows proper animation even when the units state has already changed.
    ///     Used when combat logic pre-calculates all state changes before presentation.
    /// </summary>
    /// <param name="hpBefore">Starting HP value</param>
    /// <param name="hpAfter">Target HP value</param>
    public void AnimateToHealth(int hpBefore, int hpAfter)
    {
        if (!_slider || _unit == null)
            return;

        var maxHP = _unit.Stats.MaxHP;
        if (maxHP <= 0)
            return;

        var from = Mathf.Clamp01((float)hpBefore / maxHP);
        var to = Mathf.Clamp01((float)hpAfter / maxHP);

        if (_animation != null)
            StopCoroutine(_animation);

        _animation = StartCoroutine(AnimateRoutine(from, to));
    }

    private IEnumerator AnimateRoutine(float from, float to)
    {
        var elapsed = 0f;

        _slider.value = from;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / _animationDuration;
            _slider.value = Mathf.Lerp(from, to, t);
            yield return null;
        }

        _slider.value = to;
    }
}