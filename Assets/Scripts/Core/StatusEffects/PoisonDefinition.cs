namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata AND balance data for the Poison status effect.
    ///     Inherits identity/display fields (Id, DisplayName, Description, Icon, Color) from
    ///     <see cref="StatusEffectDefinition"/>. Assign this asset wherever a Poison effect needs to
    ///     be created (e.g. PoisonPassiveDefinition, PoisonAmplifier artifact) so all values can be
    ///     tuned in the Unity Inspector without touching runtime code.
    /// </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Status Effects/Poison Definition")]
    public class PoisonDefinition : StatusEffectDefinition
    {
        [UnityEngine.Header("Balance")]
        [UnityEngine.Tooltip("Number of stacks applied per hit.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _stacks = 2;

        [UnityEngine.Tooltip("Number of turns the effect lasts.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _duration = 3;

        [UnityEngine.Tooltip("Base damage applied per stack each turn.")]
        [UnityEngine.Min(0)] [UnityEngine.SerializeField] private int _baseDamage = 2;
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
