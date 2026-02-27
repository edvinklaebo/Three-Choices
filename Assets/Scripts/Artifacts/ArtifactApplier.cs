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
            case ArtifactEffectType.StatBoost:
                ApplyStat(artifact, player);
                break;
            case ArtifactEffectType.PercentStatBoost:
                ApplyPercentStat(artifact, player);
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
            case "PhantomStrike":
                var phantom = new PhantomStrike();
                phantom.OnAttach(unit);
                unit.Passives.Add(phantom);
                break;
            case "DeathShield":
                var shield = new DeathShield();
                shield.OnAttach(unit);
                unit.Passives.Add(shield);
                break;
            case "CritChance":
                var crit = new CritChancePassive(artifact.Amount / 100f);
                crit.OnAttach(unit);
                unit.Passives.Add(crit);
                break;
            case "PoisonAmplifier":
                var amp = new PoisonAmplifier();
                amp.OnAttach(unit);
                unit.Passives.Add(amp);
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

