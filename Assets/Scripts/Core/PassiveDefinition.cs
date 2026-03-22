using System;
using Core;
using Core.Passives;
using Core.StatusEffects;
using Interfaces;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Upgrades/Passive Definition")]
public class PassiveDefinition : UpgradeDefinition
{
    [Header("Passive")] 
    [SerializeField] private PassiveId passiveId;

    [Header("Status Effect Data")]
    [Tooltip("Balance data for the Bleed passive. Assign a BleedData asset to override the code defaults.")]
    [SerializeField] private BleedData _bleedData;

    [Tooltip("Balance data for the Poison passive. Assign a PoisonData asset to override the code defaults.")]
    [SerializeField] private PoisonData _poisonData;

    public override void Apply(Unit unit)
    {
        switch (passiveId)
        {
            case PassiveId.Thorns:
                Log.Info("Passive Applied: Thorns");
                var thorns = new Thorns();
                thorns.OnAttach(unit);
                unit.Passives.Add(thorns);
                break;

            case PassiveId.Rage:
                Log.Info("Passive Applied: Rage");
                var rage = new Rage(unit);
                rage.OnAttach(unit);
                unit.Passives.Add(rage);
                break;

            case PassiveId.Lifesteal:
                Log.Info("Passive Applied: Lifesteal");
                var ls = new Lifesteal(unit, 0.2f);
                ls.OnAttach(unit);
                unit.Passives.Add(ls);
                break;

            case PassiveId.Poison:
                Log.Info("Passive Applied: Poison");
                if (_poisonData == null)
                    Log.Warning($"PassiveDefinition '{displayName}': PoisonData not assigned — using code defaults.");
                var poison = _poisonData != null
                    ? new PoisonUpgrade(unit, _poisonData)
                    : new PoisonUpgrade(unit);
                poison.OnAttach(unit);
                unit.Passives.Add(poison);
                break;

            case PassiveId.Bleed:
                Log.Info("Passive Applied: Bleed");
                if (_bleedData == null)
                    Log.Warning($"PassiveDefinition '{displayName}': BleedData not assigned — using code defaults.");
                var bleed = _bleedData != null
                    ? new BleedUpgrade(unit, _bleedData)
                    : new BleedUpgrade(unit);
                bleed.OnAttach(unit);
                unit.Passives.Add(bleed);
                break;

            case PassiveId.DoubleStrike:
                Log.Info("Passive Applied: DoubleStrike");
                var doubleStrike = new DoubleStrike(0.25f, 0.75f);
                doubleStrike.OnAttach(unit);
                unit.Passives.Add(doubleStrike);
                break;

            default:
                throw new ArgumentOutOfRangeException(passiveId.ToString());
        }
    }

    public override void Upgrade(IAbility ability)
    {
        // I am cooking with some questionable shit here a bit but fuck it.
        throw new NotImplementedException();
    }

#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName, PassiveId passive)
    {
        id = identifier;
        displayName = soName;
        passiveId = passive;
    }

    public void EditorInit(string identifier, string soName, PassiveId passive,
                           BleedData bleedData, PoisonData poisonData)
    {
        id = identifier;
        displayName = soName;
        passiveId = passive;
        _bleedData = bleedData;
        _poisonData = poisonData;
    }
#endif
}