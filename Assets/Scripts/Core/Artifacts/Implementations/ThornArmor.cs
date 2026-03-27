using System;

using Core.Combat;

using Interfaces;
using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Thorn Armor effect.
    /// Reflects half of the owner's armor value as damage to any attacker.
    /// Implements ICombatListener and IActionCreator to integrate with the combat pipeline.
    /// Uses a re-entrance guard to prevent infinite reflect chains.
    /// Registered by CombatEngine when the artifact is present in Unit.Artifacts.
    /// </summary>
    [Serializable]
    public class ThornArmor : IArtifact, ICombatListener, IActionCreator
    {
        [NonSerialized] private Unit _owner;
        [NonSerialized] private CombatContext _context;
        [NonSerialized] private bool _isReflecting;

        public int Priority => 100;

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
            context.On<OnHitEvent>(OnHit);
        }

        public void UnregisterHandlers(CombatContext context)
        {
            context.Off<OnHitEvent>(OnHit);
            _context = null;
        }

        private void OnHit(OnHitEvent evt)
        {
            if (_owner == null) return;
            if (evt.Target != _owner) return;
            if (evt.Source == null) return;
            if (_isReflecting) return;

            var thornsDamage = _owner.Stats.Armor / 2;
            if (thornsDamage <= 0) return;

            Log.Info($"[ThornArmor] Reflect: {_owner.Name} deals {thornsDamage} to {evt.Source.Name}");

            _isReflecting = true;
            try
            {
                _context.DealDamage(_owner, evt.Source, thornsDamage, actionCreator: this);
            }
            finally
            {
                _isReflecting = false;
            }
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new ThornsAction(source, target, finalDamage, hpBefore, hpAfter, maxHP);
    }
}
