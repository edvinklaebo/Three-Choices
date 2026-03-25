using Core.Passives;

using Interfaces;

using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Glass Cannon passive.
    ///     The unit deals 60% increased damage but takes 40% increased damage.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Glass Cannon")]
    public class GlassCannonDefinition : PassiveDefinition
    {
        protected override IPassive CreatePassive(Unit unit) => new GlassCannon(unit);
    }
}
