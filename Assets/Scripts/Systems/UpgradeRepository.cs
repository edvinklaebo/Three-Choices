using System.Collections.Generic;

using Core;

using UnityEngine;

namespace Systems
{
    public interface IUpgradeRepository
    {
        List<UpgradeDefinition> GetAll();
    }

    public class UpgradePool : ScriptableObject, IUpgradeRepository
    {
        private List<UpgradeDefinition> Upgrades;

        private void OnEnable()
        {
            var upgrades = Resources.LoadAll<UpgradeDefinition>(nameof(this.Upgrades));

            this.Upgrades = new List<UpgradeDefinition>(upgrades);
        }

        public List<UpgradeDefinition> GetAll()
        {
            var copyOfUpgrades = new List<UpgradeDefinition>(this.Upgrades);

            return copyOfUpgrades;
        }
    }
}