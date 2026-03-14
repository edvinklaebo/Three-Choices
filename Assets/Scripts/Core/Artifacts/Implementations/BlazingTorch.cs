using System;

using Core.StatusEffects;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Blazing Torch effect.
    /// Applies a Burn status effect to any enemy hit by the owner.
    /// Subscribes to the owner's OnHit event to trigger on each successful hit.
    /// </summary>
    [Serializable]
    public class BlazingTorch : IArtifact
    {
        [SerializeField] private int _burnDuration;
        [SerializeField] private int _burnDamage;

        public int Priority => 100;

        public BlazingTorch(int burnDuration = 3, int burnDamage = 4)
        {
            _burnDuration = burnDuration;
            _burnDamage = burnDamage;
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

            Log.Info("[BlazingTorch] Applying burn on hit", new
            {
                attacker = self.Name,
                target = target.Name,
                burnDamage = _burnDamage,
                burnDuration = _burnDuration
            });

            target.ApplyStatus(new Burn(_burnDuration, _burnDamage));
        }
    }
}
