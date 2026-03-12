using System;
using System.Collections.Generic;

using Core.Combat;
using Core.Handlers;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    [Serializable]
    public class DoubleStrike : IPassive, ICombatHandlerProvider
    {
        [SerializeField] private float triggerChance;
        [SerializeField] private float damageMultiplier;
    
        private List<DoubleStrikeData> _pendingStrikes = new();
        private bool _suspended;

        public int Priority => 100;

        public DoubleStrike(float triggerChance, float damageMultiplier)
        {
            this.triggerChance = triggerChance;
            this.damageMultiplier = damageMultiplier;
        }

        public void OnAttach(Unit owner)
        {
            owner.OnHit += TryTrigger;
        }

        public void OnDetach(Unit owner)
        {
            owner.OnHit -= TryTrigger;
        }

        public ICombatListener CreateCombatHandler(Unit owner) => new ExtraAttackHandler(owner, this);

        /// <summary>Suspends strike queuing during second-hit processing (called by <see cref="ExtraAttackHandler"/>).</summary>
        internal void Suspend() => this._suspended = true;

        /// <summary>Resumes strike queuing after second-hit processing (called by <see cref="ExtraAttackHandler"/>).</summary>
        internal void Resume() => this._suspended = false;

        private void TryTrigger(Unit self, Unit target, int damage)
        {
            if (this._suspended)
                return;

            if (UnityEngine.Random.value >= this.triggerChance)
                return;

            this._pendingStrikes.Add(new DoubleStrikeData(target, this.damageMultiplier));
        }

        /// <summary>
        ///     Get and clear all pending double strikes that need to be executed.
        /// </summary>
        public List<DoubleStrikeData> ConsumePendingStrikes()
        {
            var strikes = new List<DoubleStrikeData>(this._pendingStrikes);
            this._pendingStrikes.Clear();
            return strikes;
        }

        public readonly struct DoubleStrikeData
        {
            public readonly Unit Target;
            public readonly float DamageMultiplier;

            public DoubleStrikeData(Unit target, float damageMultiplier)
            {
                this.Target = target;
                this.DamageMultiplier = damageMultiplier;
            }
        }
    }
}
