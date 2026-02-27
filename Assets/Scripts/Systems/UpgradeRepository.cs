using System.Collections.Generic;
using UnityEngine;

public interface IUpgradeRepository
{
    List<UpgradeDefinition> GetAll();
}

public class UpgradePool : ScriptableObject, IUpgradeRepository
{
    private List<UpgradeDefinition> Upgrades;

    private void OnEnable()
    {
        var upgrades = Resources.LoadAll<UpgradeDefinition>(nameof(Upgrades));

        Upgrades = new List<UpgradeDefinition>(upgrades);
    }

    public List<UpgradeDefinition> GetAll()
    {
        var copyOfUpgrades = new List<UpgradeDefinition>(Upgrades);

        return copyOfUpgrades;
    }
}