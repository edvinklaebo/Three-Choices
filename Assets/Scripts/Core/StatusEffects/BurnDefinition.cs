using UnityEngine;

namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds all balance data for the Burn status effect.
    ///     Assign a BurnDefinition asset wherever a Burn effect needs to be created
    ///     (e.g. BlazingTorch artifact) so values can be tuned in the Unity
    ///     Inspector without touching runtime code.
    ///     Note: Burn does not use stacks — it keeps the highest BaseDamage seen
    ///     and refreshes its duration on re-apply.
    /// </summary>
    [CreateAssetMenu(menuName = "Status Effects/Burn Definition")]
    public class BurnDefinition : ScriptableObject
    {
        [Tooltip("Number of turns the effect lasts (also used when refreshing on re-apply).")]
        [Min(1)] [SerializeField] private int _duration = 3;

        [Tooltip("Flat damage dealt each turn.")]
        [Min(1)] [SerializeField] private int _baseDamage = 4;
        // Defaults above match the original hardcoded values from BlazingTorch (burnDuration=3, burnDamage=4).

        public int Duration => _duration;
        public int BaseDamage => _baseDamage;

#if UNITY_EDITOR
        public void EditorInit(int duration, int baseDamage)
        {
            _duration = duration;
            _baseDamage = baseDamage;
        }
#endif
    }
}
