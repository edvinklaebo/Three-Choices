using System;

using Core.StatusEffects;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.Passives
{
    [Serializable]
    public class PoisonUpgrade : IPassive
    {
        [SerializeField] private int _stacks;
        [SerializeField] private int _duration;
        [SerializeField] private int _baseDamage;

        public int Priority => 100;

        public PoisonUpgrade(Unit owner, int stacks = 2, int duration = 3, int baseDamage = 2)
        {
            _stacks = stacks;
            _duration = duration;
            _baseDamage = baseDamage;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="PoisonDefinition"/> ScriptableObject.</summary>
        public PoisonUpgrade(Unit owner, PoisonDefinition data)
        {
            UnityEngine.Debug.Assert(data != null, "PoisonUpgrade: data must not be null");
            _stacks = data.Stacks;
            _duration = data.Duration;
            _baseDamage = data.BaseDamage;
        }

        public void OnAttach(Unit owner)
        {
            owner.OnHit += ApplyPoison;
        }

        public void OnDetach(Unit owner)
        {
            owner.OnHit -= ApplyPoison;
        }

        private void ApplyPoison(Unit self, Unit target, int _)
        {
            if (target == null)
                return;

            Log.Info("Poison passive triggered", new
            {
                target = target.Name,
                stacks = _stacks,
                duration = _duration,
                baseDamage = _baseDamage
            });

            target.ApplyStatus(new Poison(_stacks, _duration, _baseDamage));
        }
    }
}
