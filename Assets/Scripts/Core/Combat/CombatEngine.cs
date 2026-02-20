using System;
using System.Collections.Generic;
using System.Linq;

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

        // Register armor mitigation as a global combat rule (applied in Mitigation phase)
        _context.RegisterListener(new ArmorMitigationModifier());

        // Register combat listeners from both units
        RegisterListeners(attacker);
        RegisterListeners(defender);

        Log.Info("Combat started", new
        {
            attacker = attacker.Name,
            defender = defender.Name,
            attackerHp = attacker.Stats.CurrentHP,
            defenderHp = defender.Stats.CurrentHP,
            attackerAtk = attacker.Stats.AttackPower,
            defenderAtk = defender.Stats.AttackPower,
            attackerArmor = attacker.Stats.Armor,
            defenderArmor = defender.Stats.Armor,
            attackerSpeed = attacker.Stats.Speed,
            defenderSpeed = defender.Stats.Speed
        });

        _attackerTurn = attacker.Stats.Speed >= defender.Stats.Speed;

        Log.Info("Turn order decided", new
        {
            first = _attackerTurn ? attacker.Name : defender.Name,
            reason = "speed",
            aSpeed = attacker.Stats.Speed,
            bSpeed = defender.Stats.Speed
        });
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

        Log.Info("Combat round start", new
        {
            round = _round,
            aHp = _attacker.Stats.CurrentHP,
            bHp = _defender.Stats.CurrentHP,
            turn = _attackerTurn ? _attacker.Name : _defender.Name
        });

        var acting = _attackerTurn ? _attacker : _defender;
        var target = _attackerTurn ? _defender : _attacker;

        TickStatusesTurnStart(acting);

        if (acting.isDead)
            return;

        // Trigger abilities at turn start (e.g., Fireball)
        TriggerAbilities(acting, target);

        if (target.isDead)
            return;

        // Execute attack with event-driven flow
        Attack(acting, target, _round);

        TickStatusesTurnEnd(acting);

        _attackerTurn = !_attackerTurn;
    }

    private List<ICombatAction> BuildResult()
    {
        var winner = _attacker.Stats.CurrentHP > 0 ? _attacker.Name : _defender.Name;

        Log.Info("Combat finished", new
        {
            winner,
            rounds = _round,
            finalAHp = _attacker.Stats.CurrentHP,
            finalBHp = _defender.Stats.CurrentHP
        });

        return _context.Actions.ToList();
    }

    private void RegisterListeners(Unit unit)
    {
        // Register passives that implement ICombatListener
        foreach (var passive in unit.Passives)
        {
            if (passive is ICombatListener listener)
            {
                _context.RegisterListener(listener);
            }
        }

        // Register abilities that implement ICombatListener
        foreach (var ability in unit.Abilities)
        {
            if (ability is ICombatListener listener)
            {
                _context.RegisterListener(listener);
            }
        }
    }

    private void Attack(Unit attacker, Unit defender, int round)
    {
        Log.Info("Attack start", new
        {
            round,
            attacker = attacker.Name,
            defender = defender.Name,
            attackerHp = attacker.Stats.CurrentHP,
            defenderHp = defender.Stats.CurrentHP
        });

        // Raise before attack event for pre-resolution listeners
        _context.Raise(new BeforeAttackEvent(attacker, defender));

        // Pass raw AttackPower — armor reduction is applied in the Mitigation phase by ArmorMitigationModifier
        _context.ResolveAttack(attacker, defender, attacker.Stats.AttackPower);

        // Raise AfterAttackEvent after full resolution so post-resolution effects (e.g. DoubleStrike) can react
        _context.Raise(new AfterAttackEvent(attacker, defender));
    }

    private void TickStatusesTurnStart(Unit unit)
    {
        if (!unit.StatusEffects.Any())
            return;

        for (var i = unit.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = unit.StatusEffects[i];

            var damage = effect.OnTurnStart(unit);

            if (damage > 0)
                _context.ApplyDamage(null, unit, damage, DamageSource.StatusEffect, effect.Id);

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(unit);
                unit.StatusEffects.RemoveAt(i);
            }
        }
    }

    private void TriggerAbilities(Unit source, Unit target)
    {
        if (!source.Abilities.Any())
            return;

        foreach (var ability in source.Abilities)
        {
            Log.Info("Triggering ability", new
            {
                source = source.Name,
                target = target.Name,
                ability = ability.GetType().Name
            });

            var hpBefore = target.Stats.CurrentHP;

            // Trigger the ability — returns damage without applying HP mutation
            var damage = ability.OnAttack(source, target);

            // Apply damage through context (handles action creation and death)
            if (damage > 0)
                _context.ApplyDamage(source, target, damage, DamageSource.Ability);

            var hpAfter = target.Stats.CurrentHP;

            // Let ability create its own actions if it implements IActionCreator
            if (ability is IActionCreator actionCreator)
            {
                actionCreator.CreateActions(_context, source, target, hpBefore, hpAfter);
            }
        }
    }

    private void TickStatusesTurnEnd(Unit unit)
    {
        if (!unit.StatusEffects.Any())
            return;

        for (var i = unit.StatusEffects.Count - 1; i >= 0; i--)
        {
            var effect = unit.StatusEffects[i];

            var damage = effect.OnTurnEnd(unit);

            if (damage > 0)
                _context.ApplyDamage(null, unit, damage, DamageSource.StatusEffect, effect.Id);

            // Remove expired effects
            if (effect.Duration <= 0)
            {
                effect.OnExpire(unit);
                unit.StatusEffects.RemoveAt(i);
            }
        }
    }

    public CombatContext Context => _context;
}
