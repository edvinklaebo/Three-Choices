using Core.Abilities;
using UnityEngine;
using Utils;

/// <summary>Ability definition for Fireball. Stacks increase the base damage.</summary>
[CreateAssetMenu(menuName = "Upgrades/Abilities/Fireball")]
public class FireballDefinition : AbilityDefinition
{
    public override void Apply(Unit unit)
    {
        Log.Info("Ability Applied: Fireball");

        var existing = FindExistingAbility<Fireball>(unit);
        if (existing != null)
        {
            existing.AddDamage(Fireball.DamagePerStack);
            return;
        }

        unit.Abilities.Add(new Fireball(projectileSprite: ProjectileSprite));
    }
}
