using Core.Passives;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Poison passive.
    ///     Applies a Poison status effect to any unit the owner hits.
    ///     Assign a <see cref="PoisonDefinition"/> asset to drive all balance values
    ///     from the Inspector. Falls back to code defaults when none is assigned.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Poison")]
    public class PoisonPassiveDefinition : PassiveDefinition
    {
        [Tooltip("Balance data for the Poison effect. Leave empty to use code defaults.")]
        [SerializeField] private PoisonDefinition _poisonDefinition;

        protected override IPassive CreatePassive(Unit unit)
            => _poisonDefinition != null
                ? new PoisonUpgrade(unit, _poisonDefinition)
                : new PoisonUpgrade(unit);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, PoisonDefinition poisonDefinition = null)
        {
            id = identifier;
            displayName = soName;
            _poisonDefinition = poisonDefinition;
        }
#endif
    }
}
