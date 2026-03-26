using Core.Passives;
using Interfaces;
using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Rage passive.
    ///     Increases damage output based on missing health percentage.
    ///     <see cref="_maxBonus"/> caps the bonus at full missing HP (default 1.0 = +100% at 0 HP).
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Rage")]
    public class RageDefinition : PassiveDefinition
    {
        [Tooltip("Maximum damage bonus when HP is at 0 (e.g. 1.0 = +100%). Scales linearly with missing HP.")]
        [Range(0f, 4f)] [SerializeField] private float _maxBonus = 1.0f;

        protected override IPassive CreatePassive(Unit unit) => new Rage(unit, _maxBonus);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName, float maxBonus = 1.0f)
        {
            id = identifier;
            displayName = soName;
            _maxBonus = maxBonus;
        }
#endif
    }
}
