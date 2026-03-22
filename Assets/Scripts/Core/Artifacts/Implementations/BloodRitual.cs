using System;

using Core.StatusEffects;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Blood Ritual effect.
    /// Applies a Bleed status effect to any enemy hit by the owner.
    /// Subscribes to the owner's OnHit event to trigger on each successful hit.
    /// Bleed stacks accumulate when re-applied.
    /// </summary>
    [Serializable]
    public class BloodRitual : IArtifact
    {
        [SerializeField] private int _bleedStacks;
        [SerializeField] private int _bleedDuration;
        [SerializeField] private int _bleedDamage;

        public int Priority => 100;

        public BloodRitual(int stacks = 2, int duration = 3, int baseDamage = 2)
        {
            _bleedStacks = stacks;
            _bleedDuration = duration;
            _bleedDamage = baseDamage;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="BleedDefinition"/> ScriptableObject.</summary>
        public BloodRitual(BleedDefinition data)
        {
            UnityEngine.Debug.Assert(data != null, "BloodRitual: data must not be null");
            _bleedStacks = data.Stacks;
            _bleedDuration = data.Duration;
            _bleedDamage = data.BaseDamage;
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

            Log.Info("[BloodRitual] Applying bleed on hit", new
            {
                attacker = self.Name,
                target = target.Name,
                stacks = _bleedStacks,
                duration = _bleedDuration
            });

            target.ApplyStatus(new Bleed(_bleedStacks, _bleedDuration, _bleedDamage));
        }
    }
}
