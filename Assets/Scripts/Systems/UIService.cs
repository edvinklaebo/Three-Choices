using UnityEngine;

/// <summary>
/// Service for UI presentation during combat.
/// Placeholder implementation for combat animation system.
/// </summary>
public class UIService
{
    public void ShowDamage(Unit target, int amount)
    {
        Log.Info("Showing damage UI", new { target = target.Name, damage = amount });
        // Placeholder: Show floating damage text
    }

    public void ShowHealing(Unit target, int amount)
    {
        Log.Info("Showing healing UI", new { target = target.Name, healing = amount });
        // Placeholder: Show healing effect
    }

    public void ShowStatusEffect(Unit target, string effectName)
    {
        Log.Info("Showing status effect UI", new { target = target.Name, effect = effectName });
        // Placeholder: Show status icon
    }
}
