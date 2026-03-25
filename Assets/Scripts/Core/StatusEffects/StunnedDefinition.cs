namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata AND balance data for the Stunned status effect.
    ///     Inherits identity/display fields (Id, DisplayName, Description, Icon, Color) from
    ///     <see cref="StatusEffectDefinition"/>. Assign this asset wherever a Stunned effect needs to
    ///     be created so all values can be tuned in the Unity Inspector without touching runtime code.
    /// </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Status Effects/Stunned Definition")]
    public class StunnedDefinition : StatusEffectDefinition
    {
        [UnityEngine.Header("Balance")]
        [UnityEngine.Tooltip("Number of turns the unit will be stunned (one stack = one skipped turn).")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _stacks = 1;

        public int Stacks => _stacks;

#if UNITY_EDITOR
        public void EditorInit(int stacks)
        {
            _stacks = stacks;
        }
#endif
    }
}
