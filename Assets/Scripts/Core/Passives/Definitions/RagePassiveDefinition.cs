using Core;
using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Rage effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Rage")]
public class RagePassiveDefinition : PassiveDefinition
{
    protected override string PassiveLogName => "Rage";
    protected override IPassive CreatePassive(Unit unit) => new Rage(unit);
}
