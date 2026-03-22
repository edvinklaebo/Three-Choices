using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     Bleed status effect that deals damage over time based on stacks.
    ///     Bypasses armor. Stacks accumulate when re-applied.
    /// </summary>
    [Serializable]
    public class Bleed : IStatusEffect
    {
        public Bleed(int stacks, int duration, int baseDamage)
        {
            Stacks = stacks;
            Duration = duration;
            BaseDamage = baseDamage;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="BleedDefinition"/> ScriptableObject.</summary>
        public Bleed(BleedDefinition data)
        {
            UnityEngine.Debug.Assert(data != null, "Bleed: data must not be null");
            Stacks = data.Stacks;
            Duration = data.Duration;
            BaseDamage = data.BaseDamage;
        }

        public string Id => "Bleed";

        [field: SerializeField] public int Stacks { get; set; }

        [field: SerializeField] public int Duration { get; set; }

        [field: SerializeField] public int BaseDamage { get; set; }

        public void OnApply(Unit target)
        {
            Log.Info("Bleed applied", new
            {
                target = target.Name,
                stacks = Stacks,
                duration = Duration
            });
        }

        public int OnTurnStart(Unit target)
        {
            Log.Info("Bleed ticking", new
            {
                target = target.Name,
                damage = Stacks,
                duration = Duration,
                hpBefore = target.Stats.CurrentHP
            });

            var damage = Stacks;
            Duration--;

            Log.Info("Bleed damage calculated", new
            {
                target = target.Name,
                damage,
                remainingDuration = Duration
            });

            return damage;
        }

        public int OnTurnEnd(Unit target)
        {
            // No behavior on turn end
            return 0;
        }

        public void OnExpire(Unit target)
        {
            Log.Info("Bleed expired", new
            {
                target = target.Name
            });
        }

        public void AddStacks(IStatusEffect effect)
        {
            Stacks += effect.Stacks;
            Duration = effect.Duration;
            BaseDamage = effect.BaseDamage;
        }
    }
}