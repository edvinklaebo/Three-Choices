using System.Collections.Generic;

/// <summary>
/// Manages persistent modifiers that carry across runs (meta-progression).
/// Modifiers can be unlocked, upgraded, and applied globally.
/// </summary>
public static class MetaProgressionSystem
{
    private static readonly Dictionary<string, IDamageModifier> _unlockedModifiers = new();
    private static readonly List<IDamageModifier> _activeGlobalModifiers = new();

    /// <summary>
    /// Unlock a persistent modifier that can be applied to future runs.
    /// </summary>
    public static void UnlockModifier(string id, IDamageModifier modifier)
    {
        if (!_unlockedModifiers.ContainsKey(id))
        {
            _unlockedModifiers.Add(id, modifier);

            Log.Info("Meta-progression modifier unlocked", new
            {
                modifierId = id,
                priority = modifier.Priority
            });
        }
    }

    /// <summary>
    /// Check if a modifier has been unlocked.
    /// </summary>
    public static bool IsUnlocked(string id)
    {
        return _unlockedModifiers.ContainsKey(id);
    }

    /// <summary>
    /// Apply an unlocked modifier globally for the current run.
    /// </summary>
    public static void ActivateModifier(string id)
    {
        if (_unlockedModifiers.TryGetValue(id, out var modifier))
        {
            DamagePipeline.Register(modifier);
            _activeGlobalModifiers.Add(modifier);

            Log.Info("Meta-progression modifier activated", new
            {
                modifierId = id,
                priority = modifier.Priority
            });
        }
    }

    /// <summary>
    /// Deactivate all global modifiers (typically at run end).
    /// </summary>
    public static void DeactivateAll()
    {
        foreach (var modifier in _activeGlobalModifiers)
            DamagePipeline.Unregister(modifier);

        _activeGlobalModifiers.Clear();
    }

    /// <summary>
    /// Get all unlocked modifier IDs.
    /// </summary>
    public static IEnumerable<string> GetUnlockedModifiers()
    {
        return _unlockedModifiers.Keys;
    }

    /// <summary>
    /// Clear all unlocked modifiers (for testing or reset).
    /// </summary>
    public static void Reset()
    {
        DeactivateAll();
        _unlockedModifiers.Clear();
    }
}
