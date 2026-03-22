using UnityEngine;

namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds all balance data for the Poison status effect.
    ///     Assign a PoisonDefinition asset wherever a Poison effect needs to be created
    ///     (e.g. PoisonPassiveDefinition, PoisonAmplifier artifact) so values can be
    ///     tuned in the Unity Inspector without touching runtime code.
    /// </summary>
    [CreateAssetMenu(menuName = "Status Effects/Poison Definition")]
    public class PoisonDefinition : ScriptableObject
    {
        [Tooltip("Number of stacks applied per hit.")]
        [Min(1)] [SerializeField] private int _stacks = 2;

        [Tooltip("Number of turns the effect lasts.")]
        [Min(1)] [SerializeField] private int _duration = 3;

        [Tooltip("Base damage applied per stack each turn.")]
        [Min(0)] [SerializeField] private int _baseDamage = 2;
        // Defaults above match the original hardcoded values from PoisonUpgrade (stacks=2, duration=3, baseDamage=2)
        // and PoisonAmplifier (bonusStacks=2, bonusDuration=3, bonusBaseDamage=2).

        public int Stacks => _stacks;
        public int Duration => _duration;
        public int BaseDamage => _baseDamage;

#if UNITY_EDITOR
        public void EditorInit(int stacks, int duration, int baseDamage)
        {
            _stacks = stacks;
            _duration = duration;
            _baseDamage = baseDamage;
        }
#endif
    }
}
