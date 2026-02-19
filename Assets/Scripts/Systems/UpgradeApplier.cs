using System;

public static class UpgradeApplier
{
    public static void Apply(UpgradeDefinition upgrade, Unit unit)
    {
        switch (upgrade.Type)
        {
            case UpgradeType.Stat:
                ApplyStat(upgrade, unit);
                break;

            case UpgradeType.Ability:
                ApplyAbility(upgrade, unit);
                break;

            case UpgradeType.Passive:
                ApplyPassive(upgrade, unit);
                break;
            default:
                throw new ArgumentOutOfRangeException(upgrade.Type.ToString());
        }
    }

    private static void ApplyStat(UpgradeDefinition upgrade, Unit unit)
    {
        switch (upgrade.Stat)
        {
            case StatType.MaxHP:
                unit.Stats.MaxHP += upgrade.Amount;
                unit.Stats.CurrentHP += upgrade.Amount;
                break;

            case StatType.AttackPower:
                unit.Stats.AttackPower += upgrade.Amount;
                break;

            case StatType.Armor:
                unit.Stats.Armor += upgrade.Amount;
                break;

            case StatType.Speed:
                unit.Stats.Speed += upgrade.Amount;
                break;

            default:
                throw new ArgumentOutOfRangeException(upgrade.Type.ToString());
        }
    }

    private static void ApplyAbility(UpgradeDefinition upgrade, Unit unit)
    {
        switch (upgrade.AbilityId)
        {
            case "Fireball":
                Log.Info("Ability Applied: Fireball");
                unit.Abilities.Add(new Fireball());
                break;
            default:
                throw new ArgumentOutOfRangeException(upgrade.AbilityId);
        }
    }

    private static void ApplyPassive(UpgradeDefinition upgrade, Unit unit)
    {
        switch (upgrade.AbilityId)
        {
            case "Thorns":
                Log.Info("Passive Applied: Thorns");
                unit.Passives.Add(new Thorns(unit));
                break;

            case "Rage":
                Log.Info("Passive Applied: Rage");
                DamagePipeline.Register(new Rage(unit));
                break;

            case "Lifesteal":
                Log.Info("Passive Applied: Lifesteal");
                unit.Passives.Add(new Lifesteal(unit, 0.2f));
                break;

            case "Poison":
                Log.Info("Passive Applied: Poison");
                unit.Passives.Add(new Poison(unit));
                break;

            case "Bleed":
                Log.Info("Passive Applied: Bleed");
                unit.Passives.Add(new BleedUpgrade(unit));
                break;

            default:
                throw new ArgumentOutOfRangeException(upgrade.AbilityId);
        }
    }
}