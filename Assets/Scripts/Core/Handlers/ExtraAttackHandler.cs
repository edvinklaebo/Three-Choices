using Core.Combat;
using Core.Passives;

using UnityEngine;

using Utils;

namespace Core.Handlers
{
    /// <summary>
    /// Handles the combat-pipeline integration for <see cref="DoubleStrike"/>:
    /// executes pending extra attacks after each attack, enforces recursion safety,
    /// and emits log entries.  Created and registered by <see cref="CombatEngine"/>
    /// via <see cref="ICombatHandlerProvider"/>.
    /// </summary>
    public class ExtraAttackHandler : ICombatListener
    {
        private readonly Unit _owner;
        private readonly DoubleStrike _passive;
        private CombatContext _context;
        private bool _isProcessingStrikes;

        public int Priority => 210; // Late priority — after damage is dealt, after lifesteal

        public ExtraAttackHandler(Unit owner, DoubleStrike passive)
        {
            this._owner = owner;
            this._passive = passive;
        }

        public void RegisterHandlers(CombatContext context)
        {
            this._context = context;
            context.On<AfterAttackEvent>(OnAfterAttack);
        }

        public void UnregisterHandlers(CombatContext context)
        {
            this._context = null;
            context.Off<AfterAttackEvent>(OnAfterAttack);
        }

        private void OnAfterAttack(AfterAttackEvent evt)
        {
            if (evt.Source != this._owner)
                return;

            if (this._isProcessingStrikes)
                return;

            this._isProcessingStrikes = true;
            this._passive.Suspend();
            try
            {
                var strikes = this._passive.ConsumePendingStrikes();
                foreach (var strikeData in strikes)
                {
                    if (strikeData.Target.IsDead)
                        continue;

                    var secondBaseDamage = Mathf.CeilToInt(this._owner.Stats.AttackPower * strikeData.DamageMultiplier);

                    Log.Info("Double Strike second hit", new
                    {
                        attacker = this._owner.Name,
                        target = strikeData.Target.Name,
                        secondBaseDamage,
                        strikeData.DamageMultiplier
                    });

                    this._context.DealDamage(this._owner, strikeData.Target, secondBaseDamage);
                }
            }
            finally
            {
                this._passive.Resume();
                this._isProcessingStrikes = false;
            }
        }
    }
}
