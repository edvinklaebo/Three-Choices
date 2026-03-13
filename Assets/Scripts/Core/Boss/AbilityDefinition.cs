using UnityEngine;

namespace Core.Boss
{
    /// <summary>
    ///     Base ScriptableObject for data-driven boss abilities.
    ///     Subclass this to implement specific ability behavior that activates when a boss enters a phase.
    /// </summary>
    public abstract class BossAbilityDefinition : ScriptableObject
    {
        [SerializeField] private string _abilityName;
        [TextArea] [SerializeField] private string _description;

        public string AbilityName => this._abilityName;
        public string Description => this._description;

        /// <summary>
        ///     Called when the boss enters the phase that contains this ability.
        ///     Implementations should register listeners, start coroutines, or modify boss state.
        /// </summary>
        public abstract void Activate(BossController boss);
    }
}
