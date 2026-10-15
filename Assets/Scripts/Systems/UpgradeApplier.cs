using System;

using Core;

namespace Systems
{
    public static class UpgradeApplier
    {
        public static void Apply(UpgradeDefinition upgrade, Unit unit)
        {
            if (upgrade == null) throw new ArgumentNullException(nameof(upgrade));
            if (unit == null) throw new ArgumentNullException(nameof(unit));

            var category = upgrade is AbilityDefinition ? ObtainedItemCategory.Abilities :
                upgrade is PassiveDefinition ? ObtainedItemCategory.Passives : ObtainedItemCategory.Upgrades;
            var id = string.IsNullOrWhiteSpace(upgrade.Id) ? upgrade.name : upgrade.Id;
            if (string.IsNullOrWhiteSpace(id))
                id = upgrade.GetType().Name;
            var item = new ObtainedItem(id, upgrade.DisplayName, upgrade.Description, category);

            upgrade.Apply(unit);
            unit.RecordObtainedItem(item);
        }
    }
}
