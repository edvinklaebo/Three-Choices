using System;

using Core.Combat;
using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Spell Echo effect (Epic).
    /// After each ability is cast, it is automatically cast a second time (echoed).
    /// Uses a re-entrance guard to prevent the echo from triggering another echo.
    /// Registered by CombatEngine when the artifact is present in Unit.Artifacts.
    /// </summary>
    [Serializable]
    public class SpellEcho : IArtifact, ICombatListener
    {
        [NonSerialized] private Unit _owner;
        [NonSerialized] private CombatContext _context;
        [NonSerialized] private bool _isEchoing;

        public int Priority => 200; // Late priority — fires after the original cast is fully resolved

        public void OnAttach(Unit owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            _owner = owner;
        }

        public void OnDetach(Unit owner)
        {
            _owner = null;
        }

        public void RegisterHandlers(CombatContext context)
        {
            _context = context;
            context.On<OnAbilityTriggerEvent>(OnAbilityTrigger);
        }

        public void UnregisterHandlers(CombatContext context)
        {
            context.Off<OnAbilityTriggerEvent>(OnAbilityTrigger);
            _context = null;
        }

        private void OnAbilityTrigger(OnAbilityTriggerEvent evt)
        {
            if (evt.Source != _owner) return;
            if (_isEchoing) return; // prevent the echo from echoing itself
            if (evt.Target == null || evt.Target.IsDead) return;
            if (_context == null) return;

            Log.Info("[SpellEcho] Echoing ability", new
            {
                owner = _owner.Name,
                target = evt.Target.Name,
                ability = evt.Ability.GetType().Name
            });

            _isEchoing = true;
            try
            {
                evt.Ability.OnCast(_owner, evt.Target, _context);
            }
            finally
            {
                _isEchoing = false;
            }
        }
    }
}
