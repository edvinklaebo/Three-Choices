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


    private void OnDamaged(Unit self, Unit attacker, int damageTaken)
    {
        Log.Info("Applied thorns to "  + (attacker?.Name ?? "unknown"));
    }
}