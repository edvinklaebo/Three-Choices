using System;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Glass Cannon passive: deal 60% increased damage, but take 40% increased damage.
    ///     Applied as a late-stage multiplier (priority 200).
    /// </summary>
    [Serializable]
    public class GlassCannon : IPassive, IDamageModifier
    {
        private const float OutgoingBonus = 0.6f;
        private const float IncomingPenalty = 0.4f;

        private Unit _owner;

        public GlassCannon(Unit owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        public int Priority => 200;

        public void Modify(DamageContext ctx)
        {
            if (_owner == null)
                return;

            if (ctx.Source == _owner)
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + OutgoingBonus));

            if (ctx.Target == _owner)
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + IncomingPenalty));
        }

        public void OnAttach(Unit owner)
        {
            _owner = owner;
        }

        public void OnDetach(Unit owner)
        {
            _owner = null;
        }
    }
}
