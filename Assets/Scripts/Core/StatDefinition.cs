using System;
using Core;
using Interfaces;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Stat Definition")]
public class StatDefinition : UpgradeDefinition
{
    [SerializeField] private int amount;

    [Header("Stat Upgrade")] [SerializeField]
    private StatType stat;

    public override void Apply(Unit unit)
    {
        switch (stat)
        {
            case StatType.MaxHP:
                unit.Stats.MaxHP += amount;
                unit.Stats.CurrentHP += amount;
                break;

            case StatType.AttackPower:
                unit.Stats.AttackPower += amount;
                break;

            case StatType.Armor:
                unit.Stats.Armor += amount;
                break;

            case StatType.Speed:
                unit.Stats.Speed += amount;
                break;

            default:
                throw new ArgumentOutOfRangeException(stat.ToString());
        }
    }

    public override void Upgrade(IAbility ability)
    {
        // We are throwing a bit
        throw new NotImplementedException();
    }

#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName, StatType type, int value)
    {
        id = identifier;
        displayName = soName;
        stat = type;
        amount = value;
    }
#endif
}