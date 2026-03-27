using System;
using System.Collections;

using Interfaces;

using UI.Combat;

using UnityEngine;

using Utils;

namespace Core
{
    /// <summary>
    /// Represents an Arcane Missiles ability action in combat.
    /// Plays missile visuals from source to target for each missile hit, then shows damage.
    /// </summary>
    public class ArcaneMissilesAction : ICombatAction
    {
        public Unit Source { get; }
        private Unit Target { get; }
        private int Damage { get; }
        private int TargetHPBefore { get; }
        private int TargetHPAfter { get; }
        private int TargetMaxHP { get; }
        private Sprite Sprite { get; }

        public ArcaneMissilesAction(Unit source, Unit target, int damage, int targetHPBefore, int targetHPAfter, int targetMaxHP, Sprite sprite = null)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Damage = damage;
            TargetHPBefore = targetHPBefore;
            TargetHPAfter = targetHPAfter;
            TargetMaxHP = targetMaxHP;
            Sprite = sprite;
        }

        public IEnumerator Play(AnimationContext ctx)
        {
            Log.Info("ArcaneMissilesAction.Play", new
            {
                source = Source?.Name ?? "null",
                target = Target?.Name ?? "null",
                damage = Damage,
                hpBefore = TargetHPBefore,
                hpAfter = TargetHPAfter
            });

            // Animate projectile from source center to target center (no lunge)
            yield return ctx.Anim.PlayProjectile(Source, Target, Sprite);

            // Play hit effect on target
            yield return ctx.Anim.PlayHit(Target);

            // Show damage UI with arcane color
            ctx.UI.ShowDamage(Target, Damage, TargetHPBefore, TargetHPAfter, TargetMaxHP, DamageType.Arcane);

            // Play hit sound
            ctx.SFX.PlayHitSound(Target);
        }
    }
}
