using Core;

namespace Interfaces
{
    /// <summary>
    ///     Modifies or suppresses incoming healing on a unit.
    ///     Implement on status effects or passives that alter how much a unit can be healed.
    ///     Applied in <see cref="Unit.Heal" /> before healing is resolved.
    /// </summary>
    public interface IHealingModifier
    {
        /// <summary>
        ///     Called when the unit is about to be healed.
        ///     Return the adjusted heal amount; return 0 to fully block healing.
        /// </summary>
        /// <param name="unit">The unit receiving the healing.</param>
        /// <param name="amount">The incoming heal amount.</param>
        /// <returns>The modified heal amount.</returns>
        int ModifyHealing(Unit unit, int amount);
    }
}
