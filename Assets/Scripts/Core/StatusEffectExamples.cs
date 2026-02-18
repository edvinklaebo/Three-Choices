/// <summary>
///     Example demonstrating how to use the status effect system.
///     This file shows various ways to apply poison and other patterns for future status effects.
/// </summary>
public static class StatusEffectExamples
{
    /// <summary>
    ///     Example 1: Apply poison from a weapon attack
    /// </summary>
    public static void ApplyPoisonFromWeapon(Unit attacker, Unit target)
    {
        // Weapon applies 2 stacks of poison for 3 turns
        target.ApplyStatus(new Poison(2, 3, 2));
    }

    /// <summary>
    ///     Example 2: Apply poison from a passive (on receiving damage)
    ///     This is implemented in the Poison class
    /// </summary>
    public static void ApplyPoisonFromPassive(Unit defender)
    {
        // When player has Poison passive, attackers get poisoned
        // See Assets/Scripts/Core/Poison.cs
        defender.Passives.Add(new Poison(defender));
        // Now whenever defender is hit, attacker receives 2 stacks for 3 turns
    }

    /// <summary>
    ///     Example 3: Apply poison from an artifact effect (on damage dealt)
    /// </summary>
    public static void ApplyPoisonFromArtifact(Unit attacker, Unit target)
    {
        // Artifact adds 1 stack of poison for 2 turns on hit
        target.ApplyStatus(new Poison(1, 2, 3));
    }

    /// <summary>
    ///     Example 4: Apply strong poison as a skill/ability
    /// </summary>
    public static void ApplyPoisonFromSkill(Unit caster, Unit target)
    {
        // Skill applies 5 stacks of poison for 4 turns instead of direct damage
        target.ApplyStatus(new Poison(5, 4, 4));
    }

    /// <summary>
    ///     Example 5: Check if a unit has poison
    /// </summary>
    public static bool HasPoison(Unit unit)
    {
        return unit.StatusEffects.Exists(e => e.Id == "Poison");
    }

    /// <summary>
    ///     Example 6: Get total poison stacks on a unit
    /// </summary>
    public static int GetPoisonStacks(Unit unit)
    {
        var poison = unit.StatusEffects.Find(e => e.Id == "Poison");
        return poison?.Stacks ?? 0;
    }

    /// <summary>
    ///     Example pattern for future status effects:
    ///     - Burn: Decays stacks each turn (stacks--, damage = stacks)
    ///     - Bleed: Triggers on attack (OnTurnStart checks if unit attacked)
    ///     - Weak: Reduces damage dealt (via IDamageModifier)
    ///     - Vulnerable: Takes more damage (via IDamageModifier)
    ///     - Regeneration: Heals each turn (target.Heal(stacks))
    ///     - Stun: Skip turn (prevent attack in CombatSystem)
    /// </summary>
    public static void FutureStatusEffectPatterns()
    {
        // Implementation examples would go here when these effects are added
    }
}