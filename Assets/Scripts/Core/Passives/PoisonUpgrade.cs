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
            this._stacks = stacks;
            this._duration = duration;
            this._baseDamage = baseDamage;
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
                stacks = this._stacks,
                duration = this._duration,
                baseDamage = this._baseDamage
            });

            target.ApplyStatus(new Poison(this._stacks, this._duration, this._baseDamage));
        }
    }
}
