using System.Collections;
using UnityEngine;

/// <summary>
/// Service for playing unit animations.
/// Handles lunge movements, attack animations, and visual feedback.
/// Works with UnitView for positioning.
/// </summary>
public class AnimationService
{
    private CombatView _combatView;

    [Header("Animation Timings")]
    private const float LUNGE_DURATION = 0.2f;
    private const float RETURN_DURATION = 0.2f;
    private const float HIT_REACT_DURATION = 0.15f;

    public void SetCombatView(CombatView combatView)
    {
        _combatView = combatView;
    }

    public IEnumerator PlayAttack(Unit source)
    {
        Log.Info("Playing attack animation", new { source = source.Name });

        var unitView = GetUnitView(source);
        if (unitView != null && unitView.LungePoint != null)
        {
            // Lunge forward
            yield return LungeToPoint(unitView.transform, unitView.LungePoint.position, LUNGE_DURATION);

            // Brief pause at lunge point
            yield return new WaitForSeconds(0.05f);

            // Return to idle
            yield return LungeToPoint(unitView.transform, unitView.IdlePoint.position, RETURN_DURATION);
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
        {
            // Simple hit reaction - shake/flash would go here
            yield return new WaitForSeconds(HIT_REACT_DURATION);
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }
    }

    public IEnumerator PlayDeath(Unit target)
    {
        Log.Info("Playing death animation", new { target = target.Name });

        var unitView = GetUnitView(target);
        if (unitView != null)
        {
            // Simple fade or scale down - placeholder
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }
    }

    /// <summary>
    /// Move transform to target position over duration.
    /// Simple linear interpolation.
    /// </summary>
    private IEnumerator LungeToPoint(Transform transform, Vector3 targetPosition, float duration)
    {
        var startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        transform.position = targetPosition;
    }

    /// <summary>
    /// Get the UnitView for a given Unit.
    /// </summary>
    private UnitView GetUnitView(Unit unit)
    {
        if (_combatView == null)
            return null;

        if (_combatView.PlayerView?.Unit == unit)
            return _combatView.PlayerView;

        if (_combatView.EnemyView?.Unit == unit)
            return _combatView.EnemyView;

        return null;
    }
}
