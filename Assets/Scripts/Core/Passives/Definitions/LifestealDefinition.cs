using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Lifesteal passive.
    ///     Heals the owner for a percentage of damage dealt.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Lifesteal")]
    public class LifestealDefinition : PassiveDefinition
    {
        [Tooltip("Fraction of damage dealt that is converted to healing (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float _lifeStealPercent = 0.2f;

        protected override IPassive CreatePassive(Unit unit) => new Lifesteal(unit, _lifeStealPercent);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, float lifeStealPercent = 0.2f)
        {
            id = identifier;
            displayName = soName;
            _lifeStealPercent = lifeStealPercent;
        }
#endif
    }
}
