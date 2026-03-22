using Interfaces;
using UnityEngine;
using Utils;

namespace Core.Abilities
{
    /// <summary>
    ///     Single source of truth for the Shadow Bolt ability.
    ///     Holds both the balance config (damage, weak stacks/duration, cooldown) and the upgrade-card
    ///     behaviour (first-pickup creates the ability; subsequent pickups stack damage).
    ///     Tweak all values in the Unity Editor without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Shadow Bolt")]
    public class ShadowBoltDefinition : AbilityDefinition
    {
        [Header("Shadow Bolt")]
        [Tooltip("Number of Weak stacks applied to the target on hit.")]
        [Min(1)] [SerializeField] private int _weakStacks = 2;
        [Tooltip("Duration (in turns) of the Weak status effect applied to the target on hit.")]
        [Min(1)] [SerializeField] private int _weakDuration = 2;

        public int WeakStacks => _weakStacks;
        public int WeakDuration => _weakDuration;

        public override IAbility CreateRuntimeAbility() => new ShadowBolt(this);

        public override void Apply(Unit unit)
        {
            Log.Info("Ability Applied: Shadow Bolt");

            var existing = FindExistingAbility<ShadowBolt>(unit);
            if (existing != null)
                existing.AddDamage(DamagePerUpgrade);
            else
                unit.Abilities.Add(new ShadowBolt(this));
        }

        public override void Upgrade(IAbility ability)
        {
            ability.Upgrade(DamagePerUpgrade);
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
                               int baseDamage = 8, int damagePerUpgrade = 3, int cooldownRounds = 0,
                               int weakStacks = 2, int weakDuration = 2)
        {
            id = identifier;
            displayName = soName;
            EditorInitAbility(baseDamage, damagePerUpgrade, cooldownRounds);
            _weakStacks = weakStacks;
            _weakDuration = weakDuration;
        }
#endif
    }
}
