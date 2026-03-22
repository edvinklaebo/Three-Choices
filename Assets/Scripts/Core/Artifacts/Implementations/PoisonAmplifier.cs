using System;

using Core.StatusEffects;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Poison Tipped Darts effect.
    /// When the owner hits an enemy that has Poison, adds bonus stacks equal to the current stack count,
    /// effectively doubling any poison stacks applied on the same hit.
    /// Subscribes after PoisonUpgrade so stacks are already present when this fires.
    /// </summary>
    [Serializable]
    public class PoisonAmplifier : IArtifact
    {
        [SerializeField] private int _bonusStacks;
        [SerializeField] private int _bonusDuration;
        [SerializeField] private int _bonusBaseDamage;

        public int Priority => 100;

        public PoisonAmplifier(int bonusStacks = 2, int bonusDuration = 3, int bonusBaseDamage = 2)
        {
            _bonusStacks = bonusStacks;
            _bonusDuration = bonusDuration;
            _bonusBaseDamage = bonusBaseDamage;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="PoisonDefinition"/> ScriptableObject.</summary>
        public PoisonAmplifier(PoisonDefinition data)
        {
            UnityEngine.Debug.Assert(data != null, "PoisonAmplifier: data must not be null");
            _bonusStacks = data.Stacks;
            _bonusDuration = data.Duration;
            _bonusBaseDamage = data.BaseDamage;
        }

        public void OnAttach(Unit owner)
        {
            owner.OnHit += OnHit;
        }

        public void OnDetach(Unit owner)
        {
            owner.OnHit -= OnHit;
        }

        private void OnHit(Unit self, Unit target, int damage)
        {
            if (target == null || target.IsDead)
                return;

            for (var i = 0; i < target.StatusEffects.Count; i++)
            {
                if (target.StatusEffects[i].Id != "Poison")
                    continue;

                target.ApplyStatus(new Poison(_bonusStacks, _bonusDuration, _bonusBaseDamage));

                Log.Info("[PoisonAmplifier] Bonus poison stacks added", new
                {
                    target = target.Name,
                    bonusStacks = _bonusStacks
                });

                break;
            }
        }
    }
}
