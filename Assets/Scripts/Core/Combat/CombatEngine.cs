using System;
using System.Collections.Generic;
using System.Linq;
using Core.Modifiers;
using Interfaces;
using Utils;

namespace Core.Combat
{
    /// <summary>
    /// Instance-based combat engine that uses event-driven architecture.
    /// Replaces static CombatSystem with extensible pattern.
    /// Not thread-safe: a single instance must not run concurrent fights.
    /// </summary>
    public class CombatEngine
    {
        private readonly CombatContext _context = new();

        private Unit _attacker;
        private Unit _defender;
        private bool _attackerTurn;
        private int _round;

        public List<ICombatAction> RunFight(Unit attacker, Unit defender)
        {
            Initialize(attacker, defender);

            try
            {
                while (!IsFinished())
                    ExecuteRound();

                return BuildResult();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "CombatEngine.RunFight failed", new
                {
                    attacker = _attacker.Name,
                    defender = _defender.Name,
                    round = _round,
                    attackerHp = _attacker.Stats.CurrentHP,
                    defenderHp = _defender.Stats.CurrentHP,
                    attackerTurn = _attackerTurn
                });

                throw;
            }
            finally
            {
                // Clean up listeners
                _context.Clear();
            }
        }

        private void Initialize(Unit attacker, Unit defender)
        {
            _attacker = attacker;
            _defender = defender;
            _round = 0;

            _context.Clear();
            DamagePipeline.Clear();

            // Register armor mitigation as a global combat rule (applied in Mitigation phase)
            _context.RegisterListener(new ArmorMitigationModifier());

            // Register combat listeners from both units
            RegisterListeners(attacker);
            RegisterListeners(defender);

            _attackerTurn = attacker.Stats.Speed >= defender.Stats.Speed;
        }

        private bool IsFinished()
        {
            // HP-based check is used between rounds so that units starting at 0 HP are handled correctly.
            // isDead-based checks inside ExecuteRound serve as within-round early exits when a unit
            // dies from a status effect or ability before the normal attack phase.
            return _attacker.Stats.CurrentHP <= 0 || _defender.Stats.CurrentHP <= 0;
        }

        private void ExecuteRound()
        {
            _round++;

            var acting = _attackerTurn ? _attacker : _defender;
            var target = _attackerTurn ? _defender : _attacker;

            TickStatusesTurnStart(acting, target);
            if (acting.IsDead || target.IsDead)
                return;

            TriggerAbilities(acting, target);

            if (acting.IsDead || target.IsDead)
                return;

            Attack(acting, target);
            if (acting.IsDead || target.IsDead)
                return;

            TickStatusesTurnEnd(acting, target);

            _attackerTurn = !_attackerTurn;
        }

        private List<ICombatAction> BuildResult()
        {
            return _context.Actions.ToList();
        }

        private void RegisterListeners(Unit unit)
        {
            foreach (var passive in unit.Passives)
            {
                if (passive is ICombatListener listener)
                    _context.RegisterListener(listener);

                if (passive is ICombatHandlerProvider provider)
                    _context.RegisterListener(provider.CreateCombatHandler(unit));
            }

            foreach (var artifact in unit.Artifacts)
            {
                if (artifact is ICombatListener listener)
                    _context.RegisterListener(listener);

                if (artifact is ICombatHandlerProvider provider)
                    _context.RegisterListener(provider.CreateCombatHandler(unit));
            }
        }

        private void Attack(Unit source, Unit target)
        {
            // Raise before attack event for pre-resolution listeners
            _context.Raise(new BeforeAttackEvent(source, target));

            // Pass raw AttackPower — armor reduction is applied in the Mitigation phase by ArmorMitigationModifier
            _context.DealDamage(source, target, source.Stats.AttackPower);

            // Raise AfterAttackEvent after full resolution so post-resolution effects (e.g. DoubleStrike) can react
            _context.Raise(new AfterAttackEvent(source, target));
        }

        private void TickStatusesTurnStart(Unit source, Unit target)
        {
            if (!source.StatusEffects.Any() && !target.StatusEffects.Any())
                return;

            // Apply statuses on source
            // ApplyStatus(source, target);
            
            // Apply statuses on target
            // ApplyStatus(target, source);
        }

        private void TriggerAbilities(Unit source, Unit target)
        {
            if (!source.Abilities.Any())
                return;

            foreach (var ability in source.Abilities)
            {
                if (target == null || target.IsDead) break;

                ability.OnCast(source, target, _context);
                _context.Raise(new OnAbilityTriggerEvent(source, target, ability));
            }
        }

        private void TickStatusesTurnEnd(Unit source, Unit target)
        {
            if (!source.StatusEffects.Any())
                return;

            // Apply statuses on source
            ApplyStatus(source, target);
            
            // Apply statuses on target
            ApplyStatus(target, source);
        }
        
        private void ApplyStatus(Unit owner, Unit damageSource)
        {
            for (var i = owner.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = owner.StatusEffects[i];

                var damage = effect.OnTurnStart(owner);

                if (damage > 0)
                    _context.DealDamage(damageSource, owner, damage, effectId: effect.Id);

                if (effect.Duration <= 0)
                {
                    effect.OnExpire(owner);
                    owner.StatusEffects.RemoveAt(i);
                }

                if (owner.IsDead)
                    return;
            }
        }
    }
}