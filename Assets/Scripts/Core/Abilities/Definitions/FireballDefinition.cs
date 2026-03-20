using Interfaces;
using UnityEngine;
using Utils;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     Single source of truth for the Fireball ability.
    ///     Holds both the balance config (damage, burn, cooldown) and the upgrade-card
    ///     behaviour (first-pickup creates the ability; subsequent pickups stack damage).
    ///     Tweak all values in the Unity Editor without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Fireball")]
    public class FireballDefinition : AbilityDefinition
    {
        [Header("Fireball")]
        [Tooltip("How many turns the Burn status lasts.")]
        [Min(1)] [SerializeField] private int _burnDuration = 3;
        [Tooltip("Burn damage each tick as a fraction of the final damage dealt (0–1).")]
        [Range(0f, 1f)] [SerializeField] private float _burnDamagePercent = 0.5f;
        [Tooltip("Sprite used for the projectile animation.")]
        [SerializeField] private Sprite _projectileSprite;

        public int BurnDuration => _burnDuration;
        public float BurnDamagePercent => _burnDamagePercent;
        public Sprite ProjectileSprite => _projectileSprite;

        public override IAbility CreateRuntimeAbility() => new Fireball(this);

        public override void Apply(Unit unit)
        {
            Log.Info("Ability Applied: Fireball");

            var existing = FindExistingAbility<Fireball>(unit);
            if (existing != null)
                existing.AddDamage(DamagePerUpgrade);
            else
                unit.Abilities.Add(new Fireball(this));
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
                               int baseDamage = 10, int damagePerUpgrade = 5, int cooldownRounds = 0,
                               int burnDuration = 3, float burnDamagePercent = 0.5f)
        {
            id = identifier;
            displayName = soName;
            EditorInitAbility(baseDamage, damagePerUpgrade, cooldownRounds);
            _burnDuration = burnDuration;
            _burnDamagePercent = burnDamagePercent;
        }
#endif
    }
}
