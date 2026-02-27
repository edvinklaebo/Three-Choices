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
            case "Arcane Missiles":
                Log.Info("Ability Applied: Arcane Missiles");
                unit.Abilities.Add(new ArcaneMissiles());
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
                var thorns = new Thorns();
                thorns.OnAttach(unit);
                unit.Passives.Add(thorns);
                break;

            case "Rage":
                Log.Info("Passive Applied: Rage");
                var rage = new Rage(unit);
                DamagePipeline.Register(rage);
                break;

            case "Lifesteal":
                Log.Info("Passive Applied: Lifesteal");
                var ls = new Lifesteal(unit, 0.2f);
                ls.OnAttach(unit);
                unit.Passives.Add(ls);
                break;

            case "Poison":
                Log.Info("Passive Applied: Poison");
                var poison = new PoisonUpgrade(unit);
                poison.OnAttach(unit);
                unit.Passives.Add(poison);
                break;

            case "Bleed":
                Log.Info("Passive Applied: Bleed");
                var bleed = new BleedUpgrade(unit);
                bleed.OnAttach(unit);
                unit.Passives.Add(bleed);
                break;

            case "DoubleStrike":
                Log.Info("Passive Applied: DoubleStrike");
                var doubleStrike = new DoubleStrike(0.25f, 0.75f);
                doubleStrike.OnAttach(unit);
                unit.Passives.Add(doubleStrike);
                break;

            default:
                throw new ArgumentOutOfRangeException(upgrade.AbilityId);
        }
    }
}