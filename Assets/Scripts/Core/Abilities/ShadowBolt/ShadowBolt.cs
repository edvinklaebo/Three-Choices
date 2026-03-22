using System;
using Core.Combat;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;

namespace Core.Abilities
{
    /// <summary>
    ///     Shadow Bolt ability that fires once per cast.
    ///     Deals direct damage and applies the <see cref="Weak"/> status effect to the target.
    ///     Implements <see cref="IActionCreator"/> to produce a <see cref="ShadowBoltAction"/>
    ///     (projectile animation) instead of the default lunge-based DamageAction.
    ///
    ///     Static config lives in <see cref="ShadowBoltDefinition"/> (ScriptableObject).
    ///     This class owns all mutable runtime state: upgrade stacks, cooldown counter.
    /// </summary>
    [Serializable]
    public class ShadowBolt : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _weakStacks;
        [SerializeField] private int _weakDuration;
        [SerializeField] private Sprite _projectileSprite;
        [SerializeField] private int _currentCooldown;
        [SerializeField] private int _cooldownRounds;

        public const int DamagePerStack = 3;

        public int Priority => 30;

        /// <summary>Data-driven constructor: reads all config from a <see cref="ShadowBoltDefinition"/> SO.</summary>
        public ShadowBolt(ShadowBoltDefinition definition)
        {
            Debug.Assert(definition != null, "ShadowBolt: definition must not be null");
            _baseDamage = definition.BaseDamage;
            _weakStacks = definition.WeakStacks;
            _weakDuration = definition.WeakDuration;
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
                               _ => new Weak(_weakStacks, _weakDuration),
                               actionCreator: this);

            _currentCooldown = _cooldownRounds;
        }

        public void Upgrade(int value)
        {
            AddDamage(value);
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new ShadowBoltAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);

#if UNITY_EDITOR
        /// <summary>Creates a ShadowBolt for editor/test use without a full asset. Do not use in production code.</summary>
        public static ShadowBolt EditorCreate(int baseDamage = 8, int weakStacks = 2, int weakDuration = 2)
        {
            var definition = ScriptableObject.CreateInstance<ShadowBoltDefinition>();
            definition.EditorInit("_editor", "_editor", baseDamage, DamagePerStack, 0, weakStacks, weakDuration);
            return new ShadowBolt(definition);
        }
#endif
    }
}
