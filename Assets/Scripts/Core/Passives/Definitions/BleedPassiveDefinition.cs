using Core.Passives;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Bleed passive.
    ///     Applies a Bleed status effect to any unit the owner hits.
    ///     Assign a <see cref="BleedDefinition"/> asset to drive all balance values
    ///     from the Inspector. Falls back to code defaults when none is assigned.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Bleed")]
    public class BleedPassiveDefinition : PassiveDefinition
    {
        [Tooltip("Balance data for the Bleed effect. Leave empty to use code defaults.")]
        [SerializeField] private BleedDefinition _bleedDefinition;

        protected override IPassive CreatePassive(Unit unit)
            => _bleedDefinition != null
                ? new BleedUpgrade(unit, _bleedDefinition)
                : new BleedUpgrade(unit);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, BleedDefinition bleedDefinition = null)
        {
            id = identifier;
            displayName = soName;
            _bleedDefinition = bleedDefinition;
        }
#endif
    }
}
