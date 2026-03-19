using Interfaces;
using UnityEngine;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     ScriptableObject config for the Arcane Missiles ability.
    ///     Tweak damage, missile count, and cooldown in the Unity Editor without touching code.
    ///     At runtime, <see cref="CreateRuntimeAbility"/> creates an <see cref="ArcaneMissiles"/>
    ///     instance seeded from these values; the instance owns all mutable state.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Arcane Missiles Data")]
    public class ArcaneMissilesData : AbilityData
    {
        [Header("Arcane Missiles")]
        [Tooltip("Number of missiles fired per cast.")]
        [Min(1)] [SerializeField] private int _missileCount = 3;
        [Tooltip("Sprite used for the projectile animation.")]
        [SerializeField] private Sprite _projectileSprite;

        public int MissileCount => _missileCount;
        public Sprite ProjectileSprite => _projectileSprite;

        public override IAbility CreateRuntimeAbility() => new ArcaneMissiles(this);

#if UNITY_EDITOR
        public void EditorInit(int baseDamage, int damagePerUpgrade, int cooldownRounds, int missileCount)
        {
            EditorInitBase(baseDamage, damagePerUpgrade, cooldownRounds);
            _missileCount = missileCount;
        }
#endif
    }
}
