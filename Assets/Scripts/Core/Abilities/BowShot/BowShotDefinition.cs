using Interfaces;
using UnityEngine;
using Utils;

namespace Core.Abilities
{
    /// <summary>
    ///     Single source of truth for the Bow Shot ability.
    ///     Holds both the balance config (damage, bleed stacks/duration/base-damage, cooldown) and the
    ///     upgrade-card behaviour (first-pickup creates the ability; subsequent pickups stack damage).
    ///     Tweak all values in the Unity Editor without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Bow Shot")]
    public class BowShotDefinition : AbilityDefinition
    {
        [Header("Bow Shot")]
        [Tooltip("Number of Bleed stacks applied to the target on hit.")]
        [Min(1)] [SerializeField] private int _bleedStacks = 2;
        [Tooltip("Duration (in turns) of the Bleed status effect applied to the target on hit.")]
        [Min(1)] [SerializeField] private int _bleedDuration = 3;
        [Tooltip("Base damage per Bleed stack per turn.")]
        [Min(0)] [SerializeField] private int _bleedBaseDamage = 2;
        [Tooltip("Sprite used for the projectile animation.")]
        [SerializeField] private Sprite _projectileSprite;

        public int BleedStacks => _bleedStacks;
        public int BleedDuration => _bleedDuration;
        public int BleedBaseDamage => _bleedBaseDamage;
        public Sprite ProjectileSprite => _projectileSprite;

        public override IAbility CreateRuntimeAbility() => new BowShot(this);

        public override void Apply(Unit unit)
        {
            Log.Info("Ability Applied: Bow Shot");

            var existing = FindExistingAbility<BowShot>(unit);
            if (existing != null)
                existing.AddDamage(DamagePerUpgrade);
            else
                unit.Abilities.Add(new BowShot(this));
        }

        public override void Upgrade(IAbility ability)
        {
            ability.Upgrade(DamagePerUpgrade);
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
                               int baseDamage = 8, int damagePerUpgrade = 3, int cooldownRounds = 0,
                               int bleedStacks = 2, int bleedDuration = 3, int bleedBaseDamage = 2)
        {
            id = identifier;
            displayName = soName;
            EditorInitAbility(baseDamage, damagePerUpgrade, cooldownRounds);
            _bleedStacks = bleedStacks;
            _bleedDuration = bleedDuration;
            _bleedBaseDamage = bleedBaseDamage;
        }
#endif
    }
}
