using Interfaces;
using UnityEngine;
using Utils;

namespace Core.Abilities.Definitions
{
    /// <summary>
    ///     Single source of truth for the Arcane Missiles ability.
    ///     Holds both the balance config (damage, missile count, cooldown) and the upgrade-card
    ///     behavior (first-pickup creates the ability; subsequent pickups stack damage).
    ///     Tweak all values in the Unity Editor without touching code.
    /// </summary>
    [CreateAssetMenu(menuName = "Abilities/Arcane Missiles")]
    public class ArcaneMissilesDefinition : AbilityDefinition
    {
        [Header("Arcane Missiles")]
        [Tooltip("Number of missiles fired per cast.")]
        [Min(1)] [SerializeField] private int _missileCount = 3;
        [Tooltip("Sprite used for the projectile animation.")]
        [SerializeField] private Sprite _projectileSprite;

        public int MissileCount => _missileCount;
        public Sprite ProjectileSprite => _projectileSprite;

        public override IAbility CreateRuntimeAbility() => new ArcaneMissiles(this);

        public override void Apply(Unit unit)
        {
            Log.Info("Ability Applied: Arcane Missiles");

            var existing = FindExistingAbility<ArcaneMissiles>(unit);
            if (existing != null)
                Upgrade(existing);
            else
                unit.Abilities.Add(new ArcaneMissiles(this));
        }

        public override void Upgrade(IAbility ability)
        {
            ability.Upgrade(DamagePerUpgrade);
        }

#if UNITY_EDITOR
        public void EditorInit(string identifier, string soName,
                               int baseDamage = 5, int damagePerUpgrade = 1, int cooldownRounds = 0,
                               int missileCount = 3)
        {
            id = identifier;
            displayName = soName;
            EditorInitAbility(baseDamage, damagePerUpgrade, cooldownRounds);
            _missileCount = missileCount;
        }
#endif
    }
}
