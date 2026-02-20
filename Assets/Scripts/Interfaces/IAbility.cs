public interface IAbility
{
    /// <summary>
    /// Executes the ability. Returns the amount of HP damage dealt to the target;
    /// the caller is responsible for applying it via <see cref="CombatContext.ApplyDamage"/>.
    /// </summary>
    int OnCast(Unit self, Unit target);
}