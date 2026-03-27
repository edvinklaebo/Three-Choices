using Interfaces;
using Core.StatusEffects;
using UnityEngine;
using Utils;

namespace Core.Abilities
{
    /// <summary>
    ///     Single source of truth for the Bow Shot ability.
    ///     Holds both the balance config (damage, bleed definition, cooldown) and the
    ///     upgrade-card behavior (first-pickup creates the ability; subsequent pickups stack damage).
    ///     Assign a <see cref="BleedDefinition"/> asset to drive all bleed values from the Inspector.
    ///     Falls back to code defaults when none is assigned.
    ///     Tweak all values in the Unity Editor without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Bow Shot")]
    public class BowShotDefinition : AbilityDefinition
    {
        [Header("Bow Shot")]
        [Tooltip("Balance data for the Bleed effect. Leave empty to use code defaults.")]
        [SerializeField] private BleedDefinition _bleedDefinition;
        [Tooltip("Sprite used for the projectile animation.")]
        [SerializeField] private Sprite _projectileSprite;

        public BleedDefinition BleedDefinition => _bleedDefinition;
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
                               BleedDefinition bleedDefinition = null)
        {
            id = identifier;
            displayName = soName;
            EditorInitAbility(baseDamage, damagePerUpgrade, cooldownRounds);
            _bleedDefinition = bleedDefinition;
        }
#endif
    }
}
