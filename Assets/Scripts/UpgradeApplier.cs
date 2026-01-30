using System;
using UnityEngine;

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
            default:
                throw new ArgumentOutOfRangeException(upgrade.AbilityId);
        }
    }

    private static void ApplyPassive(UpgradeDefinition upgrade, Unit unit)
    {
        switch (upgrade.AbilityId)
        {
            case "Thorns":
                Debug.Log("Passive Applied: Thorns");
                break;

            case "Rage":
                Debug.Log("Passive Applied: Rage");
                break;
            
            case "Lifesteal":
                Debug.Log("Passive Applied: Lifesteal");
                break;

            case "Poison":
                Debug.Log("Passive Applied: Poison");
                break;
            
            default:
                throw new ArgumentOutOfRangeException(upgrade.AbilityId);
        }
    }
}