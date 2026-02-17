using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Pipeline for applying damage modifiers in priority order.
/// Supports both global modifiers and context-specific modifiers.
/// </summary>
public static class DamagePipeline
{
    private static readonly List<IDamageModifier> _globalModifiers = new();

    public static void Register(IDamageModifier mod)
    {
        _globalModifiers.Add(mod);
    }

    public static void Unregister(IDamageModifier mod)
    {
        _globalModifiers.Remove(mod);
    }

    /// <summary>
    /// Process damage context through all registered modifiers in priority order.
    /// </summary>
    public static void Process(DamageContext ctx)
    {
        // Collect all applicable modifiers from global registration and unit passives
        var allModifiers = new List<IDamageModifier>(_globalModifiers.Count + 10);
        
        // Add globally registered modifiers
        allModifiers.AddRange(_globalModifiers);

        // Add unit-specific modifiers (from passives) - only those not already globally registered
        if (ctx.Source != null)
        {
            foreach (var passive in ctx.Source.Passives)
            {
                if (passive is IDamageModifier modifier && !_globalModifiers.Contains(modifier))
                {
                    allModifiers.Add(modifier);
                }
            }
        }

        // Sort once by priority and apply
        allModifiers.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
        foreach (var mod in allModifiers)
            mod.Modify(ctx);

        // Ensure non-negative damage
        if (ctx.FinalValue < 0)
            ctx.FinalValue = 0;
    }

    /// <summary>
    /// Clear all global modifiers. Useful for testing or run resets.
    /// </summary>
    public static void Clear()
    {
        _globalModifiers.Clear();
    }
}