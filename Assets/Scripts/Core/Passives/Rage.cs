using System;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Increases damage based on missing health percentage.
    ///     Applied as a late-stage multiplier (priority 200).
    /// </summary>
    [Serializable]
    public class Rage : IPassive, IDamageModifier
    {
        private Unit _owner;

        public Rage(Unit owner)
        {
            this._owner = owner;
        }

        public int Priority => 200; // Late-stage multiplier

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != this._owner)
                return;

            var missingHpPercent =
                1f - (float)this._owner.Stats.CurrentHP / this._owner.Stats.MaxHP;

            var bonus = 1f + missingHpPercent; // up to +100%

            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);
        }

        public void OnAttach(Unit owner)
        {
        }

        public void OnDetach(Unit owner)
        {
        }
    }
}