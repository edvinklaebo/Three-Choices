using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Floating text component for damage numbers.
/// Spawns at target position, floats up and fades out.
/// Color-coded by damage type.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    [Header("Animation")]
    [SerializeField] private float _floatDuration = 1f;
    [SerializeField] private float _floatDistance = 50f;
    [SerializeField] private AnimationCurve _floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform _rectTransform;
    private Vector3 _startPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_text == null)
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// Display damage number at world position.
    /// </summary>
    public void Show(int amount, DamageType damageType, Vector3 worldPosition, Camera uCamera)
    {
        if (!_text)
            return;

        // Set text and color
        _text.text = amount.ToString();
        _text.color = GetColorForDamageType(damageType);

        // Position in screen space
        var screenPos = uCamera.WorldToScreenPoint(worldPosition);
        _rectTransform.position = screenPos;
        _startPosition = _rectTransform.anchoredPosition;

        // Start animation
        StartCoroutine(AnimateRoutine());
    }

    private IEnumerator AnimateRoutine()
    {
        var elapsed = 0f;

        while (elapsed < _floatDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / _floatDuration;

            // Float up
            var offset = _floatCurve.Evaluate(t) * _floatDistance;
            _rectTransform.anchoredPosition = _startPosition + Vector3.up * offset;

            // Fade out
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f - t;
            }

            yield return null;
        }

        // Return to pool
        FloatingTextPool.Instance?.Return(this);
    }

    /// <summary>
    /// Get color for damage type.
    /// </summary>
    private Color GetColorForDamageType(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Physical => new Color(1f, 0.2f, 0.2f),    // Red
            DamageType.Poison => new Color(0.6f, 0.2f, 0.8f),    // Purple
            DamageType.Bleed => new Color(0.8f, 0f, 0f),         // Dark red
            DamageType.Burn => new Color(1f, 0.5f, 0f),          // Orange
            DamageType.Heal => new Color(0.2f, 1f, 0.2f),        // Green
            DamageType.Arcane => new Color(0.3f, 0.8f, 1f),      // Blue
            _ => Color.white
        };
    }

    /// <summary>
    /// Reset state when returned to pool.
    /// </summary>
    public void Reset()
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 1f;
        }

        StopAllCoroutines();
    }
}

/// <summary>
/// Damage type enum for color coding.
/// </summary>
public enum DamageType
{
    Physical,
    Poison,
    Bleed,
    Burn,
    Heal,
    Arcane
}
