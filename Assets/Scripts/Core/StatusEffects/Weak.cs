using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     Weak status effect that reduces the affected unit's outgoing damage.
    ///     Each stack reduces outgoing damage by <see cref="_damageReductionPerStack"/> (flat).
    ///     Stacks are capped at <see cref="_maxStacks"/>. Optionally decays stacks each turn.
    ///     Duration decrements each turn; the effect expires when it reaches zero.
    /// </summary>
    [Serializable]
    public class Weak : IStatusEffect, IDamageModifier
    {
        [SerializeField] private int _stacks;
        [SerializeField] private int _duration;
        [SerializeField] private int _damageReductionPerStack;
        [SerializeField] private int _maxStacks;
        [SerializeField] private int _stackDecayPerTurn;
        [SerializeField] private bool _refreshDurationOnReapply;
        [SerializeField] private int _baseDuration;

        public Weak(int stacks, int duration, int damageReductionPerStack = 1,
            int maxStacks = 10, int stackDecayPerTurn = 0, bool refreshDurationOnReapply = true)
        {
            Debug.Assert(stacks > 0, "Weak: stacks must be > 0");
            Debug.Assert(duration > 0, "Weak: duration must be > 0");
            Debug.Assert(damageReductionPerStack >= 0, "Weak: damageReductionPerStack must be >= 0");
            Debug.Assert(maxStacks > 0, "Weak: maxStacks must be > 0");

            _stacks = Math.Min(stacks, maxStacks);
            _duration = duration;
            _baseDuration = duration;
            _damageReductionPerStack = damageReductionPerStack;
            _maxStacks = maxStacks;
            _stackDecayPerTurn = stackDecayPerTurn;
            _refreshDurationOnReapply = refreshDurationOnReapply;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="WeakDefinition"/> ScriptableObject.</summary>
        public Weak(WeakDefinition data)
        {
            Debug.Assert(data != null, "Weak: data must not be null");

            _stacks = data.Stacks;
            _duration = data.Duration;
            _baseDuration = data.Duration;
            _damageReductionPerStack = data.DamageReductionPerStack;
            _maxStacks = data.MaxStacks;
            _stackDecayPerTurn = data.StackDecayPerTurn;
            _refreshDurationOnReapply = data.RefreshDurationOnReapply;
        }

        public string Id => "Weak";

        public int Stacks => _stacks;

        public int Duration => _duration;

        /// <summary>Weak deals no direct tick damage; BaseDamage is always 0.</summary>
        public int BaseDamage => 0;

        /// <summary>Standard priority — applied after armor penetration, before final multipliers.</summary>
        public int Priority => 100;

        public void OnApply(Unit target)
        {
            Log.Info("Weak applied", new
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
                Log.Info("Weak stacks decayed", new
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
            Log.Info("Weak expired", new
            {
                target = target.Name
            });
        }

        public void AddStacks(IStatusEffect effect)
        {
            _stacks = Math.Min(_maxStacks, _stacks + effect.Stacks);

            if (_refreshDurationOnReapply)
                _duration = _baseDuration;

            Log.Info("Weak stacks added", new
            {
                addedStacks = effect.Stacks,
                newStacks = _stacks,
                duration = _duration
            });
        }

        /// <summary>
        ///     Reduces the source unit's outgoing damage by <c>Stacks * DamageReductionPerStack</c>.
        ///     Final damage is clamped to a minimum of zero.
        /// </summary>
        public void Modify(DamageContext context)
        {
            var reduction = _stacks * _damageReductionPerStack;
            context.FinalValue = Math.Max(0, context.FinalValue - reduction);

            Log.Info("Weak reduced outgoing damage", new
            {
                source = context.Source?.Name,
                reduction,
                finalValue = context.FinalValue
            });
        }
    }
}
