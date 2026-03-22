using System;
using Core.Combat;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;

namespace Core.Abilities
{
    /// <summary>
    ///     Fireball ability that triggers at turn start.
    ///     Deals damage that can crit, then applies burn scaled to the final damage dealt.
    ///     Burn cannot crit and does not stack.
    ///     Implements <see cref="IActionCreator"/> to produce a <see cref="FireballAction"/>
    ///     (projectile animation) instead of the default lunge-based DamageAction.
    ///
    ///     Static config lives in <see cref="FireballDefinition"/> (ScriptableObject).
    ///     This class owns all mutable runtime state: upgrade stacks, cooldown counter.
    /// </summary>
    [Serializable]
    public class Fireball : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _burnDuration;
        [SerializeField] private float _burnDamagePercent;
        [SerializeField] private Sprite _projectileSprite;
        [SerializeField] private int _currentCooldown;
        [SerializeField] private int _cooldownRounds;

        public const int DamagePerStack = 5;

        public int Priority => 50;

        /// <summary>Data-driven constructor: reads all config from a <see cref="FireballDefinition"/> SO.</summary>
        public Fireball(FireballDefinition definition)
        {
            Debug.Assert(definition != null, "Fireball: definition must not be null");
            _baseDamage = definition.BaseDamage;
            _burnDuration = definition.BurnDuration;
            _burnDamagePercent = definition.BurnDamagePercent;
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
            // Respect cooldown — decrement and skip if not ready.
            if (_currentCooldown > 0)
            {
                _currentCooldown--;
                return;
            }

            if (target == null || target.IsDead)
                return;

            context.DealDamage(self, target, _baseDamage,
                               finalDamage => new Burn(_burnDuration, Mathf.CeilToInt(finalDamage * _burnDamagePercent)),
                               actionCreator: this);

            // Reset cooldown after casting.
            _currentCooldown = _cooldownRounds;
        }

        public void Upgrade(int value)
        {
            AddDamage(value);
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new FireballAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);

#if UNITY_EDITOR
        /// <summary>Creates a Fireball for editor/test use without a full asset. Do not use in production code.</summary>
        public static Fireball EditorCreate(int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f, Sprite projectileSprite = null)
        {
            var definition = ScriptableObject.CreateInstance<FireballDefinition>();
            definition.EditorInit("_editor", "_editor", baseDamage, DamagePerStack, 0, burnDuration, burnDamagePercent);
            return new Fireball(definition);
        }
#endif
    }
}