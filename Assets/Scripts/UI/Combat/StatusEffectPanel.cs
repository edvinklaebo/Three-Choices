using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Status effects panel for a unit.
///     Displays icons for active status effects with stack counts.
///     Updates automatically when unit's status effects change.
///     Handles overflow with "+N" indicator.
/// </summary>
public class StatusEffectPanel : MonoBehaviour
{
    [SerializeField] private StatusEffectIcon _iconPrefab;
    [SerializeField] private Transform _iconContainer;
    [SerializeField] private int _maxVisibleIcons = 6;
    private readonly List<StatusEffectIcon> _activeIcons = new();
    private readonly Stack<StatusEffectIcon> _iconPool = new();
    private int _lastEffectCount = -1;

    private Unit _unit;

    private void Awake()
    {
        if (_iconPrefab == null) Log.Error("StatusEffectPanel: IconPrefab not assigned");

        if (_iconContainer == null) _iconContainer = transform;
    }

    private void Update()
    {
        // Poll for status effect changes
        // In a production environment, this would be event-driven
        if (_unit != null && _unit.StatusEffects != null)
            // Only refresh if the count changed to avoid recreating icons every frame
            if (_unit.StatusEffects.Count != _lastEffectCount)
            {
                RefreshDisplay();
                _lastEffectCount = _unit.StatusEffects.Count;
            }
    }

    /// <summary>
    ///     Initialize the status effect panel with a unit.
    /// </summary>
    public void Initialize(Unit unit)
    {
        if (unit == null)
        {
            Log.Error("StatusEffectPanel: Cannot initialize with null unit");
            return;
        }

        _unit = unit;

        // Initial display
        RefreshDisplay();

        Log.Info("StatusEffectPanel initialized", new
        {
            unit = unit.Name
        });
    }

    /// <summary>
    ///     Refresh the status effect display based on unit's current effects.
    /// </summary>
    private void RefreshDisplay()
    {
        if (_unit == null || _unit.StatusEffects == null)
            return;

        // Return all active icons to pool
        foreach (var icon in _activeIcons) ReturnIconToPool(icon);
        _activeIcons.Clear();

        // Get status effects from unit
        var effects = _unit.StatusEffects;
        var visibleCount = Mathf.Min(effects.Count, _maxVisibleIcons);

        // Display visible effects
        for (var i = 0; i < visibleCount; i++)
        {
            var effect = effects[i];
            var icon = GetIconFromPool();

            icon.SetEffect(effect.Id, effect.Stacks, effect.Duration);
            icon.gameObject.SetActive(true);

            _activeIcons.Add(icon);
        }

        // Show overflow indicator if needed
        var overflow = effects.Count - visibleCount;
        if (overflow > 0)
        {
            var overflowIcon = GetIconFromPool();
            overflowIcon.SetOverflow(overflow);
            overflowIcon.gameObject.SetActive(true);
            _activeIcons.Add(overflowIcon);
        }
    }

    private StatusEffectIcon GetIconFromPool()
    {
        if (_iconPool.Count > 0) return _iconPool.Pop();

        var icon = Instantiate(_iconPrefab, _iconContainer);
        return icon;
    }

    private void ReturnIconToPool(StatusEffectIcon icon)
    {
        if (icon == null)
            return;

        icon.gameObject.SetActive(false);
        _iconPool.Push(icon);
    }
}