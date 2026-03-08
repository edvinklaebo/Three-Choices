using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Passive Definition")]
public class PassiveDefinition : UpgradeDefinition
{
    [Header("Passive")] 
    [SerializeField] private PassiveId passiveId;
    
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
                DamagePipeline.Register(rage);
                break;

            case PassiveId.Lifesteal:
                Log.Info("Passive Applied: Lifesteal");
                var ls = new Lifesteal(unit, 0.2f);
                ls.OnAttach(unit);
                unit.Passives.Add(ls);
                break;

            case PassiveId.Poison:
                Log.Info("Passive Applied: Poison");
                var poison = new PoisonUpgrade(unit);
                poison.OnAttach(unit);
                unit.Passives.Add(poison);
                break;

            case PassiveId.Bleed:
                Log.Info("Passive Applied: Bleed");
                var bleed = new BleedUpgrade(unit);
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
            
#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName, PassiveId passive)
    {
        id = identifier;
        displayName = soName;
        passiveId = passive;
    }
#endif
}