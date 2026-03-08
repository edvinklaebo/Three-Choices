using System;

public static class UpgradeApplier
{
    
    public static void Apply(UpgradeDefinition upgrade, Unit unit)
    {
        upgrade.Apply(unit);
    }



    private static void ApplyAbility(AbilityDefinition ability, Unit unit)
    {

    }



    private static void ApplyPassive(PassiveDefinition passive, Unit unit)
    {
  
    }
}