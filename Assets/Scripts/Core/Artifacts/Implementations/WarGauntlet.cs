using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// War Gauntlet effect.
    /// Adds flat bonus damage to every attack made by the owner.
    /// Implements IDamageModifier so DamagePipeline picks it up automatically from Unit.Artifacts.
    /// Applied early (priority 10) before percentage-based multipliers.
    /// </summary>
    [Serializable]
    public class WarGauntlet : IArtifact, IDamageModifier
    {
        [SerializeField] private int _bonusDamage;
        [NonSerialized] private Unit _owner;

        public int Priority => 10; // Early flat bonus, same as FlatDamageModifier

        public WarGauntlet(int bonusDamage = 7)
        {
            _bonusDamage = bonusDamage;
        }

        public void OnAttach(Unit owner)
        {
            _owner = owner;
        }

        public void OnDetach(Unit owner)
        {
            _owner = null;
        }

        public void Modify(DamageContext ctx)
        {
            if (ctx.Source != _owner) return;

            ctx.FinalValue += _bonusDamage;

            Log.Info("[WarGauntlet] Flat damage bonus applied", new
            {
                source = _owner.Name,
                bonus = _bonusDamage,
                finalDamage = ctx.FinalValue
            });
        }
    }
}
