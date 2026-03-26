using System;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Increases damage based on missing health percentage.
    ///     At full HP the bonus is 0; at 0 HP the bonus reaches <see cref="_maxBonus"/> (e.g. 1.0 = +100%).
    ///     Applied as a late-stage multiplier (priority 200).
    /// </summary>
    [Serializable]
    public class Rage : IPassive, IDamageModifier
    {
        [SerializeField] private float _maxBonus;

        private Unit _owner;

        public Rage(Unit owner, float maxBonus = 1.0f)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Debug.Assert(maxBonus >= 0f, "Rage: maxBonus must be >= 0");
            _maxBonus = maxBonus;
        }

        public int Priority => 200; // Late-stage multiplier

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner)
                return;

            var missingHpPercent =
                1f - (float)_owner.Stats.CurrentHP / _owner.Stats.MaxHP;

            var bonus = 1f + missingHpPercent * _maxBonus;

            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * bonus);
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