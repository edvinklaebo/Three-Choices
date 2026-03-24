using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.StatusEffects
{
    /// <summary>
    ///     Regeneration status effect that heals the owner each round based on remaining stacks.
    ///     One stack is removed after each activation. The effect expires when all stacks are consumed.
    /// </summary>
    [Serializable]
    public class Regeneration : IStatusEffect
    {
        [SerializeField] private int _stacks;
        [SerializeField] private int _healingPerStack;

        public Regeneration(int stacks, int healingPerStack)
        {
            Debug.Assert(stacks > 0, "Regeneration: stacks must be > 0");
            Debug.Assert(healingPerStack > 0, "Regeneration: healingPerStack must be > 0");

            _stacks = stacks;
            _healingPerStack = healingPerStack;
        }

        /// <summary>Data-driven constructor: reads all config from a <see cref="RegenerationDefinition"/> ScriptableObject.</summary>
        public Regeneration(RegenerationDefinition data)
        {
            Debug.Assert(data != null, "Regeneration: data must not be null");

            _stacks = data.Stacks;
            _healingPerStack = data.HealingPerStack;
        }

        public string Id => "Regeneration";

        public int Stacks => _stacks;

        /// <summary>
        ///     Duration mirrors the remaining stacks: when all stacks are consumed the effect expires.
        /// </summary>
        public int Duration => _stacks;

        /// <summary>Regeneration deals no damage; BaseDamage is always 0.</summary>
        public int BaseDamage => 0;

        public void OnApply(Unit target)
        {
            Log.Info("Regeneration applied", new
            {
                target = target.Name,
                stacks = _stacks,
                healingPerStack = _healingPerStack
            });
        }

        public int OnTurnStart(Unit target)
        {
            var heal = _stacks * _healingPerStack;
            _stacks--;

            Log.Info("Regeneration ticking", new
            {
                target = target.Name,
                heal,
                remainingStacks = _stacks,
                healingPerStack = _healingPerStack
            });

            target.Heal(heal);

            return 0;
        }

        public int OnTurnEnd(Unit target)
        {
            return 0;
        }

        public void OnExpire(Unit target)
        {
            Log.Info("Regeneration expired", new
            {
                target = target.Name
            });
        }

        public void AddStacks(IStatusEffect effect)
        {
            _stacks += effect.Stacks;

            if (effect is Regeneration regen)
                _healingPerStack = Math.Max(_healingPerStack, regen._healingPerStack);

            Log.Info("Regeneration stacks added", new
            {
                addedStacks = effect.Stacks,
                newStacks = _stacks,
                healingPerStack = _healingPerStack
            });
        }
    }
}
