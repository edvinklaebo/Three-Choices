using System;

using Core.Combat;

using Interfaces;

using Utils;

namespace Core.Passives
{
    /// <summary>
    /// Deals damage equal to a fraction of the owner's armour to any unit that attacks them.
    /// The fraction is controlled by <see cref="_armorMultiplier"/> (default 0.5 = half armour).
    /// Reflects only on direct attacks (attacker != null) to avoid triggering on status effects.
    /// Uses a re-entrance guard to prevent infinite reflect chains when both units carry Thorns.
    /// Reflects via <see cref="CombatContext.DealDamage"/> (through <see cref="OnHitEvent"/>) so a
    /// <see cref="ThornsAction"/> is created and the damage is displayed to the player.
    /// </summary>
    [Serializable]
    public class Thorns : IPassive, ICombatListener, IActionCreator
    {
        [SerializeField] private float _armorMultiplier;

        [NonSerialized] private Unit _owner;
        [NonSerialized] private CombatContext _context;
        [NonSerialized] private bool _isReflecting;

        public Thorns(float armorMultiplier = 0.5f)
        {
            UnityEngine.Debug.Assert(armorMultiplier > 0f, "Thorns: armorMultiplier must be > 0");
            _armorMultiplier = armorMultiplier;
        }

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

        // Called via the combat event bus when a hit lands. Routes thorn reflect through
        // DealDamage so a DamageAction is recorded and the damage is displayed.
        private void OnHit(OnHitEvent evt)
        {
            if (_owner == null)
                return;

            if (evt.Target != _owner)
                return;

            if (evt.Source == null)
                return;

            if (_isReflecting)
                return;

            var thornsDamage = UnityEngine.Mathf.CeilToInt(_owner.Stats.Armor * _armorMultiplier);
            if (thornsDamage <= 0)
                return;

            Log.Info($"Thorns reflect: {_owner.Name} dealt {thornsDamage} to {evt.Source.Name}");

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