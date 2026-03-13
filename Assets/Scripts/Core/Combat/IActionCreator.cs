using Interfaces;

namespace Core.Combat
{
    /// <summary>
    /// Interface for abilities that create a custom combat action instead of the default DamageAction.
    /// Implement this on an <see cref="IAbility"/> to supply a bespoke animation (e.g. FireballAction).
    /// </summary>
    public interface IActionCreator
    {
        /// <summary>
        /// Creates the combat action that represents this damage event in the animation queue.
        /// Called by <see cref="CombatDamageResolver"/> in place of the default DamageAction.
        /// </summary>
        ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP);
    }
}
