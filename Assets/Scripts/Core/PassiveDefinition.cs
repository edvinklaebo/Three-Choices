using System;
using UnityEngine;

/// <summary>
///     Upgrade effect that grants the unit a passive ability.
/// </summary>
[CreateAssetMenu(menuName = "Upgrades/Effects/Passive")]
public class PassiveDefinition : UpgradeEffectDefinition
{
    [SerializeField] private AbilityId _passiveId;

    [Header("Lifesteal")]
    [Range(0f, 1f)]
    [SerializeField] private float _lifestealPercent = 0.2f;

    [Header("Double Strike")]
    [Range(0f, 1f)]
    [SerializeField] private float _doubleStrikeChance = 0.25f;

    [Range(0f, 2f)]
    [SerializeField] private float _doubleStrikeDamageMultiplier = 0.75f;

    public AbilityId PassiveId => _passiveId;

    public override void Apply(Unit unit)
    {
        switch (_passiveId)
        {
            case AbilityId.Thorns:
                Log.Info("Passive Applied: Thorns");
                var thorns = new Thorns();
                thorns.OnAttach(unit);
                unit.Passives.Add(thorns);
                break;

            case AbilityId.Rage:
                // Rage registers itself with DamagePipeline rather than unit.Passives
                // because it needs to intercept damage events globally before they reach the unit.
                Log.Info("Passive Applied: Rage");
                var rage = new Rage(unit);
                DamagePipeline.Register(rage);
                break;

            case AbilityId.Lifesteal:
                Log.Info("Passive Applied: Lifesteal");
                var ls = new Lifesteal(unit, _lifestealPercent);
                ls.OnAttach(unit);
                unit.Passives.Add(ls);
                break;

            case AbilityId.Poison:
                Log.Info("Passive Applied: Poison");
                var poison = new PoisonUpgrade(unit);
                poison.OnAttach(unit);
                unit.Passives.Add(poison);
                break;

            case AbilityId.Bleed:
                Log.Info("Passive Applied: Bleed");
                var bleed = new BleedUpgrade(unit);
                bleed.OnAttach(unit);
                unit.Passives.Add(bleed);
                break;

            case AbilityId.DoubleStrike:
                Log.Info("Passive Applied: DoubleStrike");
                var doubleStrike = new DoubleStrike(_doubleStrikeChance, _doubleStrikeDamageMultiplier);
                doubleStrike.OnAttach(unit);
                unit.Passives.Add(doubleStrike);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_passiveId), _passiveId, null);
        }
    }

#if UNITY_EDITOR
    public void EditorInit(AbilityId passiveId, float lifestealPercent = 0.2f,
        float doubleStrikeChance = 0.25f, float doubleStrikeDamageMultiplier = 0.75f)
    {
        _passiveId = passiveId;
        _lifestealPercent = lifestealPercent;
        _doubleStrikeChance = doubleStrikeChance;
        _doubleStrikeDamageMultiplier = doubleStrikeDamageMultiplier;
    }
#endif
}
