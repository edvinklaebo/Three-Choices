using UnityEngine;

/// <summary>
///     Trait that applies poison stacks to any unit this enemy hits.
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
        var stacks = _poisonStacks;
        var duration = _poisonDuration;
        var baseDamage = _poisonBaseDamage;

        unit.OnHit += (attacker, target, damage) =>
        {
            target.ApplyStatus(new Poison(stacks, duration, baseDamage));
        };
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
