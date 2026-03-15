using System;

using Core.Combat;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Vampiric Fang effect.
    /// Heals the owner for a percentage of every point of damage dealt.
    /// Implements ICombatListener to hook into the PostResolve combat phase.
    /// Registered by CombatEngine when the artifact is present in Unit.Artifacts.
    /// </summary>
    [Serializable]
    public class VampiricFang : IArtifact, ICombatListener
    {
        [SerializeField] private float _percent;
        [NonSerialized] private Unit _owner;

        public int Priority => 250; // Late priority — after damage is applied

        public VampiricFang(float percent = 0.2f)
        {
            _percent = percent;
        }

        public void OnAttach(Unit owner)
        {
            _owner = owner;
        }

        public void OnDetach(Unit owner)
        {
            _owner = null;
        }

        public void RegisterHandlers(CombatContext context)
        {
            context.On<DamagePhaseEvent>(OnDamagePhase);
        }

        public void UnregisterHandlers(CombatContext context)
        {
            context.Off<DamagePhaseEvent>(OnDamagePhase);
        }

        private void OnDamagePhase(DamagePhaseEvent evt)
        {
            if (evt.Phase != CombatPhase.PostResolve) return;
            if (evt.Context.Source != _owner) return;

            var healAmount = Mathf.CeilToInt(evt.Context.FinalDamage * _percent);
            evt.Context.PendingHealing += healAmount;

            Log.Info("[VampiricFang] Lifesteal heal queued", new
            {
                source = _owner.Name,
                finalDamage = evt.Context.FinalDamage,
                healAmount
            });
        }
    }
}
