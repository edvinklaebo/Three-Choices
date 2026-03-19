using System;
using Core.Abilities.Definitions;
using Core.Combat;
using Interfaces;
using UnityEngine;

namespace Core.Abilities
{
    /// <summary>
    ///     Arcane Missiles ability that fires multiple missiles each turn.
    ///     Each missile is processed independently through the damage pipeline.
    ///     Implements <see cref="IActionCreator"/> to produce an <see cref="ArcaneMissilesAction"/>
    ///     (projectile animation) instead of the default lunge-based DamageAction.
    ///
    ///     Static config lives in <see cref="ArcaneMissilesData"/> (ScriptableObject).
    ///     This class owns all mutable runtime state: upgrade stacks, modifiers, cooldown counter.
    /// </summary>
    [Serializable]
    public class ArcaneMissiles : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _missileCount;
        [SerializeField] private Sprite _projectileSprite;
        // Runtime modifier — applied on top of _baseDamage without touching the SO.
        [SerializeField] private int _damageBonus;
        // Rounds remaining before this ability can fire again.
        [SerializeField] private int _currentCooldown;
        [SerializeField] private int _cooldownRounds;

        public const int DamagePerStack = 1;

        public int Priority => 40;

        /// <summary>Data-driven constructor: reads all config from an <see cref="ArcaneMissilesData"/> SO.</summary>
        public ArcaneMissiles(ArcaneMissilesData data)
        {
            Debug.Assert(data != null, "ArcaneMissiles: data must not be null");
            _baseDamage = data.BaseDamage;
            _missileCount = data.MissileCount;
            _projectileSprite = data.ProjectileSprite;
            _cooldownRounds = data.CooldownRounds;
        }

        /// <summary>Legacy constructor kept for backward-compatibility with tests and older code.</summary>
        public ArcaneMissiles(int baseDamage = 5, int missileCount = 3, Sprite projectileSprite = null)
        {
            _baseDamage = baseDamage;
            _missileCount = missileCount;
            _projectileSprite = projectileSprite;
        }

        /// <summary>Permanently increases per-missile damage. Called when the player picks up the ability again.</summary>
        public void AddDamage(int amount)
        {
            Debug.Assert(amount > 0, "AddDamage: amount must be positive");
            _baseDamage += amount;
        }

        /// <summary>
        ///     Adds one or more missiles to the salvo as an upgrade modifier.
        ///     Example: an upgrade card that says "Fire +1 extra missile" calls <c>missiles.AddMissile(1)</c>.
        /// </summary>
        public void AddMissile(int count = 1)
        {
            Debug.Assert(count > 0, "AddMissile: count must be positive");
            _missileCount += count;
        }

        /// <summary>
        ///     Adds a one-time runtime damage bonus per missile without modifying the ScriptableObject.
        ///     Use this from artifacts or other external systems.
        ///     Example: "Missiles deal +1 damage" artifact calls <c>missiles.AddDamageBonus(1)</c>.
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
            for (var i = 0; i < _missileCount; i++)
            {
                if (target.IsDead)
                    break;

                context.DealDamage(self, target, damage, actionCreator: this);
            }

            // Reset cooldown after casting.
            _currentCooldown = _cooldownRounds;
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new ArcaneMissilesAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);
    }
}