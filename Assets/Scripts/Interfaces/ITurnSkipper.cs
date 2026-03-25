namespace Interfaces
{
    /// <summary>
    /// Implemented by status effects that cause the owning unit to skip their attack turn.
    /// The CombatEngine checks for this interface before processing abilities and attacks.
    /// </summary>
    public interface ITurnSkipper
    {
        /// <summary>
        /// Consumes one turn-skip charge.
        /// Returns true if the turn should be skipped; false if no charges remain.
        /// </summary>
        bool ConsumeTurnSkip();
    }
}
