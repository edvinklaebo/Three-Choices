using System;

using Core.Combat;
using Core.StatusEffects;

using Interfaces;

using UnityEngine;

namespace Core.Passives
{
    /// <summary>
    ///     Regeneration passive: at the start of each fight, applies a
    ///     <see cref="Regeneration"/> status effect to the owner.
    ///     Balance values are supplied by <see cref="Definitions.RegenerationPassiveDefinition"/>.
    /// </summary>
    [Serializable]
    public class RegenerationPassive : IPassive, ICombatListener
    {
        [SerializeField] private int _stacks;
        [SerializeField] private int _healingPerStack;

        [NonSerialized] private Unit _owner;

        public RegenerationPassive(Unit owner, int stacks, int healingPerStack)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Debug.Assert(stacks > 0, "RegenerationPassive: stacks must be > 0");
            Debug.Assert(healingPerStack > 0, "RegenerationPassive: healingPerStack must be > 0");
            _stacks = stacks;
            _healingPerStack = healingPerStack;
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

        /// <summary>
        ///     Called at combat start. Applies the Regeneration status effect to the owner.
        /// </summary>
        public void RegisterHandlers(CombatContext context)
        {
            _owner?.ApplyStatus(new Regeneration(_stacks, _healingPerStack));
        }

        public void UnregisterHandlers(CombatContext context)
        {
            // No event subscriptions to clean up.
        }
    }
}
