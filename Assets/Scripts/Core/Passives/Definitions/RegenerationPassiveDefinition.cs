using Core.Passives;
using Core.StatusEffects;

using Interfaces;

using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Regeneration passive upgrade.
    ///     At the start of each fight the unit receives a <see cref="Regeneration"/>
    ///     status effect that heals it for <see cref="_healingPerStack"/> HP per remaining
    ///     stack each round.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Regeneration")]
    public class RegenerationPassiveDefinition : PassiveDefinition
    {
        [Tooltip("Number of regeneration stacks applied at the start of each fight.")]
        [Range(1, 20)] [SerializeField] private int _stacks = 3;

        [Tooltip("Amount healed per stack each round.")]
        [Range(1, 50)] [SerializeField] private int _healingPerStack = 5;

        protected override IPassive CreatePassive(Unit unit) =>
            new RegenerationPassive(unit, _stacks, _healingPerStack);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
            int stacks = 3, int healingPerStack = 5)
        {
            id = identifier;
            displayName = soName;
            _stacks = stacks;
            _healingPerStack = healingPerStack;
        }
#endif
    }
}
