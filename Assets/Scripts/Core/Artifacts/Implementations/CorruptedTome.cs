using System;

using Interfaces;

using UnityEngine;

using Utils;

namespace Core.Artifacts.Passives
{
    /// <summary>
    /// Corrupted Tome effect.
    /// Applies a percentage damage multiplier to all attacks made by the owner.
    /// Implements IDamageModifier so DamagePipeline picks it up automatically from Unit.Artifacts.
    /// Applied at standard priority (100) alongside other percentage modifiers.
    /// </summary>
    [Serializable]
    public class CorruptedTome : IArtifact, IDamageModifier
    {
        [SerializeField] private float _multiplier;
        [NonSerialized] private Unit _owner;

        public int Priority => 100; // Standard priority

        public CorruptedTome(float multiplier = 1.3f)
        {
            _multiplier = multiplier;
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

            ctx.FinalValue = Mathf.CeilToInt(ctx.FinalValue * _multiplier);

            Log.Info("[CorruptedTome] Damage multiplied", new
            {
                source = _owner.Name,
                multiplier = _multiplier,
                finalDamage = ctx.FinalValue
            });
        }
    }
}
