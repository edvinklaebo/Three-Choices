using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Ability Definition")]
public class AbilityDefinition : UpgradeDefinition
{
    [SerializeField] private Sprite projectileSprite;
    
    [Header("Ability Upgrade")] 
    [SerializeField] private AbilityId abilityId;

    public override void Apply(Unit unit)
    {
        switch (abilityId)
        {
            case AbilityId.Fireball:
                Log.Info("Ability Applied: Fireball");
                var existingFireball = FindAbility<Fireball>(unit);
                if (existingFireball != null)
                    existingFireball.AddDamage(Fireball.DamagePerStack);
                else
                    unit.Abilities.Add(new Fireball(projectileSprite: projectileSprite));
                break;
            case AbilityId.ArcaneMissiles:
                Log.Info("Ability Applied: Arcane Missiles");
                var existingMissiles = FindAbility<ArcaneMissiles>(unit);
                if (existingMissiles != null)
                    existingMissiles.AddDamage(ArcaneMissiles.DamagePerStack);
                else
                    unit.Abilities.Add(new ArcaneMissiles(projectileSprite: projectileSprite));
                break;
            default:
                throw new ArgumentOutOfRangeException(abilityId.ToString());
        }
    }
    
    private static T FindAbility<T>(Unit unit) where T : class, IAbility
    {
        foreach (var ability in unit.Abilities)
            if (ability is T found)
                return found;
        return null;
    }
    
#if UNITY_EDITOR
    public void EditorInit(string identifier, string soName, AbilityId ability)
    {
        id = identifier;
        displayName = soName;
        abilityId = ability;
    }
#endif
}