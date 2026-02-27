using System;

public static class ArtifactApplier
{
    /// <summary>
    /// Applies the artifact's effect permanently to the player unit.
    /// Effects persist until game reset (run state cleared).
    /// </summary>
    public static void ApplyToPlayer(ArtifactDefinition artifact, Unit player)
    {
        if (artifact == null) throw new ArgumentNullException(nameof(artifact));
        if (player == null) throw new ArgumentNullException(nameof(player));

        Log.Info($"[ArtifactApplier] Applying artifact '{artifact.DisplayName}' to {player.Name}");

        switch (artifact.EffectType)
        {
            case ArtifactEffectType.StatBoost:
                ApplyStat(artifact, player);
                break;
            case ArtifactEffectType.AddPassive:
                ApplyPassive(artifact, player);
                break;
            case ArtifactEffectType.AddAbility:
                ApplyAbility(artifact, player);
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.EffectType.ToString());
        }
    }

    private static void ApplyStat(ArtifactDefinition artifact, Unit unit)
    {
        switch (artifact.Stat)
        {
            case StatType.MaxHP:
                unit.Stats.MaxHP += artifact.Amount;
                unit.Stats.CurrentHP += artifact.Amount;
                break;
            case StatType.AttackPower:
                unit.Stats.AttackPower += artifact.Amount;
                break;
            case StatType.Armor:
                unit.Stats.Armor += artifact.Amount;
                break;
            case StatType.Speed:
                unit.Stats.Speed += artifact.Amount;
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.Stat.ToString());
        }

        Log.Info($"[ArtifactApplier] Stat applied: {artifact.Stat} +{artifact.Amount} to {unit.Name}");
    }

    private static void ApplyPassive(ArtifactDefinition artifact, Unit unit)
    {
        switch (artifact.AbilityId)
        {
            case "Lifesteal":
                var ls = new Lifesteal(unit, 0.2f);
                ls.OnAttach(unit);
                unit.Passives.Add(ls);
                break;
            case "DoubleStrike":
                var ds = new DoubleStrike(0.25f, 0.75f);
                ds.OnAttach(unit);
                unit.Passives.Add(ds);
                break;
            case "Thorns":
                var thorns = new Thorns();
                thorns.OnAttach(unit);
                unit.Passives.Add(thorns);
                break;
            case "Rage":
                // Rage integrates via the global DamagePipeline rather than Unit.Passives
                // because it requires access to damage context during calculation, not unit events.
                var rage = new Rage(unit);
                DamagePipeline.Register(rage);
                break;
            case "Poison":
                var poison = new PoisonUpgrade(unit);
                poison.OnAttach(unit);
                unit.Passives.Add(poison);
                break;
            case "Bleed":
                var bleed = new BleedUpgrade(unit);
                bleed.OnAttach(unit);
                unit.Passives.Add(bleed);
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.AbilityId);
        }

        Log.Info($"[ArtifactApplier] Passive applied: {artifact.AbilityId} to {unit.Name}");
    }

    private static void ApplyAbility(ArtifactDefinition artifact, Unit unit)
    {
        switch (artifact.AbilityId)
        {
            case "Fireball":
                unit.Abilities.Add(new Fireball());
                break;
            case "Arcane Missiles":
                unit.Abilities.Add(new ArcaneMissiles());
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.AbilityId);
        }

        Log.Info($"[ArtifactApplier] Ability applied: {artifact.AbilityId} to {unit.Name}");
    }
}
