using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Double Strike effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Double Strike")]
public class DoubleStrikePassiveDefinition : PassiveDefinition
{
    [SerializeField] private float _triggerChance = 0.25f;
    [SerializeField] private float _damageMultiplier = 0.75f;

    protected override IPassive CreatePassive(Unit unit) => new DoubleStrike(_triggerChance, _damageMultiplier);
}
