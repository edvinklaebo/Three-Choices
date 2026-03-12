using Core;
using Core.Passives;
using UnityEngine;

/// <summary>
///     Trait that applies poison stacks to any unit this enemy hits.
///     Delegates to <see cref="PoisonUpgrade"/> so the behaviour stays in one place.
///     Create via the asset menu: Game/Enemies/Traits/Poison On Hit.
/// </summary>
[CreateAssetMenu(menuName = "Game/Enemies/Traits/Poison On Hit")]
public class PoisonOnHitTrait : EnemyTraitDefinition
{
    [SerializeField] private int _poisonStacks = 2;
    [SerializeField] private int _poisonDuration = 3;
    [SerializeField] private int _poisonBaseDamage = 1;

    public override void Apply(Unit unit)
    {
        var passive = new PoisonUpgrade(unit, _poisonStacks, _poisonDuration, _poisonBaseDamage);
        unit.Passives.Add(passive);
        passive.OnAttach(unit);
    }

#if UNITY_EDITOR
    public void EditorInit(int poisonStacks, int poisonDuration = 3, int poisonBaseDamage = 1)
    {
        _poisonStacks = poisonStacks;
        _poisonDuration = poisonDuration;
        _poisonBaseDamage = poisonBaseDamage;
    }
#endif
}
