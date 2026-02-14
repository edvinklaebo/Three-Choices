using System.Collections;
using UnityEngine;

/// <summary>
/// Service for visual effects during combat.
/// Placeholder implementation for combat animation system.
/// </summary>
public class VFXService
{
    public IEnumerator PlayStatus(Unit target, string effectName)
    {
        Log.Info("Playing status VFX", new { target = target.Name, effect = effectName });
        // Placeholder: yield for VFX duration
        yield return new WaitForSeconds(0.3f);
    }

    public IEnumerator PlayImpact(Unit target)
    {
        Log.Info("Playing impact VFX", new { target = target.Name });
        // Placeholder: yield for VFX duration
        yield return new WaitForSeconds(0.15f);
    }
}
