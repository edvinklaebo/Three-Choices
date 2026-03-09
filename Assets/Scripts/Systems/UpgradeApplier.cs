using System;

public static class UpgradeApplier
{
    public static void Apply(UpgradeDefinition upgrade, Unit unit)
    {
        upgrade.Apply(unit);
    }
}
