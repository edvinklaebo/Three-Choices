using System.Collections;
using UnityEngine;

/// <summary>
///     Service for playing unit animations.
///     Handles lunge movements, attack animations, and visual feedback.
///     Works with UnitView for positioning.
/// </summary>
public class AnimationService
{
    [Header("Animation Timings")] private const float LUNGE_DURATION = 0.2f;

    private const float RETURN_DURATION = 0.2f;
    private const float HIT_REACT_DURATION = 0.15f;
    private CombatView _combatView;

    public void SetCombatView(CombatView combatView)
    {
        _combatView = combatView;
    }

    public IEnumerator PlayAttack(Unit source)
    {
        Log.Info("Playing attack animation", new { source = source.Name });

        var unitView = GetUnitView(source);
        if (unitView != null && unitView.SpriteTransform != null && unitView.LungePoint != null &&
            unitView.IdlePoint != null)
        {
            // Calculate lunge offset in local space (relative to parent UnitView)
            var idleWorldPos = unitView.IdlePoint.position;
            var lungeWorldPos = unitView.LungePoint.position;
            var lungeOffset = lungeWorldPos - idleWorldPos;

            // Lunge forward - animate sprite in local space
            yield return LungeSprite(unitView.SpriteTransform, lungeOffset, LUNGE_DURATION);

            // Brief pause at lunge point
            yield return new WaitForSeconds(0.05f);

            // Return to idle - reset sprite position
            yield return LungeSprite(unitView.SpriteTransform, Vector3.zero, RETURN_DURATION);
        }
        else
        {
            // Fallback: simple delay
            yield return new WaitForSeconds(0.3f);
        }
    }

    public IEnumerator PlayHit(Unit target)
    {
        Log.Info("Playing hit animation", new { target = target.Name });

        var unitView = GetUnitView(target);
        if (unitView != null)
            // Simple hit reaction - shake/flash would go here
            yield return new WaitForSeconds(HIT_REACT_DURATION);
        else
            yield return new WaitForSeconds(0.2f);
    }

    public IEnumerator PlayDeath(Unit target)
    {
        Log.Info("Playing death animation", new { target = target.Name });

        var unitView = GetUnitView(target);
        if (unitView != null)
            // Simple fade or scale down - placeholder
            yield return new WaitForSeconds(0.5f);
        else
            yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    ///     Move transform to target position over duration.
    ///     Simple linear interpolation.
    /// </summary>
    private IEnumerator LungeToPoint(Transform transform, Vector3 targetPosition, float duration)
    {
        var startPosition = transform.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
    }

    /// <summary>
    ///     Move sprite to target local offset over duration.
    ///     Animates in local space, so parent UnitView stays at its world position.
    /// </summary>
    private IEnumerator LungeSprite(Transform spriteTransform, Vector3 targetLocalOffset, float duration)
    {
        var startLocalPos = spriteTransform.localPosition;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;

            spriteTransform.localPosition = Vector3.Lerp(startLocalPos, targetLocalOffset, t);

            yield return null;
        }

        spriteTransform.localPosition = targetLocalOffset;
    }

    /// <summary>
    ///     Get the UnitView for a given Unit.
    /// </summary>
    private UnitView GetUnitView(Unit unit)
    {
        if (_combatView == null)
            return null;

        return _combatView.GetUnitView(unit);
    }
}