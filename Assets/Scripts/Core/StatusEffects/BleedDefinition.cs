using UnityEngine;

namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds all balance data for the Bleed status effect.
    ///     Assign a BleedDefinition asset wherever a Bleed effect needs to be created
    ///     (e.g. BleedPassiveDefinition, BloodRitual artifact) so values can be
    ///     tuned in the Unity Inspector without touching runtime code.
    /// </summary>
    [CreateAssetMenu(menuName = "Status Effects/Bleed Definition")]
    public class BleedDefinition : ScriptableObject
    {
        [Tooltip("Number of stacks applied per hit.")]
        [Min(1)] [SerializeField] private int _stacks = 2;

        [Tooltip("Number of turns the effect lasts.")]
        [Min(1)] [SerializeField] private int _duration = 3;

        [Tooltip("Base damage applied per stack each turn.")]
        [Min(0)] [SerializeField] private int _baseDamage = 2;
        // Defaults above match the original hardcoded values from BleedUpgrade (stacks=2, duration=3, baseDamage=2)
        // and BloodRitual (stacks=2, duration=3, baseDamage=2).

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
