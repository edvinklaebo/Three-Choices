using System;
using System.Collections.Generic;

using Interfaces;

namespace Core
{
    public class DamageContext
    {
        public readonly Unit Source;
        public readonly Unit Target;

        public int BaseValue;
        public int FinalValue;

        public bool IsCritical;

        // Phase-system fields: set and read by phase listeners and CombatContext.ResolveAttack
        public int ModifiedDamage;
        public int FinalDamage;
        public int PendingHealing;
        public readonly int PendingResourceGain;
        public readonly List<IStatusEffect> PendingStatuses = new();
        public bool Cancelled;

        public DamageContext(Unit source, Unit target, int value)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            BaseValue = value;
            FinalValue = value;
            ModifiedDamage = value;
            FinalDamage = value;
            PendingResourceGain = 0; // TODO implement resources
        }
    }
}