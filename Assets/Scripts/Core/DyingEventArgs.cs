/// <summary>
/// Event arguments for <see cref="Unit.Dying"/>.
/// A subscriber can set <see cref="Cancelled"/> to true and provide a <see cref="ReviveHp"/> value
/// to prevent the death and restore the unit to that HP instead.
/// Only the first cancellation takes effect; subsequent subscribers should check <see cref="Cancelled"/>
/// before overwriting an existing revive.
/// </summary>
public class DyingEventArgs
{
    /// <summary>
    /// Set to true to cancel the death. The unit will remain alive.
    /// </summary>
    public bool Cancelled { get; set; }

    /// <summary>
    /// HP to restore when <see cref="Cancelled"/> is true.
    /// Clamped to [1, MaxHP] by <see cref="Unit"/> (minimum of 1 so the cancellation cannot leave the unit at 0 HP).
    /// </summary>
    public int ReviveHp { get; set; }
}
