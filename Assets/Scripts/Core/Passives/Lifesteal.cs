using UnityEngine;

public class Lifesteal : Passive
{
    private readonly float percent;

    public Lifesteal(Unit owner, float percent)
    {
        Owner = owner;
        this.percent = percent;

        owner.Damaged += OnDamageDealt;
    }

    private void OnDamageDealt(Unit target, int damage)
    {
        var heal = Mathf.CeilToInt(damage * percent);
        Owner.Heal(heal);
    }
}