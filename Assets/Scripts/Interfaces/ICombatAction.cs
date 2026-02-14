using System.Collections;

/// <summary>
/// Represents one visualized outcome of combat.
/// Contains only presentation logic, no gameplay logic.
/// </summary>
public interface ICombatAction
{
    /// <summary>
    /// Plays the combat action using the provided animation context.
    /// Must always terminate.
    /// </summary>
    /// <param name="ctx">The animation context containing presentation services</param>
    /// <returns>Coroutine for timing</returns>
    IEnumerator Play(AnimationContext ctx);
}
