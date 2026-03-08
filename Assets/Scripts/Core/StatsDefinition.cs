using System;
using UnityEngine;

/// <summary>
///     Upgrade effect that applies a flat bonus to one of the unit's base statistics.
/// </summary>
[CreateAssetMenu(menuName = "Upgrades/Effects/Stat")]
public class StatsDefinition : UpgradeEffectDefinition
{
    [SerializeField] private StatType _stat;
    [SerializeField] private int _amount;

    public StatType Stat => _stat;
    public int Amount => _amount;

    public override void Apply(Unit unit)
    {
        switch (_stat)
        {
            case StatType.MaxHP:
                unit.Stats.MaxHP += _amount;
                unit.Stats.CurrentHP += _amount;
                break;

            case StatType.AttackPower:
                unit.Stats.AttackPower += _amount;
                break;

            case StatType.Armor:
                unit.Stats.Armor += _amount;
                break;

            case StatType.Speed:
                unit.Stats.Speed += _amount;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_stat), _stat, null);
        }
    }

#if UNITY_EDITOR
    public void EditorInit(StatType stat, int amount)
    {
        _stat = stat;
        _amount = amount;
    }
#endif
}
