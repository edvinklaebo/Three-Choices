namespace Core.StatusEffects
{
    /// <summary>
    ///     ScriptableObject that holds display metadata AND balance data for the Weak status effect.
    ///     Inherits identity/display fields (Id, DisplayName, Description, Icon, Color) from
    ///     <see cref="StatusEffectDefinition"/>. Assign this asset wherever a Weak effect needs to
    ///     be created so all values can be tuned in the Unity Inspector without touching runtime code.
    ///     Each stack of Weak reduces the target's outgoing damage by <see cref="DamageReductionPerStack"/>.
    /// </summary>
    [UnityEngine.CreateAssetMenu(menuName = "Status Effects/Weak Definition")]
    public class WeakDefinition : StatusEffectDefinition
    {
        [UnityEngine.Header("Balance")]
        [UnityEngine.Tooltip("Number of stacks applied per application.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _stacks = 1;

        [UnityEngine.Tooltip("Number of turns the effect lasts.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _duration = 2;

        [UnityEngine.Tooltip("Flat damage reduction applied per stack when the weakened unit deals damage.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _damageReductionPerStack = 1;

        [UnityEngine.Tooltip("Maximum number of stacks that can be accumulated.")]
        [UnityEngine.Min(1)] [UnityEngine.SerializeField] private int _maxStacks = 10;

        [UnityEngine.Tooltip("Number of stacks removed at the start of each turn. Set to 0 to disable decay.")]
        [UnityEngine.Min(0)] [UnityEngine.SerializeField] private int _stackDecayPerTurn;

        [UnityEngine.Tooltip("When true, reapplying Weak refreshes the duration to the base value.")]
        [UnityEngine.SerializeField] private bool _refreshDurationOnReapply = true;

        public int Stacks => _stacks;
        public int Duration => _duration;
        public int DamageReductionPerStack => _damageReductionPerStack;
        public int MaxStacks => _maxStacks;
        public int StackDecayPerTurn => _stackDecayPerTurn;
        public bool RefreshDurationOnReapply => _refreshDurationOnReapply;

#if UNITY_EDITOR
        public void EditorInit(int stacks, int duration, int damageReductionPerStack,
            int maxStacks, int stackDecayPerTurn, bool refreshDurationOnReapply)
        {
            _stacks = stacks;
            _duration = duration;
            _damageReductionPerStack = damageReductionPerStack;
            _maxStacks = maxStacks;
            _stackDecayPerTurn = stackDecayPerTurn;
            _refreshDurationOnReapply = refreshDurationOnReapply;
        }
#endif
    }
}
