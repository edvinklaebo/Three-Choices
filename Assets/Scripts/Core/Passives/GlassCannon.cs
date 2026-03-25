using System;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Glass Cannon passive: deal increased damage, but take increased damage.
    ///     Balance values are supplied by <see cref="Definitions.GlassCannonDefinition"/>.
    ///     Applied as a late-stage multiplier (priority 200).
    /// </summary>
    [Serializable]
    public class GlassCannon : IPassive, IDamageModifier
    {
        [SerializeField] private float _outgoingBonus = 0.6f;
        [SerializeField] private float _incomingPenalty = 0.4f;

        private Unit _owner;

        public GlassCannon(Unit owner, float outgoingBonus, float incomingPenalty)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Debug.Assert(outgoingBonus >= 0f, "GlassCannon: outgoingBonus must be >= 0");
            Debug.Assert(incomingPenalty >= 0f, "GlassCannon: incomingPenalty must be >= 0");
            _outgoingBonus = outgoingBonus;
            _incomingPenalty = incomingPenalty;
        }

        public int Priority => 200;

        public void Modify(DamageContext ctx)
        {
            if (_owner == null)
                return;

            if (ctx.Source == _owner)
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + _outgoingBonus));

            if (ctx.Target == _owner)
                ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * (1f + _incomingPenalty));
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
