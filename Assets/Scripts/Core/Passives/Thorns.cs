using System;

[Serializable]
public class Thorns : IPassive
{
    private Unit _owner;

    public Thorns(Unit owner)
    {
        _owner = owner;
        OnAttach(owner);
    }

    public void OnAttach(Unit owner)
    {
        owner.Damaged += OnDamaged;
    }

    public void OnDetach(Unit owner)
    {
        owner.Damaged -= OnDamaged;
    }


    private void OnDamaged(Unit attacker, int damageTaken)
    {
        if (attacker == null)
            return;

        var thornDamage = _owner.Stats.Armor / 4;

        attacker.ApplyDamage(_owner, thornDamage);
    }
}