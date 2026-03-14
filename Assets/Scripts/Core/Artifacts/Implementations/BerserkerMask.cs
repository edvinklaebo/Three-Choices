using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Berserker Mask effect.
    /// Increases outgoing damage based on missing health percentage.
    /// At full health the bonus is zero; at 0 HP the bonus doubles damage.
    /// Implements IDamageModifier so DamagePipeline picks it up automatically from Unit.Artifacts.
    /// </summary>
    [Serializable]
    public class BerserkerMask : IArtifact, IDamageModifier
    {
        [NonSerialized] private Unit _owner;

        public int Priority => 200; // Late-stage multiplier, same as Rage

        public void OnAttach(Unit owner)
        {
            _owner = owner;
        }

        public void OnDetach(Unit owner)
        {
            _owner = null;
        }

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            var missingHpPercent = 1f - (float)_owner.Stats.CurrentHP / _owner.Stats.MaxHP;
            var bonus = 1f + missingHpPercent; // 1x at full HP, up to 2x at 0 HP

            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);

            Log.Info("[BerserkerMask] Damage boosted by missing HP", new
            {
                source = _owner.Name,
                missingHpPercent,
                finalDamage = ctx.FinalValue
            });
        }
    }
}
