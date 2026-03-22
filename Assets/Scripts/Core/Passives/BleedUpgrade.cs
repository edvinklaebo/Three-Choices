using System;

using Core.StatusEffects;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Passive upgrade that applies Bleed status effect when owner hits a target.
    /// </summary>
    [Serializable]
    public class BleedUpgrade : IPassive
    {
        [SerializeField] private int stacks;
        [SerializeField] private int duration;
        [SerializeField] private int baseDamage;

        public int Priority => 100;

        public BleedUpgrade(Unit owner, int stacks = 2, int duration = 3, int baseDamage = 2)
        {
            this.stacks = stacks;
            this.duration = duration;
            this.baseDamage = baseDamage;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="BleedDefinition"/> ScriptableObject.</summary>
        public BleedUpgrade(Unit owner, BleedDefinition data)
        {
            UnityEngine.Debug.Assert(data != null, "BleedUpgrade: data must not be null");
            stacks = data.Stacks;
            duration = data.Duration;
            baseDamage = data.BaseDamage;
        }

        public void OnAttach(Unit owner)
        {
            owner.OnHit += ApplyBleed;
        }

        public void OnDetach(Unit owner)
        {
            owner.OnHit -= ApplyBleed;
        }

        private void ApplyBleed(Unit attacker, Unit target, int _)
        {
            target?.ApplyStatus(new Bleed(stacks, duration, baseDamage));
        }
    }
}
