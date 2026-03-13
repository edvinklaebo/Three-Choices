using Core;
using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Poison on-hit effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Poison")]
public class PoisonPassiveDefinition : PassiveDefinition
{
    [SerializeField] private int _stacks = 2;
    [SerializeField] private int _duration = 3;
    [SerializeField] private int _baseDamage = 2;

    protected override string PassiveLogName => "Poison";
    protected override IPassive CreatePassive(Unit unit) => new PoisonUpgrade(unit, _stacks, _duration, _baseDamage);
}
