using System;
using UnityEngine;

namespace Core.Boss
{
    /// <summary>
    ///     Defines a single phase of a boss fight.
    ///     Phases must be ordered from highest to lowest <see cref="TriggerHPPercent"/>.
    ///     The first phase must have <see cref="TriggerHPPercent"/> set to 100.
    ///     Subsequent phases trigger when the boss HP drops below their threshold.
    /// </summary>
    [Serializable]
    public class BossPhaseDefinition
    {
        /// <summary>HP percent (0–100) at which this phase activates. First phase must be 100.</summary>
        [SerializeField] private int _triggerHPPercent;

        /// <summary>Abilities that become active when this phase is entered.</summary>
        [SerializeField] private BossAbilityDefinition[] _abilities;

        /// <summary>Seconds between attacks in this phase.</summary>
        [SerializeField] private float _attackInterval;

        public int TriggerHPPercent => _triggerHPPercent;
        public BossAbilityDefinition[] Abilities => _abilities;
        public float AttackInterval => _attackInterval;

#if UNITY_EDITOR
        public void EditorInit(int triggerHPPercent, float attackInterval, BossAbilityDefinition[] abilities = null)
        {
            _triggerHPPercent = triggerHPPercent;
            _attackInterval = attackInterval;
            _abilities = abilities ?? Array.Empty<BossAbilityDefinition>();
        }
#endif
    }
}
