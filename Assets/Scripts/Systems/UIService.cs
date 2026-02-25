using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Service for UI presentation during combat.
///     Handles damage numbers, healing effects, and status indicators.
/// </summary>
public class UIService
{
    private IReadOnlyDictionary<Unit, UnitUIBinding> _bindings = new Dictionary<Unit, UnitUIBinding>();

    /// <summary>
    ///     Provide the fully built unit → UI binding map.
    ///     Must be called once after <see cref="CombatView.BuildBindings" /> so all
    ///     lookups use direct references instead of repeated component searches.
    /// </summary>
    public void SetBindings(IReadOnlyDictionary<Unit, UnitUIBinding> bindings)
    {
        var newBindings = bindings ?? new Dictionary<Unit, UnitUIBinding>();

        // Collect health bar instances present in the new bindings so we do not
        // unbind health bars that are being reused across fights on the same CombatView.
        var reusedHealthBars = new HashSet<HealthBarUI>();
        foreach (var b in newBindings.Values)
            if (b.HealthBar != null)
                reusedHealthBars.Add(b.HealthBar);

        foreach (var binding in _bindings.Values)
            if (binding.HealthBar != null && !reusedHealthBars.Contains(binding.HealthBar))
                binding.HealthBar.Unbind();

        _bindings = newBindings;
    }

    /// <summary>
    ///     Show damage with explicit HP values for proper health bar animation.
    ///     This overload is used by DamageAction to animate from old HP to new HP.
    /// </summary>
    public void ShowDamage(Unit target, int amount, int hpBefore, int hpAfter, int maxHP,
        DamageType damageType = DamageType.Physical)
    {
        Log.Info("Showing damage UI with HP values", new
        {
            target = target.Name,
            damage = amount,
            type = damageType,
            hpBefore,
            hpAfter,
            maxHP
        });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.Spawn(amount, damageType, worldPosition.Value);

        // Animate health bar with explicit HP values
        AnimateHealthBarToValue(target, hpBefore, hpAfter);

        // Update HP text with explicit values
        UpdateHealthText(target, hpAfter, maxHP);
    }

    public void ShowHealing(Unit target, int amount, int hpBefore, int hpAfter)
    {
        Log.Info("Showing healing UI", new { target = target.Name, healing = amount });

        var worldPosition = GetUnitWorldPosition(target);
        if (worldPosition.HasValue && FloatingTextPool.Instance != null)
            FloatingTextPool.Instance.Spawn(amount, DamageType.Heal, worldPosition.Value);

        // Animate health bar to current health value
        AnimateHealthBarToValue(target, hpBefore, hpAfter);
    }

    public void ShowStatusEffect(Unit target, string effectName)
    {
        Log.Info("Showing status effect UI", new { target = target.Name, effect = effectName });
        // Status effects are displayed via StatusEffectPanel
        // This method is kept for compatibility with existing ICombatAction implementations
    }

    /// <summary>
    ///     Animates the health bar for a unit from a specific old HP to a specific new HP.
    ///     This allows proper animation even when the unit's state has already been modified.
    /// </summary>
    public void AnimateHealthBarToValue(Unit target, int hpBefore, int hpAfter)
    {
        if (target == null)
            return;

        var healthBar = GetHealthBar(target);
        if (healthBar != null)
            healthBar.AnimateToHealth(hpBefore, hpAfter);
    }

    /// <summary>
    ///     Updates the HP text for a unit with explicit values.
    ///     This ensures HP text is synchronized with presentation events, not raw state changes.
    /// </summary>
    public void UpdateHealthText(Unit target, int currentHP, int maxHP)
    {
        if (target == null)
            return;

        var hudPanel = GetHUDPanel(target);
        if (hudPanel != null) hudPanel.UpdateHealthText(currentHP, maxHP);
    }

    /// <summary>
    ///     Get the world position of a unit for UI spawn location.
    /// </summary>
    private Vector3? GetUnitWorldPosition(Unit target)
    {
        var unitView = GetUnitView(target);
        if (unitView != null)
            // Spawn above unit's center
            return unitView.transform.position + Vector3.up * 0.5f;

        return null;
    }

    private UnitView GetUnitView(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.UnitView : null;
    }

    private HealthBarUI GetHealthBar(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.HealthBar : null;
    }

    private UnitHUDPanel GetHUDPanel(Unit target)
    {
        return TryGetBinding(target, out var b) ? b.HUDPanel : null;
    }

    private bool TryGetBinding(Unit target, out UnitUIBinding binding)
    {
        binding = null;
        return target != null && _bindings.TryGetValue(target, out binding);
    }
}