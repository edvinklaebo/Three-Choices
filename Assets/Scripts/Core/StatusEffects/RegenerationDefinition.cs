namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata AND balance data for the Regeneration status effect.
    ///     Inherits identity/display fields (Id, DisplayName, Description, Icon, Color) from
    ///     <see cref="StatusEffectDefinition"/>. Assign this asset wherever a Regeneration effect needs to
    ///     be created so all values can be tuned in the Unity Inspector without touching runtime code.
    /// </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Status Effects/Regeneration Definition")]
    public class RegenerationDefinition : StatusEffectDefinition
    {
        [UnityEngine.Header("Balance")]
        [UnityEngine.Tooltip("Number of stacks (charges) applied per application.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _stacks = 3;

        [UnityEngine.Tooltip("Amount healed per stack each round.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _healingPerStack = 5;

        public int Stacks => _stacks;
        public int HealingPerStack => _healingPerStack;

#if UNITY_EDITOR
        public void EditorInit(int stacks, int healingPerStack)
        {
            _stacks = stacks;
            _healingPerStack = healingPerStack;
        }
#endif
    }
}
