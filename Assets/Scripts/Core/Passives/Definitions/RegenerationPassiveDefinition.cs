using Core.Passives;
using Core.StatusEffects;

using Interfaces;

using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Regeneration passive upgrade.
    ///     At the start of each fight the unit receives a <see cref="Regeneration"/>
    ///     status effect whose balance is driven by the assigned <see cref="RegenerationDefinition"/>
    ///     asset. Falls back to code defaults (3 stacks, 5 healing/stack) when none is assigned.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Regeneration")]
    public class RegenerationPassiveDefinition : PassiveDefinition
    {
        [Tooltip("Balance data for the Regeneration effect. Leave empty to use code defaults.")]
        [SerializeField] private RegenerationDefinition _regenerationDefinition;

        protected override IPassive CreatePassive(Unit unit)
            => _regenerationDefinition != null
                ? new RegenerationPassive(unit, _regenerationDefinition)
                : new RegenerationPassive(unit, stacks: 3, healingPerStack: 5);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
            RegenerationDefinition regenerationDefinition = null)
        {
            id = identifier;
            displayName = soName;
            _regenerationDefinition = regenerationDefinition;
        }
#endif
    }
}
