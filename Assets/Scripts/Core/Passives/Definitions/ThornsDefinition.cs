using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Thorns passive.
    ///     Reflects armor-based damage back to any unit that attacks the owner.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Thorns")]
    public class ThornsDefinition : PassiveDefinition
    {
        protected override IPassive CreatePassive(Unit unit) => new Thorns();
    }
}
