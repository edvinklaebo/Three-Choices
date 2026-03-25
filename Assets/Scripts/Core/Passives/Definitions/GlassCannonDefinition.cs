using Core.Passives;

using Interfaces;

using UnityEngine;

namespace Core.Passives.Definitions
{
    /// <summary>
    ///     Definition for the Glass Cannon passive.
    ///     The unit deals <see cref="_outgoingBonus"/> increased damage
    ///     but takes <see cref="_incomingPenalty"/> increased damage.
    /// </summary>
    [CreateAssetMenu(menuName = "Upgrades/Passive/Glass Cannon")]
    public class GlassCannonDefinition : PassiveDefinition
    {
        [Tooltip("Fraction of bonus outgoing damage (e.g. 0.6 = +60%).")]
        [Range(0f, 2f)] [SerializeField] private float _outgoingBonus = 0.6f;

        [Tooltip("Fraction of extra incoming damage (e.g. 0.4 = +40%).")]
        [Range(0f, 2f)] [SerializeField] private float _incomingPenalty = 0.4f;

        protected override IPassive CreatePassive(Unit unit) =>
            new GlassCannon(unit, _outgoingBonus, _incomingPenalty);

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
            float outgoingBonus = 0.6f, float incomingPenalty = 0.4f)
        {
            id = identifier;
            displayName = soName;
            _outgoingBonus = outgoingBonus;
            _incomingPenalty = incomingPenalty;
        }
#endif
    }
}
