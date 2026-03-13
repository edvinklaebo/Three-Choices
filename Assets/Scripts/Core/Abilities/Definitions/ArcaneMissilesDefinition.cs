using Core.Abilities;
using UnityEngine;
using Utils;

/// <summary>Ability definition for Arcane Missiles. Stacks increase per-missile damage.</summary>
[CreateAssetMenu(menuName = "Upgrades/Abilities/Arcane Missiles")]
public class ArcaneMissilesDefinition : AbilityDefinition
{
    public override void Apply(Unit unit)
    {
        Log.Info("Ability Applied: Arcane Missiles");

        var existing = FindExistingAbility<ArcaneMissiles>(unit);
        if (existing != null)
        {
            existing.AddDamage(ArcaneMissiles.DamagePerStack);
            return;
        }

        unit.Abilities.Add(new ArcaneMissiles(projectileSprite: ProjectileSprite));
    }
}
