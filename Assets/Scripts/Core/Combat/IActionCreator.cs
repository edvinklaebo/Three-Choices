/// <summary>
/// Interface for abilities that can create their own combat actions
/// </summary>
public interface IActionCreator
{
    /// <summary>
    /// Called after ability execution to create combat actions for animation
    /// </summary>
    /// <param name="context">Combat context to add actions to</param>
    /// <param name="source">Source unit</param>
    /// <param name="target">Target unit</param>
    /// <param name="hpBefore">Target HP before ability</param>
    /// <param name="hpAfter">Target HP after ability</param>
    void CreateActions(CombatContext context, Unit source, Unit target, int hpBefore, int hpAfter);
}
