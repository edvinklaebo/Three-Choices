using System;

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
    /// </summary>
    [Serializable]
    public class ArcaneMissiles : IAbility, IActionCreator
    {
        [SerializeField] private int _baseDamage;
        [SerializeField] private int _missileCount;
        [SerializeField] private Sprite _projectileSprite;

        public const int DamagePerStack = 1;

        public int Priority => 40;

        public ArcaneMissiles(int baseDamage = 5, int missileCount = 3, Sprite projectileSprite = null)
        {
            this._baseDamage = baseDamage;
            this._missileCount = missileCount;
            this._projectileSprite = projectileSprite;
        }

        public void AddDamage(int amount)
        {
            Debug.Assert(amount > 0, "AddDamage: amount must be positive");
            this._baseDamage += amount;
        }

        public void OnCast(Unit self, Unit target, CombatContext context)
        {
            if (target == null || target.IsDead)
                return;

            for (var i = 0; i < this._missileCount; i++)
            {
                if (target.IsDead)
                    break;

                context.DealDamage(self, target, this._baseDamage, actionCreator: this);
            }
        }

        public ICombatAction CreateAction(Unit source, Unit target, int finalDamage, int hpBefore, int hpAfter, int maxHP)
            => new ArcaneMissilesAction(source, target, finalDamage, hpBefore, hpAfter, maxHP, this._projectileSprite);
    }
}
