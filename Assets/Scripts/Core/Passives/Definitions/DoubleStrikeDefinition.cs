using Core.Passives;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Double Strike passive.
    ///     Grants a chance to attack twice on each hit, with the second hit dealing
    ///     a fraction of the original damage.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Double Strike")]
    public class DoubleStrikeDefinition : PassiveDefinition
    {
        [Tooltip("Probability (0–1) that a second hit is triggered after each attack.")]
        [Range(0f, 1f)] [SerializeField] private float _triggerChance = 0.25f;

        [Tooltip("Damage multiplier applied to the second hit (0–2).")]
        [Range(0f, 2f)] [SerializeField] private float _damageMultiplier = 0.75f;

        protected override IPassive CreatePassive(Unit unit) => new DoubleStrike(_triggerChance, _damageMultiplier);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
                               float triggerChance = 0.25f, float damageMultiplier = 0.75f)
        {
            id = identifier;
            displayName = soName;
            _triggerChance = triggerChance;
            _damageMultiplier = damageMultiplier;
        }
#endif
    }
}
