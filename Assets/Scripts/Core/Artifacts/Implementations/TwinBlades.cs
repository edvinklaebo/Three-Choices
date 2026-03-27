using System;

using Core.Combat;
using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Twin Blades effect.
    /// After attacking, grants a percentage chance to perform an additional strike at reduced damage.
    /// Implements ICombatListener to hook into AfterAttackEvent within the combat pipeline.
    /// Uses a re-entrance guard to prevent the extra hit from triggering another extra hit.
    /// Registered by CombatEngine when the artifact is present in Unit.Artifacts.
    /// </summary>
    [Serializable]
    public class TwinBlades : IArtifact, ICombatListener
    {
        [SerializeField] private float _triggerChance;
        [SerializeField] private float _damageMultiplier;

        [NonSerialized] private Unit _owner;
        [NonSerialized] private CombatContext _context;
        [NonSerialized] private bool _isProcessing;

        public int Priority => 210; // Late priority — after normal attack is fully resolved

        public TwinBlades(float triggerChance = 0.3f, float damageMultiplier = 0.75f)
        {
            _triggerChance = triggerChance;
            _damageMultiplier = damageMultiplier;
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
            _context = context;
            context.On<AfterAttackEvent>(OnAfterAttack);
        }

        public void UnregisterHandlers(CombatContext context)
        {
            context.Off<AfterAttackEvent>(OnAfterAttack);
            _context = null;
        }

        private void OnAfterAttack(AfterAttackEvent evt)
        {
            if (evt.Source != _owner) return;
            if (_isProcessing) return;
            if (evt.Target.IsDead) return;
            if (UnityEngine.Random.value >= _triggerChance) return;

            var secondBaseDamage = Mathf.CeilToInt(_owner.Stats.AttackPower * _damageMultiplier);

            Log.Info("[TwinBlades] Extra strike triggered", new
            {
                attacker = _owner.Name,
                target = evt.Target.Name,
                secondBaseDamage,
                multiplier = _damageMultiplier
            });

            _isProcessing = true;
            try
            {
                _context.DealDamage(_owner, evt.Target, secondBaseDamage);
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}
