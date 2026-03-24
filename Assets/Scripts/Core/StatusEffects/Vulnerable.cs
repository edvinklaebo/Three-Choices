using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     Vulnerable status effect that increases the affected unit's incoming damage.
    ///     Each stack increases incoming flat damage by <see cref="_damageIncreasePerStack"/>.
    ///     Stacks are capped at <see cref="_maxStacks"/>. Optionally decays stacks each turn.
    ///     Duration decrements each turn; the effect expires when it reaches zero.
    /// </summary>
    [Serializable]
    public class Vulnerable : IStatusEffect, IDamageModifier
    {
        [SerializeField] private int _stacks;
        [SerializeField] private int _duration;
        [SerializeField] private int _damageIncreasePerStack;
        [SerializeField] private int _maxStacks;
        [SerializeField] private int _stackDecayPerTurn;
        [SerializeField] private bool _refreshDurationOnReapply;
        [SerializeField] private int _baseDuration;

        public Vulnerable(int stacks, int duration, int damageIncreasePerStack = 1,
            int maxStacks = 10, int stackDecayPerTurn = 0, bool refreshDurationOnReapply = true)
        {
            Debug.Assert(stacks > 0, "Vulnerable: stacks must be > 0");
            Debug.Assert(duration > 0, "Vulnerable: duration must be > 0");
            Debug.Assert(damageIncreasePerStack >= 0, "Vulnerable: damageIncreasePerStack must be >= 0");
            Debug.Assert(maxStacks > 0, "Vulnerable: maxStacks must be > 0");

            _stacks = Math.Min(stacks, maxStacks);
            _duration = duration;
            _baseDuration = duration;
            _damageIncreasePerStack = damageIncreasePerStack;
            _maxStacks = maxStacks;
            _stackDecayPerTurn = stackDecayPerTurn;
            _refreshDurationOnReapply = refreshDurationOnReapply;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="VulnerableDefinition"/> ScriptableObject.</summary>
        public Vulnerable(VulnerableDefinition data)
        {
            Debug.Assert(data != null, "Vulnerable: data must not be null");

            _stacks = data.Stacks;
            _duration = data.Duration;
            _baseDuration = data.Duration;
            _damageIncreasePerStack = data.DamageIncreasePerStack;
            _maxStacks = data.MaxStacks;
            _stackDecayPerTurn = data.StackDecayPerTurn;
            _refreshDurationOnReapply = data.RefreshDurationOnReapply;
        }

        public string Id => "Vulnerable";

        public int Stacks => _stacks;

        public int Duration => _duration;

        /// <summary>Vulnerable deals no direct tick damage; BaseDamage is always 0.</summary>
        public int BaseDamage => 0;

        /// <summary>Standard priority — applied after armor penetration, before final multipliers.</summary>
        public int Priority => 100;

        public void OnApply(Unit target)
        {
            Log.Info("Vulnerable applied", new
            {
                target = target.Name,
                stacks = _stacks,
                duration = _duration
            });
        }

        public int OnTurnStart(Unit target)
        {
            _duration--;

            if (_stackDecayPerTurn > 0)
            {
                _stacks = Math.Max(0, _stacks - _stackDecayPerTurn);
                Log.Info("Vulnerable stacks decayed", new
                {
                    target = target.Name,
                    remainingStacks = _stacks,
                    remainingDuration = _duration
                });
            }

            return 0;
        }

        public int OnTurnEnd(Unit target)
        {
            return 0;
        }

        public void OnExpire(Unit target)
        {
            Log.Info("Vulnerable expired", new
            {
                target = target.Name
            });
        }

        public void AddStacks(IStatusEffect effect)
        {
            _stacks = Math.Min(_maxStacks, _stacks + effect.Stacks);

            if (_refreshDurationOnReapply)
                _duration = _baseDuration;

            Log.Info("Vulnerable stacks added", new
            {
                addedStacks = effect.Stacks,
                newStacks = _stacks,
                duration = _duration
            });
        }

        /// <summary>
        ///     Increases incoming damage to the target by <c>Stacks * DamageIncreasePerStack</c>.
        /// </summary>
        public void Modify(DamageContext context)
        {
            var increase = _stacks * _damageIncreasePerStack;
            context.FinalValue += increase;

            Log.Info("Vulnerable increased incoming damage", new
            {
                target = context.Target?.Name,
                increase,
                finalValue = context.FinalValue
            });
        }
    }
}
