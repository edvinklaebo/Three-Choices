using System;
using UnityEngine;

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
            case ArtifactEffectType.PercentStatBoost:
                ApplyPercentStat(artifact, player);
                break;
            case ArtifactEffectType.AddArtifact:
                ApplyArtifact(artifact, player);
                break;
            case ArtifactEffectType.AddAbility:
                ApplyAbility(artifact, player);
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.EffectType.ToString());
        }
    }

    private static void ApplyPercentStat(ArtifactDefinition artifact, Unit unit)
    {
        var multiplier = 1f + artifact.Amount / 100f;

        switch (artifact.Stat)
        {
            case StatType.MaxHP:
                var hpGain = Mathf.CeilToInt(unit.Stats.MaxHP * (multiplier - 1f));
                unit.Stats.MaxHP += hpGain;
                unit.Stats.CurrentHP += hpGain;
                break;
            case StatType.AttackPower:
                unit.Stats.AttackPower = Mathf.CeilToInt(unit.Stats.AttackPower * multiplier);
                break;
            case StatType.Armor:
                unit.Stats.Armor = Mathf.CeilToInt(unit.Stats.Armor * multiplier);
                break;
            case StatType.Speed:
                unit.Stats.Speed = Mathf.CeilToInt(unit.Stats.Speed * multiplier);
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.Stat.ToString(),
                    $"[ArtifactApplier.ApplyPercentStat] Unsupported stat '{artifact.Stat}' on artifact '{artifact.Id}'");
        }

        Log.Info($"[ArtifactApplier] Percent stat applied: {artifact.Stat} x{multiplier} to {unit.Name}");
    }

    private static void ApplyArtifact(ArtifactDefinition artifact, Unit unit)
    {
        IPassive passive;

        switch (artifact.AbilityId)
        {
            case "PhantomStrike":
                passive = new PhantomStrike();
                break;
            case "DeathShield":
                passive = new DeathShield();
                break;
            case "CritChance":
                passive = new CritChancePassive(artifact.Amount / 100f);
                break;
            case "PoisonAmplifier":
                passive = new PoisonAmplifier();
                break;
            default:
                throw new ArgumentOutOfRangeException(artifact.AbilityId);
        }

        passive.OnAttach(unit);
        unit.Passives.Add(passive);

        Log.Info($"[ArtifactApplier] Artifact passive applied: {artifact.AbilityId} to {unit.Name}");
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

