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
    ///     This class owns all mutable runtime state: upgrade stacks, cooldown counter.
    /// </summary>
    [Serializable]
    public class ArcaneMissiles : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _missileCount;
        [SerializeField] private Sprite _projectileSprite;
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

        /// <summary>Increases per-missile damage. Called when the player picks up the ability again or applies a modifier.</summary>
        public void AddDamage(int amount)
        {
            Debug.Assert(amount > 0, "AddDamage: amount must be positive");
            _baseDamage += amount;
        }

        /// <summary>
        ///     Adds one or more missiles to the salvo.
        ///     Example: an upgrade card that says "Fire +1 extra missile" calls <c>missiles.AddMissile(1)</c>.
        /// </summary>
        public void AddMissile(int count = 1)
        {
            Debug.Assert(count > 0, "AddMissile: count must be positive");
            _missileCount += count;
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

            for (var i = 0; i < _missileCount; i++)
            {
                if (target.IsDead)
                    break;

                context.DealDamage(self, target, _baseDamage, actionCreator: this);
            }

            // Reset cooldown after casting.
            _currentCooldown = _cooldownRounds;
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new ArcaneMissilesAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, _projectileSprite);

#if UNITY_EDITOR
        /// <summary>Creates an ArcaneMissiles for editor/test use without a full asset. Do not use in production code.</summary>
        public static ArcaneMissiles EditorCreate(int baseDamage = 5, int missileCount = 3, Sprite projectileSprite = null)
        {
            var data = ScriptableObject.CreateInstance<ArcaneMissilesData>();
            data.EditorInit(baseDamage, DamagePerStack, 0, missileCount);
            return new ArcaneMissiles(data);
        }
#endif
    }
}