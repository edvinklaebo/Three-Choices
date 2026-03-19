using System;
using Core.Abilities.Definitions;
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
    ///     Static config lives in <see cref="FireballData"/> (ScriptableObject).
    ///     This class owns all mutable runtime state: upgrade stacks, modifiers, cooldown counter.
    /// </summary>
    [Serializable]
    public class Fireball : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _burnDuration;
        [SerializeField] private float _burnDamagePercent;
        [SerializeField] private Sprite _projectileSprite;
        // Runtime modifier — applied on top of _baseDamage without touching the SO.
        [SerializeField] private int _damageBonus;
        // Rounds remaining before this ability can fire again.
        [SerializeField] private int _currentCooldown;
        [SerializeField] private int _cooldownRounds;

        public const int DamagePerStack = 5;

        public int Priority => 50;

        /// <summary>Data-driven constructor: reads all config from a <see cref="FireballData"/> SO.</summary>
        public Fireball(FireballData data)
        {
            Debug.Assert(data != null, "Fireball: data must not be null");
            _baseDamage = data.BaseDamage;
            _burnDuration = data.BurnDuration;
            _burnDamagePercent = data.BurnDamagePercent;
            _projectileSprite = data.ProjectileSprite;
            _cooldownRounds = data.CooldownRounds;
        }

        /// <summary>Legacy constructor kept for backward-compatibility with tests and older code.</summary>
        public Fireball(int baseDamage = 10, int burnDuration = 3, float burnDamagePercent = 0.5f, Sprite projectileSprite = null)
        {
            _baseDamage = baseDamage;
            _burnDuration = burnDuration;
            _burnDamagePercent = burnDamagePercent;
            _projectileSprite = projectileSprite;
        }

        /// <summary>Permanently increases base damage. Called when the player picks up the ability again.</summary>
        public void AddDamage(int amount)
        {
            Debug.Assert(amount > 0, "AddDamage: amount must be positive");
            _baseDamage += amount;
        }

        /// <summary>
        ///     Adds a one-time runtime damage bonus without modifying the ScriptableObject.
        ///     Use this from artifacts or other external systems.
        ///     Example: "Fireball deals +2 damage" artifact calls <c>fireball.AddDamageBonus(2)</c>.
        /// </summary>
        public void AddDamageBonus(int amount) => _damageBonus += amount;

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

            var damage = _baseDamage + _damageBonus;
            context.DealDamage(self, target, damage,
                               finalDamage => new Burn(_burnDuration, Mathf.CeilToInt(finalDamage * _burnDamagePercent)),
                               actionCreator: this);

            // Reset cooldown after casting.
            _currentCooldown = _cooldownRounds;
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new FireballAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);
    }
}