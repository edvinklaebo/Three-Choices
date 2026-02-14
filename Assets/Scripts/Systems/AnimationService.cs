using System.Collections;
using UnityEngine;

/// <summary>
/// Service for playing unit animations.
/// Placeholder implementation for combat animation system.
/// </summary>
public class AnimationService
{
    public IEnumerator PlayAttack(Unit source)
    {
        Log.Info("Playing attack animation", new { source = source.Name });
        // Placeholder: yield for animation duration
        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator PlayHit(Unit target)
    {
        Log.Info("Playing hit animation", new { target = target.Name });
        // Placeholder: yield for animation duration
        yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator PlayDeath(Unit target)
    {
        Log.Info("Playing death animation", new { target = target.Name });
        // Placeholder: yield for animation duration
        yield return new WaitForSeconds(0.5f);
    }
}
