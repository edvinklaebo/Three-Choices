using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Core.Abilities
{
    /// <summary>
    ///     Base ScriptableObject for all ability configurations.
    ///     Stores STATIC data only (damage values, cooldown, tags).
    ///     Runtime state (cooldown counter, upgrade stacks, modifiers) belongs in the
    ///     <see cref="IAbility"/> instance returned by <see cref="CreateRuntimeAbility"/>.
    /// </summary>
    public abstract class AbilityData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [TextArea] [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;

        [Header("Damage")]
        [Tooltip("Base damage dealt per cast (or per missile for multi-hit abilities).")]
        [Min(1)] [SerializeField] private int _baseDamage = 3;
        [Tooltip("Damage added to the runtime instance each time the player upgrades this ability.")]
        [Min(1)] [SerializeField] private int _damagePerUpgrade = 1;

        [Header("Timing")]
        [Tooltip("0 = fires every turn. N = must wait N turns between casts.")]
        [Min(0)] [SerializeField] private int _cooldownRounds;

        [Header("Tags")]
        [SerializeField] private List<string> _tags = new();

        // ---- Public read-only accessors ----
        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public int BaseDamage => _baseDamage;
        public int DamagePerUpgrade => _damagePerUpgrade;
        public int CooldownRounds => _cooldownRounds;
        public IReadOnlyList<string> Tags => _tags;

        /// <summary>Creates a ready-to-use runtime ability seeded from this config.</summary>
        public abstract IAbility CreateRuntimeAbility();

#if UNITY_EDITOR
        public void EditorInitBase(int baseDamage, int damagePerUpgrade, int cooldownRounds)
        {
            _baseDamage = baseDamage;
            _damagePerUpgrade = damagePerUpgrade;
            _cooldownRounds = cooldownRounds;
        }

        public void EditorInitTags(string[] tags)
        {
            _tags = new List<string>(tags);
        }
#endif
    }
}
