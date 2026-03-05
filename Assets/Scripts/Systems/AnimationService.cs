using System.Collections;
using UnityEngine;

/// <summary>
///     Service for playing unit animations.
///     Handles lunge movements, projectile animations, and visual feedback.
///     Works with UnitView for positioning.
/// </summary>
public class AnimationService
{
    [Header("Animation Timings")] private const float LUNGE_DURATION = 0.2f;

    private const float RETURN_DURATION = 0.2f;
    private const float HIT_REACT_DURATION = 0.15f;
    private const float DEATH_DURATION = 0.5f;
    private const float PROJECTILE_DURATION = 0.4f;

    private CombatView _combatView;
    private Transform _projectile;

    public void SetCombatView(CombatView combatView)
    {
        _combatView = combatView;
    }

    /// <summary>
    ///     Provide the projectile Transform to animate during <see cref="PlayProjectile"/>.
    ///     The object should start inactive; this service activates and deactivates it automatically.
    /// </summary>
    public void SetProjectile(Transform projectile)
    {
        _projectile = projectile;
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

    /// <summary>
    ///     Animate a projectile from the source unit's center to the target unit's center.
    ///     Falls back to a simple delay when the projectile Transform is not configured in the scene.
    /// </summary>
    public IEnumerator PlayProjectile(Unit source, Unit target)
    {
        Log.Info("Playing projectile animation", new { source = source.Name, target = target.Name });

        var sourceView = GetUnitView(source);
        var targetView = GetUnitView(target);

        if (sourceView == null || targetView == null || _projectile == null)
        {
            yield return new WaitForSeconds(PROJECTILE_DURATION);
            yield break;
        }

        var startPos = sourceView.transform.position;
        var endPos = targetView.transform.position;

        _projectile.gameObject.SetActive(true);
        _projectile.position = startPos;

        var elapsed = 0f;
        while (elapsed < PROJECTILE_DURATION)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / PROJECTILE_DURATION);
            _projectile.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        _projectile.gameObject.SetActive(false);
    }

    public IEnumerator PlayHit(Unit target)
    {
        Log.Info("Playing hit animation", new { target = target.Name });

        var unitView = GetUnitView(target);
        if (unitView != null)
            // Simple hit reaction - shake/flash would go here
            yield return new WaitForSeconds(HIT_REACT_DURATION);
        else
            yield return new WaitForSeconds(RETURN_DURATION);
    }

    public IEnumerator PlayDeath(Unit target)
    {
        Log.Info("Playing death animation", new { target = target.Name });

        yield return new WaitForSeconds(DEATH_DURATION);
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