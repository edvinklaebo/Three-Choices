using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Bleed on-hit effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Bleed")]
public class BleedPassiveDefinition : PassiveDefinition
{
    [SerializeField] private int _stacks = 2;
    [SerializeField] private int _duration = 3;
    [SerializeField] private int _baseDamage = 2;

    protected override IPassive CreatePassive(Unit unit) => new BleedUpgrade(unit, _stacks, _duration, _baseDamage);

    // BleedUpgrade's type name is "BleedUpgrade"; override to emit "Bleed" in the log.
    protected override string PassiveLogName => "Bleed";
}
