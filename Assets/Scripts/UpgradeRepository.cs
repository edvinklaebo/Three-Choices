using System.Collections.Generic;
using UnityEngine;

public interface IUpgradeRepository
{
    List<UpgradeDefinition> GetAll();
}

public class UpgradePool : ScriptableObject, IUpgradeRepository
{
    private readonly List<UpgradeDefinition> Upgrades;

    public UpgradePool()
    {
        var upgrades = Resources.LoadAll<UpgradeDefinition>("Upgrades");
        
        Upgrades = new List<UpgradeDefinition>(upgrades);
    }


    public List<UpgradeDefinition> GetAll()
    {
        return Upgrades;
    }
}
