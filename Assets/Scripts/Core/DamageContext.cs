using System.Collections.Generic;

using Interfaces;

namespace Core
{
    public class DamageContext
    {
        public readonly Unit Source;

        public int BaseValue;
        public int FinalValue;

        public bool IsCritical;
        public Unit Target;

        // Phase-system fields: set and read by phase listeners and CombatContext.ResolveAttack
        public int ModifiedDamage;
        public int FinalDamage;
        public int PendingHealing;
        public int PendingResourceGain;
        public List<IStatusEffect> PendingStatuses = new();
        public bool Cancelled;

        public DamageContext(Unit source, Unit target, int value)
        {
            this.Source = source;
            this.Target = target;
            this.BaseValue = value;
            this.FinalValue = value;
            this.ModifiedDamage = value;
            this.FinalDamage = value;
        }
    }
}