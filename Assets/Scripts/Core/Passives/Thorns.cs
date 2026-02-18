using System;

[Serializable]
public class Thorns : Passive
{
    public Thorns(Unit owner)
    {
        Owner = owner;
        owner.Damaged += OnDamaged;
    }

    private void OnDamaged(Unit attacker, int damageTaken)
    {
        if (attacker == null)
            return;

        var thornDamage = Owner.Stats.Armor / 4;

        attacker.ApplyDamage(Owner, thornDamage);
    }
}