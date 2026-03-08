using System;
using UnityEngine;

/// <summary>
///     Upgrade effect that grants or enhances an active ability on the unit.
/// </summary>
[CreateAssetMenu(menuName = "Upgrades/Effects/Ability")]
public class AbilityDefinition : UpgradeEffectDefinition
{
    [SerializeField] private AbilityId _abilityId;
    [SerializeField] private Sprite _projectileSprite;

    public AbilityId AbilityId => _abilityId;
    public Sprite ProjectileSprite => _projectileSprite;

    public override void Apply(Unit unit)
    {
        switch (_abilityId)
        {
            case AbilityId.Fireball:
                Log.Info("Ability Applied: Fireball");
                var existingFireball = FindAbility<Fireball>(unit);
                if (existingFireball != null)
                    existingFireball.AddDamage(Fireball.DamagePerStack);
                else
                    unit.Abilities.Add(new Fireball(projectileSprite: _projectileSprite));
                break;

            case AbilityId.ArcaneMissiles:
                Log.Info("Ability Applied: Arcane Missiles");
                var existingMissiles = FindAbility<ArcaneMissiles>(unit);
                if (existingMissiles != null)
                    existingMissiles.AddDamage(ArcaneMissiles.DamagePerStack);
                else
                    unit.Abilities.Add(new ArcaneMissiles(projectileSprite: _projectileSprite));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(_abilityId), _abilityId, null);
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
    public void EditorInit(AbilityId abilityId, Sprite projectileSprite = null)
    {
        _abilityId = abilityId;
        _projectileSprite = projectileSprite;
    }
#endif
}
