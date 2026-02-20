using UnityEngine;

/// <summary>
/// Applies armor-based damage mitigation during the Mitigation combat phase.
/// Formula: multiplier = 100 / (100 + armor).
/// Registered once per combat via CombatEngine — not tied to any specific unit.
/// Priority 50 — runs before other mitigation effects.
/// </summary>
public class ArmorMitigationModifier : ICombatListener
{
    public int Priority => 50;

    public void RegisterHandlers(CombatContext context)
    {
        context.On<DamagePhaseEvent>(OnMitigation);
    }

    public void UnregisterHandlers(CombatContext context)
    {
        context.Off<DamagePhaseEvent>(OnMitigation);
    }

    private void OnMitigation(DamagePhaseEvent evt)
    {
        if (evt.Phase != CombatPhase.Mitigation) return;

        var armor = evt.Context.Target.Stats.Armor;
        var multiplier = 100f / (100f + armor);

        Log.Info("Armor mitigation applied", new
        {
            target = evt.Context.Target.Name,
            armor,
            multiplier,
            damageBeforeArmor = evt.Context.ModifiedDamage
        });

        evt.Context.ModifiedDamage = Mathf.CeilToInt(evt.Context.ModifiedDamage * multiplier);
    }
}
