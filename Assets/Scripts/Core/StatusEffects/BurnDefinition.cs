namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata AND balance data for the Burn status effect.
    ///     Inherits identity/display fields (Id, DisplayName, Description, Icon, Color) from
    ///     <see cref="StatusEffectDefinition"/>. Assign this asset wherever a Burn effect needs to
    ///     be created (e.g. BlazingTorch artifact) so all values can be tuned in the Unity
    ///     Inspector without touching runtime code.
    ///     Note: Burn does not use stacks — it keeps the highest BaseDamage seen
    ///     and refreshes its duration on re-apply.
    /// </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Status Effects/Burn Definition")]
    public class BurnDefinition : StatusEffectDefinition
    {
        [UnityEngine.Header("Balance")]
        [UnityEngine.Tooltip("Number of turns the effect lasts (also used when refreshing on re-apply).")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _duration = 3;

        [UnityEngine.Tooltip("Flat damage dealt each turn.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _baseDamage = 4;

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
