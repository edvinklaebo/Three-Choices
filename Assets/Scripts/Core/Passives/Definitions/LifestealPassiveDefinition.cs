using Core;
using Core.Passives;
using Interfaces;
using UnityEngine;

/// <summary>Passive definition for the Lifesteal effect.</summary>
[CreateAssetMenu(menuName = "Upgrades/Passives/Lifesteal")]
public class LifestealPassiveDefinition : PassiveDefinition
{
    [SerializeField] private float _percent = 0.2f;

    protected override string PassiveLogName => "Lifesteal";
    protected override IPassive CreatePassive(Unit unit) => new Lifesteal(unit, _percent);
}
