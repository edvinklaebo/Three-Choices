using Core.Passives;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Rage passive.
    ///     Increases damage output based on missing health percentage.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Rage")]
    public class RageDefinition : PassiveDefinition
    {
        protected override IPassive CreatePassive(Unit unit) => new Rage(unit);
    }
}
