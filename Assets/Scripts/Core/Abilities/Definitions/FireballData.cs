using Interfaces;
using UnityEngine;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     ScriptableObject config for the Fireball ability.
    ///     Tweak damage, burn, and cooldown in the Unity Editor without touching code.
    ///     At runtime, <see cref="CreateRuntimeAbility"/> creates a <see cref="Fireball"/>
    ///     instance seeded from these values; the instance owns all mutable state.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Fireball Data")]
    public class FireballData : AbilityData
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

#if UNITY_EDITOR
        public void EditorInit(int baseDamage, int damagePerUpgrade, int cooldownRounds,
                               int burnDuration, float burnDamagePercent)
        {
            EditorInitBase(baseDamage, damagePerUpgrade, cooldownRounds);
            _burnDuration = burnDuration;
            _burnDamagePercent = burnDamagePercent;
        }
#endif
    }
}
