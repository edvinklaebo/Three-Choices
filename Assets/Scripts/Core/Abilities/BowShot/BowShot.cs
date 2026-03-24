using System;
using Core.Combat;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;

namespace Core.Abilities
{
    /// <summary>
    ///     Bow Shot ability that fires once per cast.
    ///     Deals direct physical damage and applies the <see cref="Bleed"/> status effect to the target.
    ///     Implements <see cref="IActionCreator"/> to produce a <see cref="BowShotAction"/>
    ///     (projectile animation) instead of the default lunge-based DamageAction.
    ///
    ///     Static config lives in <see cref="BowShotDefinition"/> (ScriptableObject).
    ///     This class owns all mutable runtime state: upgrade stacks, cooldown counter.
    /// </summary>
    [Serializable]
    public class BowShot : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _bleedStacks;
        [SerializeField] private int _bleedDuration;
        [SerializeField] private int _bleedBaseDamage;
        [SerializeField] private Sprite _projectileSprite;
        [SerializeField] private int _currentCooldown;
        [SerializeField] private int _cooldownRounds;

        public const int DamagePerStack = 3;

        public int Priority => 30;

        /// <summary>Data-driven constructor: reads all config from a <see cref="BowShotDefinition"/> SO.</summary>
        public BowShot(BowShotDefinition definition)
        {
            Debug.Assert(definition != null, "BowShot: definition must not be null");
            _baseDamage = definition.BaseDamage;
            _bleedStacks = definition.BleedStacks;
            _bleedDuration = definition.BleedDuration;
            _bleedBaseDamage = definition.BleedBaseDamage;
            _projectileSprite = definition.ProjectileSprite;
            _cooldownRounds = definition.CooldownRounds;
        }

        /// <summary>Increases base damage. Called when the player picks up the ability again or applies a modifier.</summary>
        public void AddDamage(int amount)
        {
            Debug.Assert(amount > 0, "AddDamage: amount must be positive");
            _baseDamage += amount;
        }

        public void OnCast(Unit self, Unit target, CombatContext context)
        {
            if (_currentCooldown > 0)
            {
                _currentCooldown--;
                return;
            }

            if (target == null || target.IsDead)
                return;

            context.DealDamage(self, target, _baseDamage,
                               _ => new Bleed(_bleedStacks, _bleedDuration, _bleedBaseDamage),
                               actionCreator: this);

            _currentCooldown = _cooldownRounds;
        }

        public void Upgrade(int value)
        {
            AddDamage(value);
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new BowShotAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);

#if UNITY_EDITOR
        /// <summary>Creates a BowShot for editor/test use without a full asset. Do not use in production code.</summary>
        public static BowShot EditorCreate(int baseDamage = 8, int bleedStacks = 2, int bleedDuration = 3, int bleedBaseDamage = 2)
        {
            var definition = ScriptableObject.CreateInstance<BowShotDefinition>();
            definition.EditorInit("_editor", "_editor", baseDamage, DamagePerStack, 0, bleedStacks, bleedDuration, bleedBaseDamage);
            return new BowShot(definition);
        }
#endif
    }
}
