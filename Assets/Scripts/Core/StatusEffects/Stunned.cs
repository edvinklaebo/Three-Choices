using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     Stunned status effect that causes the owner to skip their turn.
    ///     Each time a turn is skipped one stack is consumed. The effect expires when all stacks
    ///     are consumed. Default application is one stack.
    /// </summary>
    [Serializable]
    public class Stunned : IStatusEffect, ITurnSkipper
    {
        [SerializeField] private int _stacks;

        public Stunned(int stacks = 1)
        {
            Debug.Assert(stacks > 0, "Stunned: stacks must be > 0");
            _stacks = stacks;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="StunnedDefinition"/> ScriptableObject.</summary>
        public Stunned(StunnedDefinition data)
        {
            Debug.Assert(data != null, "Stunned: data must not be null");
            _stacks = data.Stacks;
        }

        public string Id => "Stunned";

        public int Stacks => _stacks;

        /// <summary>
        ///     Duration mirrors the remaining stacks: when all stacks are consumed the effect expires.
        /// </summary>
        public int Duration => _stacks;

        /// <summary>Stunned deals no direct damage; BaseDamage is always 0.</summary>
        public int BaseDamage => 0;

        public void OnApply(Unit target)
        {
            Log.Info("Stunned applied", new
            {
                target = target.Name,
                stacks = _stacks
            });
        }

        /// <summary>
        ///     Called by the CombatEngine's status tick at the end of the round.
        ///     Returns 0 (no tick damage). Stack consumption is handled via
        ///     <see cref="ConsumeTurnSkip"/> at the start of the acting unit's turn.
        /// </summary>
        public int OnTurnStart(Unit target)
        {
            return 0;
        }

        public int OnTurnEnd(Unit target)
        {
            return 0;
        }

        public void OnExpire(Unit target)
        {
            Log.Info("Stunned expired", new
            {
                target = target.Name
            });
        }

        public void AddStacks(IStatusEffect effect)
        {
            _stacks += effect.Stacks;

            Log.Info("Stunned stacks added", new
            {
                addedStacks = effect.Stacks,
                newStacks = _stacks
            });
        }

        /// <summary>
        ///     Consumes one stack of stun, causing the acting unit to skip their turn.
        ///     Returns true if a stack was consumed; false if no stacks remain.
        /// </summary>
        public bool ConsumeTurnSkip()
        {
            if (_stacks <= 0)
                return false;

            _stacks--;

            Log.Info("Stunned consumed turn skip", new
            {
                remainingStacks = _stacks
            });

            return true;
        }
    }
}
