using Core.Passives;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Thorns passive.
    ///     Reflects armor-based damage back to any unit that attacks the owner.
    ///     <see cref="_armorMultiplier"/> controls what fraction of the owner's armour is reflected
    ///     (default 0.5 = half armour).
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Thorns")]
    public class ThornsDefinition : PassiveDefinition
    {
        [Tooltip("Fraction of the owner's armour reflected as damage (e.g. 0.5 = half armour).")]
        [Range(0f, 2f)] [SerializeField] private float _armorMultiplier = 0.5f;

        protected override IPassive CreatePassive(Unit unit) => new Thorns(_armorMultiplier);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, float armorMultiplier = 0.5f)
        {
            id = identifier;
            displayName = soName;
            _armorMultiplier = armorMultiplier;
        }
#endif
    }
}
