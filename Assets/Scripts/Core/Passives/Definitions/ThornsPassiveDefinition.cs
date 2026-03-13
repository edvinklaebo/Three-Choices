using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Thorns effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Thorns")]
public class ThornsPassiveDefinition : PassiveDefinition
{
    protected override IPassive CreatePassive(Unit unit) => new Thorns();
}
